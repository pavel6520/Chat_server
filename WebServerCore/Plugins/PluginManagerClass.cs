using ConnectionWorker;
using ConnectionWorker.Helpers;
using log4net;
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
		public static ILog Log { get; private set; }

		private const string HTTPinitName = "httpinit";
		private const string WebSocketinitName = "websocketinit";

		private DirectoryInfo baseDirectory;
		private FileSystemWatcher baseDirectoryWatcher;
		private ControllerTreeElement controllerTree;
		private Hashtable layoutsPlugins;
		private Hashtable staticPlugins;
		private bool HTTPinit;
		private bool WebSocketInit;

		private Hashtable WSclients = new Hashtable();

		public PluginManagerClass(ref ILog log, string path) {
			Plugin.Log = Log = log;
			ControllerTreeElement.Module = Modules[0];
			baseDirectory = new DirectoryInfo(path);
			if (!baseDirectory.Exists) {
				baseDirectory.Create();
			}
			foreach (var item in Modules) {
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
			HTTPinit = staticPlugins.ContainsKey(HTTPinitName);
			WebSocketInit = staticPlugins.ContainsKey(WebSocketinitName);

			controllerTree = new ControllerTreeElement();

			baseDirectoryWatcher.EnableRaisingEvents = true;
		}

		private void Watcher_Event(object sender, FileSystemEventArgs e) {
			Watcher_Work(sender, e.FullPath, e.ChangeType);
		}
		private void Watcher_Deleted(object sender, FileSystemEventArgs e) {
			Watcher_Work(sender, e.FullPath, e.ChangeType);
		}
		private void Watcher_Changed(object sender, FileSystemEventArgs e) {
			Watcher_Work(sender, e.FullPath, e.ChangeType);
		}
		private void Watcher_Renamed(object sender, RenamedEventArgs e) {
			Watcher_Work(sender, e.FullPath, e.ChangeType, e.OldFullPath);
		}

		public void WatcherStop() {
			baseDirectoryWatcher.EnableRaisingEvents = false;
		}

		private void Watcher_Work(object sender, string path, WatcherChangeTypes changeType, string nameOld = null) {
			lock (sender) {
				try {
					string name;
					string module;
					bool fileAct;
					int intTmp = baseDirectory.FullName.Length;
					path = path.Remove(0, intTmp);
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
				}
				catch (Exception e) {
					Log.Error("Ошибка обновления пакетов", e);
				}
			}
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

		public void Work(HelperClass _helper) {
			string path = $"{_helper.Context.Request.Url.GetComponents(UriComponents.Path, UriFormat.SafeUnescaped)}";
			Queue<string> pathSplit = new Queue<string>(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
			if (pathSplit.Count == 0) {
				pathSplit.Enqueue("index");
			}
			ControllerTreeElement tree = controllerTree.Search(ref pathSplit, out string action);
			if (tree != null) {
				ControllerWorker controller = (ControllerWorker)tree.plugin.GetPluginRefObject();
				try {
					if (HTTPinit) {
						PluginWorker httpInitPlugin = GetPlugin(HTTPinitName);
						GetStaticPlugins(httpInitPlugin, ref _helper);
						httpInitPlugin._Work(_helper);
						_helper.GetData(httpInitPlugin._GetHelper());
					}
					GetStaticPlugins(controller, ref _helper);

					controller._Work(_helper, action);
					_helper.GetData(controller._GetHelper());

					_helper.Context.Response.Headers.Add(_helper.Responce.Headers);
					_helper.Context.Response.StatusCode = _helper.Responce.StatusCode;
					_helper.Context.Response.StatusDescription = _helper.Responce.StatusDescription;
				}
				catch (Exception e) {
					Log.Error("Внутренняя ошибка подключения", e);
					_helper.Answer500(e);
				}

				if (_helper.returnType == ReturnType.DefaultContent) {
					_helper.Context.Response.ContentType = _helper.Responce.ContentType;

					string bufS = "";
					if (_helper.Render.isEnabled) {
						ResentLayout(ref _helper, ref controller, ref bufS, _helper.Render.layout, true);
					}
					else {
						ResentContent(ref _helper, ref controller, ref bufS);
					}
					//byte[] buf; // не работает в данной реализации HttpListenerContext
					//while (bufS.Length > 0) {
					//	buf = Encoding.UTF8.GetBytes(bufS.Substring(0, bufS.Length > 16000 ? 16000 : bufS.Length));
					//	bufS = bufS.Remove(0, bufS.Length > 16000 ? 16000 : bufS.Length);
					//	helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
					//}
					byte[] buf = Encoding.UTF8.GetBytes(bufS);
					_helper.Context.Response.ContentLength64 = buf.Length;
					_helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				}
				else if (_helper.returnType == ReturnType.Info) {
					if (_helper.Responce.StatusCode / 100 == 3) {
						_helper.Context.Response.Redirect(_helper.Responce.RedirectLocation);
					}
				}
				if (_helper.WShelper.Acts.Count > 0) {
					if (_helper.WShelper.ActsForAll) {
						foreach (HelperClass WSclient in WSclients.Values) {
							foreach (WebSocketAct act in _helper.WShelper.Acts) {
								WSclient.ContextWs.WebSocket.Send(act.Body);
							}
						}
					}
					else {
						foreach (HelperClass WSclient in WSclients.Values) {
							foreach (WebSocketAct act in _helper.WShelper.Acts) {
								if (act.Recepients.Contains(WSclient.Auth.Login)) {
									WSclient.ContextWs.WebSocket.Send(act.Body);
								}
							}
						}
					}
				}
			}
			else {
				bool resFile = false;
				try {
					FileInfo fi = new FileInfo($"{baseDirectory.FullName}public{Path.DirectorySeparatorChar}{path.Replace('/', Path.DirectorySeparatorChar)}");
					if (fi.Exists) {
						using (FileStream reader = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read)) {
							resFile = true;
							int pos = fi.Name.LastIndexOf('.') + 1;
							string format = fi.Name.Substring(pos, fi.Name.Length - pos);
							if (format == "css") {
								_helper.Context.Response.ContentType = "text/css";
							}
							else if (format == "js") {
								_helper.Context.Response.ContentType = "text/javascript";
							}
							_helper.Context.Response.ContentLength64 = reader.Length;
							_helper.Context.Response.Headers.Add(HttpResponseHeader.CacheControl, "max-age=86400");
							reader.CopyTo(_helper.Context.Response.OutputStream, 16384);
						}
					}
					else {
						resFile = false;
					}
				}
				catch {
					resFile = false;
				}
				if (!resFile) {
					_helper.Context.Response.StatusCode = 404;
					_helper.Context.Response.StatusDescription = "Not found";
				}
			}
		}

		public void WorkWS(HelperClass _helper) {
			_helper.ContextWs.WebSocket.OnMessage += (sender, args) => {
				HelperClass helper = (HelperClass)WSclients[sender];
				var json = Newtonsoft.Json.Linq.JObject.Parse(args.Data);
				if (!json.ContainsKey("path") || !json.ContainsKey("type") || !json.ContainsKey("body")) {
					helper.ContextWs.WebSocket.Close(WebSocketSharp.CloseStatusCode.InvalidData, "Invalid message content");
					return;
				}
				string path = (string)json["path"];
				Queue<string> pathSplit = new Queue<string>(path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
				if (pathSplit.Count == 0) {
					return;
				}
				ControllerTreeElement tree = controllerTree.SearchWS(ref pathSplit, out string action);
				if (tree != null) {
					ControllerWorker controller = (ControllerWorker)tree.plugin.GetPluginRefObject();
					try {
						GetStaticPlugins(controller, ref helper);
						controller._WorkWS(helper, action, new object[] { (string)json["type"], (string)json["body"] });
						helper.GetData(controller._GetHelper());

						if (helper.returnType == ReturnType.DefaultContent) {
							string bufS = "";
							ResentContent(ref helper, ref controller, ref bufS);
							helper.ContextWs.WebSocket.Send(bufS);
							bufS = null;
						}
					}
					catch (Exception e) {
						if (helper.ContextWs.WebSocket.IsAlive) {
							Log.Error("Ошибка обработки сообщения WebSocket", e);
						}
					}
				}
			};
			_helper.ContextWs.WebSocket.OnClose += (sender, args) => {
				HelperClass helper = (HelperClass)WSclients[sender];
				WSclients.Remove(sender);
			};

			if (WebSocketInit) {
				PluginWorker httpInitPlugin = GetPlugin(WebSocketinitName);
				GetStaticPlugins(httpInitPlugin, ref _helper);
				httpInitPlugin._Work(_helper);
				_helper.GetData(httpInitPlugin._GetHelper());
				if (_helper.Auth.Status) {
					WSclients.Add(_helper.ContextWs.WebSocket, _helper);
					_helper.ContextWs.WebSocket.Accept();
					return;
				}
			}
			_helper.ContextWs.WebSocket.Close(WebSocketSharp.CloseStatusCode.Normal, "Unathorized");
		}

		public void GetStaticPlugins(PluginWorker controller, ref HelperClass helper) {
			string[] staticInclude = controller._GetStaticInclude();
			if (staticInclude != null) {
				if (helper.staticPlugins == null) {
					helper.staticPlugins = new Hashtable();
				}
				for (int i = 0; i < staticInclude.Length; i++) {
					if (!helper.staticPlugins.ContainsKey(staticInclude[i])) {
						helper.staticPlugins.Add(staticInclude[i], ((PluginLoader)staticPlugins[staticInclude[i]]).plugin.GetPluginRefObject());
					}
				}
			}
		}

		PluginWorker GetPlugin(string name) {
			return (PluginWorker)((PluginLoader)staticPlugins[name]).plugin.GetPluginRefObject();
		}

		LayoutWorker GetLayout(string name) {
			return (LayoutWorker)((PluginLoader)layoutsPlugins[name]).plugin.GetPluginRefObject();
		}

		void ResentLayout(ref HelperClass helper, ref ControllerWorker controller, ref string bufS, string layoutName, bool content) {
			LayoutWorker layout = GetLayout(layoutName);
			GetStaticPlugins(layout, ref helper);
			layout._Work(helper);
			helper.GetData(layout._GetHelper());
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
				//	byte[] buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
				//	bufS = bufS.Remove(0, 16000);
				//	helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				//}
				ec = layout._GetNextContent();
			}
		}

		void ResentContent(ref HelperClass helper, ref ControllerWorker controller, ref string bufS) {
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
				//	byte[] buf = Encoding.UTF8.GetBytes(bufS.Substring(0, 16000));
				//	bufS = bufS.Remove(0, 16000);
				//	helper.Context.Response.OutputStream.Write(buf, 0, buf.Length);
				//}
				ec = controller._GetNextContent();
			}
		}
	}
}