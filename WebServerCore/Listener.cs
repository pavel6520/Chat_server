using ConnectionWorker.Helpers;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebServerCore.Plugins;
using WebSocketSharp.Net;
using WebSocketSharp.Net.WebSockets;

namespace WebServerCore {
    sealed class Listener {
        HttpListener listener;
        bool enabled;
        private ILog Log;
        private PluginManagerClass packageManager;

        public HttpListenerPrefixCollection Prefixes { get { return listener.Prefixes; } }

        public Listener(ref ILog log, ref PluginManagerClass packageManager) {
            Log = log;
            this.packageManager = packageManager;
            listener = new HttpListener();
        }

        public int Start() {
            try {
				if (Config.SSLEnable) {
					listener.SslConfiguration.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(Config.SSLFileName, Config.SSLPass);
				}
                listener.Start();
                Thread thread = new Thread(new ThreadStart(Listen));
                thread.Start();
                enabled = true;
                return 0;
            }
            catch (Exception e) {
                Log.Error("Ошибка запуска прослушивателя", e);
            }
            return 1;
        }

        public void Stop() {
			lock (listener) {
				if (enabled) {
					enabled = false;
					listener.Abort();
				}
			}
        }

        private void Listen() {
			try {
				while (enabled) {
					var context = listener.GetContext();
					Task.Factory.StartNew(() => {
						try {
							Log.Info($"Подключение {context.Request.RemoteEndPoint.ToString()} запрос {context.Request.Url.PathAndQuery}");
							try {
								context.Response.Headers.Add(HttpResponseHeader.Server, "pavel6520/WebServerCore");

								if (context.Request.IsWebSocketRequest) {
									packageManager.WorkWS(
										new HelperClass(
											ref context, 
											Config.DBConnectionString, 
											Config.Domain,
											Config.Debug,
											context.AcceptWebSocket("13")));
								}
								else {
									packageManager.Work(
										new HelperClass(
											ref context, 
											Config.DBConnectionString,
											Config.Domain,
											Config.Debug));
								}
							}
							catch (PathNotFoundException e) {
								Log.Debug($"Не найден путь {e.Message}");
							}
							finally {
								if (!context.Request.IsWebSocketRequest) {
									context.Response.Close();
								}
							}
						}
						catch (Exception e) {
							Log.Error("Ошибка обработки подключения", e);
						}
					}, TaskCreationOptions.LongRunning);
				}
			}
			catch (SocketException e) {
				Log.Info($"Прослушиватель остановлен: {e.Message}");
			}
			catch (HttpListenerException e) {
				if (!enabled) {
					Log.Info("Прослушиватель остановлен");
				}
				else {
					Log.Error("Ошибка обработки подключения", e);
				}
			}
			catch (Exception e) {
				Log.Error("Ошибка обработки подключения", e);
			}
        }
    }
}