using ConnectionWorker;
using ConnectionWorker.Helpers;
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
		private Hashtable staticPlugins;
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
			staticPlugins = _LoadStatic();
			
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

		private Hashtable _LoadStatic() {
			Hashtable hashtable = new Hashtable();
			DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}static{Path.DirectorySeparatorChar}");
			DirectoryInfo workLD = new DirectoryInfo($"{baseDirectory.FullName}staticWork{Path.DirectorySeparatorChar}");

			foreach (var item in baseLD.GetDirectories()) {
				DirectoryInfo baseD = new DirectoryInfo($"{baseLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				DirectoryInfo workD = new DirectoryInfo($"{workLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				if (!workD.Exists) {
					workD.Create();
				}
				_FileCompare(ref baseD, ref workD);
				Plugin plugin = new Plugin(workD, item.Name, "S" +
					"tatic");
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
		
        public void HttpContextWork(ref HelperClass helper) {
			//var SW = new System.Diagnostics.Stopwatch();
			//SW.Start();
			string path = $"/{helper.Context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)}";
            Queue<string> pathSplit = new Queue<string>(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            if (pathSplit.Count == 0) {
                pathSplit.Enqueue("index");
            }
            try {
				_HttpContextWork(ref controllerTree, ref helper, ref pathSplit);
				//SW.Stop();
				//Console.WriteLine($"TIME_Controller {Convert.ToString(SW.ElapsedMilliseconds)}   {helper.Context.Request.Url.OriginalString}");
			}
            catch (PathNotFoundException) {
				if (!helper.isWebSocket) {
					try {
						//SW.Stop();
						//Console.WriteLine($"TIME_Controller {Convert.ToString(SW.ElapsedMilliseconds)}   {helper.Context.Request.Url.OriginalString}");
						//SW.Start();
						//Console.WriteLine(path);
						//string tmp = $"{baseDirectory.FullName}public{path.Replace('/', Path.DirectorySeparatorChar)}";
						//Console.WriteLine(tmp);
						FileInfo fi = new FileInfo($"{baseDirectory.FullName}public{path.Replace('/', Path.DirectorySeparatorChar)}");
						using (FileStream reader = new FileStream(fi.FullName, FileMode.Open)) {
							helper.Context.Response.ContentLength64 = reader.Length;
							reader.CopyTo(helper.Context.Response.OutputStream, 16384);
							//reader.CopyTo(helper.Context.Response.OutputStream, 8192);
							//reader.CopyTo(helper.Context.Response.OutputStream, 4096);
						}
						//SW.Stop();
						//Console.WriteLine($"TIME_FILE {Convert.ToString(SW.ElapsedMilliseconds)}   {helper.Context.Request.Url.OriginalString}");
						return;
					}
					catch { }
				}
                throw;
			}
		}

		private void _HttpContextWork(ref ControllerTreeElement tree, ref HelperClass helper, ref Queue<string> pathSplit) {
			string action;
			if (pathSplit.Count == 0) {
				action = "index";
			}
			else {
				action = pathSplit.Dequeue().ToLower();
			}
			if (tree.isLoad && tree.Actions.Contains(action)) {
				if (!helper.isWebSocket) {
					HttpListenerContext context = helper.Context;

					ControllerWorker controller = (ControllerWorker)tree.plugin.GetPluginRefObject();
					try {
						string[] staticInclude = controller._GetStaticInclude();
						if (staticInclude != null) {
							helper.staticPlugins = new Hashtable();
							for (int i = 0; i < staticInclude.Length; i++) {
								helper.staticPlugins.Add(staticInclude[i], ((Plugin)staticPlugins[staticInclude[i]]).GetPluginRefObject());
							}
						}

						controller._SetHelper(ref helper);
						controller._Work(action);
					}
					catch { throw; }

					helper = controller._GetHelper();

					context.Response.Headers.Add(helper.Responce.Headers);

					if (helper.returnType == ReturnType.Content) {
						context.Response.ContentType = "text/html; charset=UTF-8";
						context.Response.StatusDescription = "OK";

						string bufS = "";
						if (helper.Render.isEnabled) {
							ResentLayout(ref context, ref controller, ref bufS, helper.Render.layout, true);
						}
						else {
							ResentContent(ref context, ref controller, ref bufS);
						}
						if (bufS.Length > 0) {
							byte[] buf = Encoding.UTF8.GetBytes(bufS);
							context.Response.OutputStream.Write(buf, 0, buf.Length);
							buf = null;
							bufS = null;
						}
					}
					else {
						context.Response.Redirect(helper.RedirectLocation);
					}
				}
				else {

				}
			}
			else {
				if (tree.elements.Contains(action)) {
					ControllerTreeElement treeElement = (ControllerTreeElement)tree.elements[action];
					_HttpContextWork(ref treeElement, ref helper, ref pathSplit);
				}
				else {
					throw new PathNotFoundException("");
				}
			}
		}

		void ResentLayout(ref HttpListenerContext context, ref ControllerWorker controller, ref string bufS, string layoutName, bool content) {
			LayoutWorker layout = (LayoutWorker)((Plugin)layoutPlugins[layoutName]).GetPluginRefObject();
			layout._Work();
			byte[] buf;
			EchoClass ec = layout._GetNextContent();
			while (ec.type != EchoClass.EchoType.End) {
				switch (ec.type) {
					case EchoClass.EchoType.String:
						bufS += (string)ec.param;
						break;
					case EchoClass.EchoType.Layout:
						ResentLayout(ref context, ref controller, ref bufS, (string)ec.param, content);
						break;
					case EchoClass.EchoType.Content:
						if (content) {
							ResentContent(ref context, ref controller, ref bufS);
						}
						break;
				}
				if (bufS.Length > 16000) {
					buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
					bufS = bufS.Remove(0, 16000);
					context.Response.OutputStream.Write(buf, 0, buf.Length);
				}
				ec = layout._GetNextContent();
			}
		}

		void ResentContent(ref HttpListenerContext context, ref ControllerWorker controller, ref string bufS) {
			byte[] buf;
			EchoClass ec = controller._GetNextContent();
			while (ec.type != EchoClass.EchoType.End) {
				switch (ec.type) {
					case EchoClass.EchoType.String:
						bufS += (string)ec.param;
						break;
					case EchoClass.EchoType.Layout:
						ResentLayout(ref context, ref controller, ref bufS, (string)ec.param, false);
						break;
				}
				while (bufS.Length > 16000) {
					buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
					bufS = bufS.Remove(0, 16000);
					context.Response.OutputStream.Write(buf, 0, buf.Length);
				}
				ec = controller._GetNextContent();
			}
		}
	}
}