using ConnectionWorker;
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Plugins {
	public partial class PluginManagerClass {
		private static string[] _directories = new string[] { "controllers", "controllersWork", "static", "staticWork", "layouts", "layoutsWork" };

		private DirectoryInfo baseDirectory;
        private ControllerTreeElement controllerTree;
		private Hashtable layoutPlugins;
		private readonly ILog Log;

        public PluginManagerClass(ref ILog log, string path) {
            Plugin.Log = Log = log;
            baseDirectory = new DirectoryInfo(path);
            if (!baseDirectory.Exists) {
                baseDirectory.Create();
            }
			foreach(var item in _directories) {
				string tmp = $"{baseDirectory.FullName}{item}";
				if (!Directory.Exists(tmp)) {
					Directory.CreateDirectory(tmp);
				}
			}
			layoutPlugins = _LoadLayouts();
			
			controllerTree = _LoadControllerTree("", true);
		}

		private Hashtable _LoadLayouts() {
			Hashtable hashtable = new Hashtable();
			DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}layouts{Path.DirectorySeparatorChar}");
			DirectoryInfo workLD = new DirectoryInfo($"{baseDirectory.FullName}layoutsWork{Path.DirectorySeparatorChar}");

			foreach(var item in baseLD.GetDirectories()) {
				DirectoryInfo baseD = new DirectoryInfo($"{baseLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				DirectoryInfo workD = new DirectoryInfo($"{workLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				if (!workD.Exists) {
					workD.Create();
				}
				_FileCompare(ref baseD, ref workD);
				Plugin plugin = new Plugin(workD, item.Name, "Layout");
				plugin.LoadPlugin();
				if (plugin.isLoad) {
					hashtable.Add(item.Name, plugin);
				}
			}
			return hashtable;
		}

		private ControllerTreeElement _LoadControllerTree(string path, bool baseEl = false) {
			ControllerTreeElement treeElement = new ControllerTreeElement();
			DirectoryInfo treeD = new DirectoryInfo($"{baseDirectory.FullName}controllers{path}{Path.DirectorySeparatorChar}");
			DirectoryInfo workD = new DirectoryInfo($"{baseDirectory.FullName}controllersWork{path}{Path.DirectorySeparatorChar}");

			_FileCompare(ref treeD, ref workD);

			if (!baseEl) {
				treeElement.Name = workD.Name;
				treeElement.FullPath = path;
				treeElement.plugin = new Plugin(workD, treeElement.Name, "Controller");
				treeElement.Load();
				if (treeElement.isLoad) {
					ControllerWorker controller = (ControllerWorker)treeElement.plugin.GetPluginRefObject();
					treeElement.Actions = controller._GetActionList();
					//treeElement.Settings
				}
			}

			treeElement.elements = _LoadControllerTrees(ref treeD, ref workD, path);
			return treeElement;
		}

		private Hashtable _LoadControllerTrees(ref DirectoryInfo treeD, ref DirectoryInfo workD, string path, bool baseDir = false) {
			Hashtable hashtable = new Hashtable();

			_DirCompare(ref treeD, ref workD);

			foreach (var dirInTree in treeD.GetDirectories()) {
				ControllerTreeElement treeElement = _LoadControllerTree($"{path}{Path.DirectorySeparatorChar}{dirInTree.Name}");
				hashtable.Add(dirInTree.Name, treeElement);
			}

			return hashtable;
		}

		private void _FileCompare(ref DirectoryInfo treeD, ref DirectoryInfo workD) {
			List<string> filesWork = new List<string>();
			foreach (var item in workD.GetFiles()) {
				filesWork.Add(item.Name);
			}
			foreach (var item in treeD.GetFiles()) {
				string workF = $"{workD.FullName}{item.Name}";
				if (filesWork.Contains(item.Name)) {
					filesWork.Remove(item.Name);
					if (File.GetLastWriteTimeUtc(workF) < item.LastWriteTimeUtc) {
						item.CopyTo(workF, true);
					}
				}
				else {
					item.CopyTo(workF, true);
				}
			}
			foreach (var item in filesWork) {
				File.Delete($"{workD.FullName}{item}");
			}
		}

		private void _DirCompare(ref DirectoryInfo treeD, ref DirectoryInfo workD) {
			List<string> dirsWork = new List<string>();
			foreach (var item in workD.GetDirectories()) {
				dirsWork.Add(item.Name);
			}
			foreach (var item in treeD.GetDirectories()) {
				if (dirsWork.Contains(item.Name)) {
					dirsWork.Remove(item.Name);
				}
				else {
					Directory.CreateDirectory($"{workD.FullName}{item.Name}");
				}
			}
			for (int i = 0; i < dirsWork.Count; i++) {
				Directory.Delete($"{workD.FullName}{dirsWork[i]}");
			}
		}
		
        public void HttpContextWork(ref HttpListenerContext context) {
            string path = $"/{context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)}";
            Queue<string> pathSplit = new Queue<string>(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            if (pathSplit.Count == 0) {
                pathSplit.Enqueue("index");
            }
            try {
				ConnectionWorker.Helpers.HelperClass helper = new ConnectionWorker.Helpers.HelperClass(ref context);
				
				_HttpContextWork(ref controllerTree, ref helper, ref pathSplit);
            }
            catch (PathNotFoundException) {
                try {
                    FileInfo fi = new FileInfo($"{baseDirectory.FullName}public{Path.DirectorySeparatorChar}{path.Replace('/', Path.DirectorySeparatorChar)}");
                    using (FileStream reader = new FileStream(fi.FullName, FileMode.Open)) {
                        context.Response.ContentLength64 = reader.Length;
                        reader.CopyTo(context.Response.OutputStream, 16384);
                    }
                    return;
                }
                catch { }
                throw;
            }
        }
		
		private void _HttpContextWork(ref ControllerTreeElement tree, ref ConnectionWorker.Helpers.HelperClass helper, ref Queue<string> pathSplit) {
			string action;
			if (pathSplit.Count == 0) {
				action = "index";
			}
			else {
				action = pathSplit.Dequeue().ToLower();
			}
			if (tree.isLoad && tree.Actions.Contains(action)) {
				HttpListenerContext context = helper.Context;
				ControllerWorker worker = (ControllerWorker)tree.plugin.GetPluginRefObject();
				worker._SetContext(helper);
				try {
					worker._Work(action);
				}
				catch { throw; }
				byte[] buf;
				context.Response.ContentType = "text/html; charset=UTF-8";
				context.Response.StatusDescription = "OK";
				EchoClass ec;
				buf = Encoding.UTF8.GetBytes("<html><body>");
				string bufS = "";
				bool end = true;
				while (end) {
					if (bufS.Length > 16000) {
						buf = Encoding.UTF8.GetBytes(bufS);
						bufS = "";
						helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
					}
					ec = worker._GetNextContent();
					switch (ec.type) {
						case EchoClass.EchoType.String:
							bufS += (string)ec.param;
							break;
						case EchoClass.EchoType.Layout:
							break;
						case EchoClass.EchoType.End:
							end = false;
							buf = Encoding.UTF8.GetBytes(bufS);
							helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
							break;
					}
				}

				buf = Encoding.UTF8.GetBytes($"</body></html>");
				context.Response.OutputStream.Write(buf, 0, buf.Length);
			}
			else {
				ControllerTreeElement treeElement = (ControllerTreeElement)tree.elements[action];
				if (treeElement != null) {
					_HttpContextWork(ref treeElement, ref helper, ref pathSplit);
				}
				else {
					throw new PathNotFoundException("");
				}
			}
		}
	}
}