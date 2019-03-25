using log4net;
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
        //private ControllerTreeElement controllerTreeTMP;
		//private Hashtable layoutPlugins;
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
			//layoutPlugins = _LoadLayouts();
			
			controllerTree = _LoadControllerTree("", true);
			//controllerTreeTMP = _LoadControllerTreeTMP("", true);
		}

		//private Hashtable _LoadLayouts() {
		//	Hashtable hashtable = new Hashtable();
		//	DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}layouts{Path.DirectorySeparatorChar}");
		//	DirectoryInfo workLD = new DirectoryInfo($"{baseDirectory.FullName}layoutsWork{Path.DirectorySeparatorChar}");

		//	foreach(var item in baseLD.GetDirectories()) {
		//		Plugin plugin = new Plugin(item, new DirectoryInfo($"{baseDirectory.FullName}layoutsWork{Path.DirectorySeparatorChar}{item.Name}"), item.Name, "Layout");
		//		if (plugin.isLoad) {
		//			hashtable.Add(item.Name, plugin);
		//		}
		//	}
		//	return hashtable;
		//}

		//private Hashtable _LoadControllerTreesTMP(ref DirectoryInfo parentDir, string path, bool baseDir = false) {
		//	Hashtable hashtable = new Hashtable();
			
			
		//	foreach (var dirInTree in parentDir.GetDirectories()) {
		//		ControllerTreeElement treeElement = _LoadControllerTreeTMP(path);
		//		if (treeElement.plugin.isLoad || treeElement.elementsTMP.Count > 0) {
		//			hashtable.Add(dirInTree.Name, treeElement);
		//		}
		//	}

		//	return hashtable;
		//}

		//private ControllerTreeElement _LoadControllerTreeTMP(string path, bool baseEl = false) {
		//	ControllerTreeElement treeElement = new ControllerTreeElement();

		//	DirectoryInfo treeD = new DirectoryInfo($"{baseDirectory.FullName}controllers{path}{Path.DirectorySeparatorChar}");

		//	//Plugin plugin = new Plugin();

		//	return treeElement;
		//}






		private ControllerTreeElement _LoadControllerTree(string path, bool baseObject = false) {
			ControllerTreeElement treeEl = new ControllerTreeElement();
			DirectoryInfo baseD = new DirectoryInfo($"{baseDirectory.FullName}controllers{path}{Path.DirectorySeparatorChar}");
			DirectoryInfo workD = new DirectoryInfo($"{baseDirectory.FullName}controllersWork{path}{Path.DirectorySeparatorChar}");
			if (!workD.Exists) {
				workD.Create();
			}

			if (!baseObject) {
				Plugin plugin = new Plugin(baseD, workD, baseD.Name, "Controller");
				if (plugin.isLoad) {
					Log.Info($"Загружен контроллер {path}");
					Console.WriteLine($"Загружен контроллер {path}");
					treeEl.plugin = plugin;
				}
			}

			treeEl.elements = _LoadControllerTrees(ref treeEl, ref baseD, ref path);
			return treeEl;
		}

		private List<ControllerTreeElement> _LoadControllerTrees(ref ControllerTreeElement treeEl, ref DirectoryInfo baseD, ref string path) {
			List<ControllerTreeElement> trees = new List<ControllerTreeElement>();
			foreach (var item in baseD.GetDirectories()) {
				if (treeEl.plugin != null && treeEl.plugin.isLoad && treeEl.plugin.Actions.Contains($"{item.Name}Action")) {
					Log.Error($"Directory load error: Action exists: {path}{Path.DirectorySeparatorChar}{item.Name}");
				}
				else {
					trees.Add(_LoadControllerTree($"{path}{Path.DirectorySeparatorChar}{item.Name}"));
				}
			}
			return trees;
		}

        public void HttpContextWork(ref HttpListenerContext context) {
            string path = $"/{context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)}";
            Queue<string> pathSplit = new Queue<string>(
				path.Split(new char[] { '?' }, StringSplitOptions.RemoveEmptyEntries)[0]
				.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            if (pathSplit.Count == 0) {
                pathSplit.Enqueue("index");
            }
            try {
				ConnectionWorker.Helpers.HelperClass helper = new ConnectionWorker.Helpers.HelperClass(ref context);

				_HttpContextWork(controllerTree, ref helper, ref pathSplit);
            }
            catch (PathNotFoundException) {
                try {
                    FileInfo fi = new FileInfo($"{baseDirectory.FullName}public{Path.DirectorySeparatorChar}{path.Replace('/', Path.DirectorySeparatorChar)}");
                    using (FileStream reader = new FileStream(fi.FullName, FileMode.Open)) {
                        context.Response.ContentLength64 = reader.Length;
                        reader.CopyTo(context.Response.OutputStream, 4096);
                    }
                    return;
                }
                catch { }
                throw;
            }
        }

		private void _HttpContextWork(ControllerTreeElement tree, ref ConnectionWorker.Helpers.HelperClass helper, ref Queue<string> pathSplit) {
			string action;
			if (pathSplit.Count == 0) {
				action = "index";
			}
			else {
				action = pathSplit.Dequeue().ToLower();
			}
			if (tree.plugin != null && tree.plugin.isLoad && tree.plugin.Actions.Contains(action)) {
				ConnectionWorker.PluginWorker worker = tree.plugin.GetCrossDomainObject();
				worker._SetContext(helper);
				try {
					worker._Work(action);
				}
				catch { throw; }
				byte[] buf;
				helper.Context.Response.ContentType = "text/html; charset=UTF-8";
				helper.Context.Response.StatusDescription = "OK";
				
				string s = "<html><body>";
				while (s != null) {
					buf = Encoding.UTF8.GetBytes(s);
					helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
					s = worker._GetNextContentString();
				}

				buf = Encoding.UTF8.GetBytes($"</body></html>");
				helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
			}
			else {
				for (int i = 0; i < tree.elements.Count; i++) {
					if (tree.elements[i].plugin.Name == action) {
						_HttpContextWork(tree.elements[i], ref helper, ref pathSplit);
						return;
					}
				}
				throw new PathNotFoundException("");
			}
		}
    }
}