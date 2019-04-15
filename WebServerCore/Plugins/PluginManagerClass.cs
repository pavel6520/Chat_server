using ConnectionWorker;
using ConnectionWorker.Helpers;
using log4net;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Net;
using WebSocketSharp.Net.WebSockets;

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
			ControllerTreeElement.Module = Modules[0];
			baseDirectory = new DirectoryInfo(path);
			if (!baseDirectory.Exists) {
				baseDirectory.Create();
			}
			foreach(var item in Modules) {
				DirectoryInfo dir1 = new DirectoryInfo($"{baseDirectory.FullName}{item[1]}{Path.DirectorySeparatorChar}");
				if (!dir1.Exists) {
					dir1.Create();
				}
				dir1 = new DirectoryInfo($"{baseDirectory.FullName}{item[1]}Work{Path.DirectorySeparatorChar}");
				if (!dir1.Exists) {
					dir1.Create();
				}
			}
			PluginLoader.baseDirectory = baseDirectory;
			ControllerTreeElement.baseDirectory = baseDirectory;

			baseDirectoryWatcher = new FileSystemWatcher(baseDirectory.FullName);
			baseDirectoryWatcher.IncludeSubdirectories = true;
			baseDirectoryWatcher.Created += Watcher_Event;
			baseDirectoryWatcher.Deleted += Watcher_Deleted;
			baseDirectoryWatcher.Changed += Watcher_Changed;
			baseDirectoryWatcher.Renamed += Watcher_Renamed;
			baseDirectoryWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size;
			
			layoutsPlugins = LoadPlugins(ref Modules[1]);
			staticPlugins = LoadPlugins(ref Modules[2]);

			controllerTree = new ControllerTreeElement();

			baseDirectoryWatcher.EnableRaisingEvents = true;
		}

		private void Watcher_Event(object sender, FileSystemEventArgs e) {
			Watcher_Work(e.FullPath, e.ChangeType);
		}
		private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
			Watcher_Work(e.FullPath, e.ChangeType);
		}
		private void Watcher_Changed(object sender, FileSystemEventArgs e) {
			Watcher_Work(e.FullPath, e.ChangeType);
		}
		private void Watcher_Renamed(object sender, RenamedEventArgs e) {
			Watcher_Work(e.FullPath, e.ChangeType, e.OldFullPath);
		}

		private void Watcher_Work(string path, WatcherChangeTypes changeType, string nameOld = null) {
			string name;
			string module;
			bool fileAct;
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
			int moduleNum = -1;
			if (module == "controllers") { moduleNum = 0; }
			if (module == "layouts") { moduleNum = 1; }
			if (module == "static") { moduleNum = 2; }

			path = path.Replace('\\', '/');
			if (moduleNum == 0) {
				Queue<string> pathSplit = new Queue<string>((path + name).Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
				//Console.WriteLine("TESTOUT " + fullPath + " " + changeType + " " + path);
				//ControllerTreeElement tree = SearchController(ref controllerTree, ref pathSplit, true);
				ControllerTreeElement tree = controllerTree.SearchRoot(ref pathSplit);
				if (fileAct) {
					tree = (ControllerTreeElement)tree.elements[name];
					tree.Load();
				}
				else {
					switch (changeType) {
						case WatcherChangeTypes.Deleted: {
								tree.RemoveTree(name);
								break;
							}
						case WatcherChangeTypes.Created: {
								tree.AddTree(/*path, */name);
								break;
							}
						case WatcherChangeTypes.Renamed: {
								tree.RemoveTree(nameOld);
								tree.AddTree(/*$"{path}{Path.DirectorySeparatorChar}{name}", */name);
								break;
							}
					}
				}
			}
			else if (moduleNum == 1 || moduleNum == 2) {
				if (fileAct) {
					((moduleNum == 2 ? staticPlugins : layoutsPlugins)[name] as PluginLoader).Load();
				}
				else {
					PluginLoader loaderTmp = null;
					switch (changeType) {
						case WatcherChangeTypes.Deleted: {
								loaderTmp = (PluginLoader)(moduleNum == 2 ? staticPlugins : layoutsPlugins)[name];
								loaderTmp.RemoveWorkDir();
								(moduleNum == 2 ? staticPlugins : layoutsPlugins).Remove(name);
								break;
							}
						case WatcherChangeTypes.Created: {
								loaderTmp = new PluginLoader(ref Modules[moduleNum], path, name);
								loaderTmp.Load();
								(moduleNum == 2 ? staticPlugins : layoutsPlugins).Add(name, loaderTmp);
								break;
							}
						case WatcherChangeTypes.Renamed: {
								loaderTmp = (PluginLoader)(moduleNum == 2 ? staticPlugins : layoutsPlugins)[nameOld];
								loaderTmp.RemoveWorkDir();
								(moduleNum == 2 ? staticPlugins : layoutsPlugins).Remove(nameOld);
								loaderTmp = new PluginLoader(ref Modules[moduleNum], path, name);
								loaderTmp.Load();
								(moduleNum == 2 ? staticPlugins : layoutsPlugins).Add(name, loaderTmp);
								break;
							}
					}
				}
			}
			//}
			//catch { }
		}

		private Hashtable LoadPlugins(ref string[] module) {
			Hashtable hashtable = new Hashtable();

			DirectoryInfo baseLD = new DirectoryInfo($"{baseDirectory.FullName}{module[1]}{Path.DirectorySeparatorChar}");
			foreach (var item in baseLD.GetDirectories()) {
				PluginLoader loader = new PluginLoader(ref module, $"{Path.DirectorySeparatorChar}{item.Name}", item.Name);
				loader.Load();
				hashtable.Add(loader.Name, loader);
			}
			return hashtable;
		}

		public void Work(ref HelperClass helper) {
			string path = $"{helper.Context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)}";
			Queue<string> pathSplit = new Queue<string>(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
			if (pathSplit.Count == 0) {
				pathSplit.Enqueue("index");
			}
			ControllerTreeElement tree = controllerTree.Search(ref pathSplit, out string action);
			if (tree != null) {
				HttpListenerContext context = helper.Context;

				ControllerWorker controller = (ControllerWorker)tree.plugin.GetPluginRefObject();
				try {
					string[] staticInclude = controller._GetStaticInclude();
					if (staticInclude != null) {
						helper.staticPlugins = new Hashtable();
						for (int i = 0; i < staticInclude.Length; i++) {
							helper.staticPlugins.Add(staticInclude[i], ((PluginLoader)staticPlugins[staticInclude[i]]).plugin.GetPluginRefObject());
						}
					}

					controller._SetHelper(ref helper);
					controller._Work(action);
				}
				catch { throw; }

				helper.GetData(controller._GetHelper());

				context.Response.Headers.Add(helper.Responce.Headers);

				if (helper.returnType == ReturnType.Content) {
					context.Response.ContentType = "text/html; charset=UTF-8";
					context.Response.StatusDescription = "OK";

					//context.Response.

					string bufS = "";
					if (helper.Render.isEnabled) {
						ResentLayout(ref helper, ref controller, ref bufS, helper.Render.layout, true);
					}
					else {
						ResentContent(ref helper, ref controller, ref bufS);
					}
					//byte[] buf;
					//while (bufS.Length > 0) {
					//	buf = Encoding.UTF8.GetBytes(bufS.Substring(0, bufS.Length > 16000 ? 16000 : bufS.Length));
					//	bufS = bufS.Remove(0, bufS.Length > 16000 ? 16000 : bufS.Length);
					//	context.Response.OutputStream.Write(buf, 0, buf.Length);
					//}
					byte[] buf = Encoding.UTF8.GetBytes(bufS);
					context.Response.ContentLength64 = buf.Length;
					context.Response.OutputStream.Write(buf, 0, buf.Length);
					buf = null;
					bufS = null;
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

		public void WorkWS(ref HelperClass helper) {
			
			helper.ContextWs.WebSocket.Accept();
			helper.ContextWs.WebSocket.OnMessage += (sender, args) => {
				Console.WriteLine(args.Data);
				//helper.ContextWs.WebSocket.Send($"test WS message {helper.isSecureConnection}");
			};

			//for (int i = 0; i < 100; i++) {
			helper.ContextWs.WebSocket.Send($"test WS message {helper.isSecureConnection}");
			//}
		}

		void ResentLayout(ref HelperClass helper, ref ControllerWorker controller, ref string bufS, string layoutName, bool content) {
			LayoutWorker layout = (LayoutWorker)((PluginLoader)layoutsPlugins[layoutName]).plugin.GetPluginRefObject();
			layout._SetHelper(ref helper);
			layout._Work();
			helper.GetData(layout._GetHelper());
			//byte[] buf;
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
				//while (bufS.Length > 16000) {
				//	buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
				//	bufS = bufS.Remove(0, 16000);
				//	helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				//}
				ec = layout._GetNextContent();
			}
		}

		void ResentContent(ref HelperClass helper, ref ControllerWorker controller, ref string bufS) {
			//byte[] buf;
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
				//while (bufS.Length > 16000) {
				//	buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
				//	bufS = bufS.Remove(0, 16000);
				//	helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				//}
				ec = controller._GetNextContent();
			}
		}
	}
}