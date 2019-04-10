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
		private Hashtable _directoryWatchers;

		private DirectoryInfo baseDirectory;
		FileSystemWatcher baseDirectoryWatcher;
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
			_directoryWatchers = new Hashtable();
			foreach (var item in _directories) {
				string tmp = $"{baseDirectory.FullName}{item}{Path.DirectorySeparatorChar}";
				if (!Directory.Exists(tmp)) {
					Directory.CreateDirectory(tmp);
				}
			}
			baseDirectoryWatcher = new FileSystemWatcher(baseDirectory.FullName);
			baseDirectoryWatcher.IncludeSubdirectories = true;
			baseDirectoryWatcher.Created += Watcher_Event;
			baseDirectoryWatcher.Deleted += Watcher_Deleted;
			baseDirectoryWatcher.Changed += Watcher_Changed;
			baseDirectoryWatcher.Renamed += Watcher_Renamed;
			baseDirectoryWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size;

			layoutPlugins = _LoadPlugins("layouts", "Layout");
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
			_Watcher_Work(e.FullPath, e.ChangeType);
		}

		private void _Watcher_Work(string path, WatcherChangeTypes changeType) {
			int count = baseDirectory.FullName.Length;
			string fullPath = path.Remove(0, count);
			//try {
			count = fullPath.IndexOf(Path.DirectorySeparatorChar);
			string module = fullPath.Substring(0, count);
			fullPath = fullPath.Remove(0, count);
			bool fileDelete = false;
			if (fullPath.LastIndexOf('.') > fullPath.LastIndexOf(Path.DirectorySeparatorChar)) {
				count = fullPath.LastIndexOf(Path.DirectorySeparatorChar);
				fullPath = fullPath.Remove(count, fullPath.Length - count);
				fileDelete = true;
			}
			fullPath = fullPath.Replace('\\', '/');
			if (module == "controllers") {
				Queue<string> pathSplit = new Queue<string>(fullPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
				//Console.WriteLine("TESTOUT " + fullPath + " " + changeType + " " + path);
				switch (changeType) {
					case WatcherChangeTypes.Deleted: {
							ControllerTreeElement tree = _SearchController(ref controllerTree, ref pathSplit, true);
							if (fileDelete) {
								tree = (ControllerTreeElement)tree.elements[pathSplit.Dequeue()];
								tree.Unload();
								tree.Load(ref baseDirectory);
							}
							else {
								string pluginName = pathSplit.Dequeue();
								tree.RemoveTree(pluginName);
							}
							break;
						}
					case WatcherChangeTypes.Created: {
							ControllerTreeElement tree = _SearchController(ref controllerTree, ref pathSplit, true);
							string pluginName = pathSplit.Dequeue();
							if (tree.elements.ContainsKey(pluginName)) {
								tree = (ControllerTreeElement)tree.elements[pluginName];
								tree.Unload();
								tree.Load(ref baseDirectory);
							}
							else {
								tree.elements.Add(pluginName, _LoadControllerTree(fullPath));
							}
							break;
						}
					case WatcherChangeTypes.Changed: {
							ControllerTreeElement tree = _SearchController(ref controllerTree, ref pathSplit, true);
							if (fileDelete) {
								tree = (ControllerTreeElement)tree.elements[pathSplit.Dequeue()];
								tree.Unload();
								tree.Load(ref baseDirectory);
								//Console.WriteLine("TESTOUT1 " + fullPath + " " + changeType + " " + tree.FullPath);
							}
							break;
						}
					case WatcherChangeTypes.Renamed: {
							ControllerTreeElement tree = _SearchController(ref controllerTree, ref pathSplit, true);
							if (fileDelete) {
								tree = (ControllerTreeElement)tree.elements[pathSplit.Dequeue()];
								tree.Unload();
								tree.Load(ref baseDirectory);
							}
							break;
						}
				}
			}
			else if (module == "static") {

			}
			else if (module == "layouts") {

			}
			//}
			//catch { }
		}

		private Hashtable _LoadPlugins(string pathName, string type) {
			Hashtable hashtable = new Hashtable();
			DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}{pathName}{Path.DirectorySeparatorChar}");
			DirectoryInfo workLD = new DirectoryInfo($"{baseDirectory.FullName}{pathName}Work{Path.DirectorySeparatorChar}");

			foreach (var item in baseLD.GetDirectories()) {
				DirectoryInfo baseD = new DirectoryInfo($"{baseLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				DirectoryInfo workD = new DirectoryInfo($"{workLD.FullName}{item.Name}{Path.DirectorySeparatorChar}");
				if (!workD.Exists) {
					workD.Create();
				}
				_FileCompare(ref baseD, ref workD);
				Plugin plugin = new Plugin(workD, item.Name, type);
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
				Directory.Delete($"{workD.FullName}{dirsWork[i]}", true);
			}
		}

		public void HttpContextWork(ref HelperClass helper) {
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
						//reader.CopyTo(helper.Context.Response.OutputStream, 8192);
						//reader.CopyTo(helper.Context.Response.OutputStream, 4096);
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
			LayoutWorker layout = (LayoutWorker)((Plugin)layoutPlugins[layoutName]).GetPluginRefObject();
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