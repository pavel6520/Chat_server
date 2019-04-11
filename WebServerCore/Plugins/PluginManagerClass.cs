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
		public static string[][] Modules = new string[][] { new string[] { "Controller", "controllers" }, new string[] { "Layout", "layouts" }, new string[] { "Static", "static" } };

		private DirectoryInfo baseDirectory;
		private FileSystemWatcher baseDirectoryWatcher;
		private ControllerTreeElement controllerTree;
		private Hashtable layoutsPlugins;
		private Hashtable staticPlugins;
		private readonly ILog Log;

		public PluginManagerClass(ref ILog log, string path) {
			Plugin.Log = Log = log;
			baseDirectory = new DirectoryInfo(path);
			if (!baseDirectory.Exists) {
				baseDirectory.Create();
			}
			PluginLoader.baseDirectory = baseDirectory;

			baseDirectoryWatcher = new FileSystemWatcher(baseDirectory.FullName);
			baseDirectoryWatcher.IncludeSubdirectories = true;
			baseDirectoryWatcher.Created += Watcher_Event;
			baseDirectoryWatcher.Deleted += Watcher_Deleted;
			baseDirectoryWatcher.Changed += Watcher_Changed;
			baseDirectoryWatcher.Renamed += Watcher_Renamed;
			baseDirectoryWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size;

			layoutsPlugins = _LoadPlugins("layouts", "Layout");
			staticPlugins = _LoadPlugins("static", "Static");

			controllerTree = _LoadControllerTree("", true);

			baseDirectoryWatcher.EnableRaisingEvents = true;
		}

		private void Watcher_Event(object sender, FileSystemEventArgs e) {
			_Watcher_Work(e.FullPath, e.ChangeType);
		}
		private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
			_Watcher_Work(e.FullPath, e.ChangeType);
		}
		private void Watcher_Changed(object sender, FileSystemEventArgs e) {
			_Watcher_Work(e.FullPath, e.ChangeType);
		}
		private void Watcher_Renamed(object sender, RenamedEventArgs e) {
			_Watcher_Work(e.FullPath, e.ChangeType, e.OldFullPath);
		}

		private void _Watcher_Work(string path, WatcherChangeTypes changeType, string nameOld = null) {
			string name;
			string module;
			bool fileAct;
			{
				int intTmp = baseDirectory.FullName.Length;
				path = path.Remove(0, intTmp);
				//try {
				intTmp = path.IndexOf(Path.DirectorySeparatorChar);
				module = path.Substring(0, intTmp);
				path = path.Remove(0, intTmp);
				if (path.LastIndexOf('.') > path.LastIndexOf(Path.DirectorySeparatorChar)) {
					intTmp = path.LastIndexOf(Path.DirectorySeparatorChar);
					path = path.Remove(intTmp, path.Length - intTmp);
					fileAct = true;
				}
				else {
					fileAct = false;
				}
				intTmp = path.LastIndexOf(Path.DirectorySeparatorChar);
				name = path.Substring(intTmp + 1);
				path = path.Remove(intTmp, path.Length - intTmp);
				if (nameOld != null) {
					intTmp = nameOld.LastIndexOf(Path.DirectorySeparatorChar);
					nameOld = nameOld.Remove(0, intTmp + 1);
				}
				//name = 
				Console.WriteLine("TESTOUT " + path + " " + name + " " + nameOld);
			}
			path = path.Replace('\\', '/');
			if (module == "controllers") {
				Queue<string> pathSplit = new Queue<string>((path + name).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
				//Console.WriteLine("TESTOUT " + fullPath + " " + changeType + " " + path);
				ControllerTreeElement tree = _SearchController(ref controllerTree, ref pathSplit, true);
				if (fileAct) {
					tree = (ControllerTreeElement)tree.elements[name];
					tree.Unload();
					tree.Load(ref baseDirectory);
				}
				else {
					switch (changeType) {
						case WatcherChangeTypes.Deleted: {
								tree.RemoveTree(name);
								break;
							}
						case WatcherChangeTypes.Created: {
								tree.elements.Add(name, _LoadControllerTree(path));
								break;
							}
						case WatcherChangeTypes.Renamed: {
								tree.RemoveTree(nameOld);
								tree.elements.Add(name, _LoadControllerTree($"{path}{Path.DirectorySeparatorChar}{name}"));
								break;
							}
					}
				}
			}
			else if (module == "static" || module == "layouts") {
				DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}{module}{Path.DirectorySeparatorChar}");
				DirectoryInfo workLD = new DirectoryInfo($"{baseDirectory.FullName}{module}Work{Path.DirectorySeparatorChar}");
				Plugin pluginTmp = null;
				if (fileAct) {
					pluginTmp = (Plugin)(module == "static" ? staticPlugins : layoutsPlugins)[name];
					pluginTmp.UnloadPlugin();
					pluginTmp._FileCompare(ref baseDirectory, module, Path.DirectorySeparatorChar + name);
					pluginTmp.LoadPlugin();
				}
				else {
					switch (changeType) {
						case WatcherChangeTypes.Deleted: {
								pluginTmp = (Plugin)(module == "static" ? staticPlugins : layoutsPlugins)[name];
								pluginTmp.UnloadPlugin();
								(module == "static" ? staticPlugins : layoutsPlugins).Remove(name);
								break;
							}
						case WatcherChangeTypes.Created: {
								DirectoryInfo workD = new DirectoryInfo($"{workLD.FullName}{name}{Path.DirectorySeparatorChar}");
								if (!workD.Exists) {
									workD.Create();
								}
								Plugin plugin = new Plugin(workD, name, module);
								plugin._FileCompare(ref baseDirectory, path, $"{Path.DirectorySeparatorChar}{name}");
								plugin.LoadPlugin();
								(module == "static" ? staticPlugins : layoutsPlugins).Add(name, plugin);
								break;
							}
						case WatcherChangeTypes.Renamed: {
								pluginTmp = (Plugin)(module == "static" ? staticPlugins : layoutsPlugins)[nameOld];
								pluginTmp.UnloadPlugin();
								(module == "static" ? staticPlugins : layoutsPlugins).Remove(nameOld);
								DirectoryInfo workD = new DirectoryInfo($"{workLD.FullName}{name}{Path.DirectorySeparatorChar}");
								if (!workD.Exists) {
									workD.Create();
								}
								Plugin plugin = new Plugin(workD, name, module);
								plugin._FileCompare(ref baseDirectory, path, $"{Path.DirectorySeparatorChar}{name}");
								plugin.LoadPlugin();
								(module == "static" ? staticPlugins : layoutsPlugins).Add(name, plugin);
								break;
							}
					}
				}
			}
			//}
			//catch { }
		}

		private Hashtable _LoadPlugins(string pathName, string type) {
			Hashtable hashtable = new Hashtable();
			DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}{pathName}{Path.DirectorySeparatorChar}");
			DirectoryInfo workLD = new DirectoryInfo($"{baseDirectory.FullName}{pathName}Work{Path.DirectorySeparatorChar}");
			if (!baseLD.Exists) {
				baseLD.Create();
			}
			if (!baseLD.Exists) {
				workLD.Create();
			}

			foreach (var item in baseLD.GetDirectories()) {
				DirectoryInfo baseD = new DirectoryInfo($"{baseLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				DirectoryInfo workD = new DirectoryInfo($"{workLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				if (!workD.Exists) {
					workD.Create();
				}
				//_FileCompare(ref baseD, ref workD);
				Plugin plugin = new Plugin(workD, item.Name, type);
				plugin._FileCompare(ref baseDirectory, pathName, $"{Path.DirectorySeparatorChar}{item.Name}");
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
			if (!workD.Exists) {
				workD.Create();
			}

			//_FileCompare(ref treeD, ref workD);

			if (!baseEl) {
				treeElement.Name = workD.Name;
				treeElement.FullPath = path;
				treeElement.plugin = new Plugin(workD, treeElement.Name, "Controller");
				treeElement.Load(ref baseDirectory);
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
				Directory.Delete($"{workD.FullName}{dirsWork[i]}", true);
			}
		}

		public void Work(ref HelperClass helper) {
			string path = $"{helper.Context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)}";
			Queue<string> pathSplit = new Queue<string>(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
			if (pathSplit.Count == 0) {
				pathSplit.Enqueue("index");
			}
			ControllerTreeElement tree = _SearchController(ref controllerTree, ref pathSplit);
			if (tree != null) {
				string action = pathSplit.Dequeue();
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
						ResentLayout(ref helper, ref controller, ref bufS, helper.Render.layout, true);
					}
					else {
						ResentContent(ref helper, ref controller, ref bufS);
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
			else if (!helper.isWebSocket) {
				try {
					FileInfo fi = new FileInfo($"{baseDirectory.FullName}public{Path.DirectorySeparatorChar}{path.Replace('/', Path.DirectorySeparatorChar)}");
					using (FileStream reader = new FileStream(fi.FullName, FileMode.Open)) {
						helper.Context.Response.ContentLength64 = reader.Length;
						helper.Context.Response.Headers.Add(HttpResponseHeader.CacheControl, "max-age=86400");
						reader.CopyTo(helper.Context.Response.OutputStream, 16384);
					}
					return;
				}
				catch {
					helper.Context.Response.StatusCode = 404;
				}
			}
		}

		private ControllerTreeElement _SearchController(ref ControllerTreeElement tree, ref Queue<string> pathSplit, bool returnRoot = false) {
			string action;
			if (pathSplit.Count == 0) {
				action = "index";
			}
			else {
				action = pathSplit.Dequeue().ToLower();
			}
			if (returnRoot && pathSplit.Count == 0) {
				pathSplit.Enqueue(action);
				return tree;
			}
			if (tree.isLoad && tree.Actions.Contains(action)) {
				pathSplit.Enqueue(action);
				return tree;
			}
			else {
				if (tree.elements.Contains(action)) {
					ControllerTreeElement treeElement = (ControllerTreeElement)tree.elements[action];
					return _SearchController(ref treeElement, ref pathSplit, returnRoot);
				}
				else {
					return null;
				}
			}
		}

		void ResentLayout(ref HelperClass helper, ref ControllerWorker controller, ref string bufS, string layoutName, bool content) {
			LayoutWorker layout = (LayoutWorker)((Plugin)layoutsPlugins[layoutName]).GetPluginRefObject();
			layout._SetHelper(ref helper);
			layout._Work();
			byte[] buf;
			EchoClass ec = layout._GetNextContent();
			while (ec.type != EchoClass.EchoType.End) {
				switch (ec.type) {
					case EchoClass.EchoType.String:
						bufS += (string)ec.param;
						break;
					case EchoClass.EchoType.Layout:
						ResentLayout(ref helper, ref controller, ref bufS, (string)ec.param, content);
						break;
					case EchoClass.EchoType.Content:
						if (content) {
							ResentContent(ref helper, ref controller, ref bufS);
						}
						break;
				}
				if (bufS.Length > 16000) {
					buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
					bufS = bufS.Remove(0, 16000);
					helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				}
				ec = layout._GetNextContent();
			}
		}

		void ResentContent(ref HelperClass helper, ref ControllerWorker controller, ref string bufS) {
			byte[] buf;
			EchoClass ec = controller._GetNextContent();
			while (ec.type != EchoClass.EchoType.End) {
				switch (ec.type) {
					case EchoClass.EchoType.String:
						bufS += (string)ec.param;
						break;
					case EchoClass.EchoType.Layout:
						ResentLayout(ref helper, ref controller, ref bufS, (string)ec.param, false);
						break;
				}
				while (bufS.Length > 16000) {
					buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
					bufS = bufS.Remove(0, 16000);
					helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				}
				ec = controller._GetNextContent();
			}
		}
	}
}