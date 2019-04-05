using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebServerCore.Plugins;

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
            enabled = false;
            listener.Abort();
        }

        private void Listen() {
			try {
				while (enabled) {
					var context = listener.GetContext();
					Task.Factory.StartNew(() => {
						var SW = new System.Diagnostics.Stopwatch();
						SW.Start();
						try {
							Log.Info($"Подключение {context.Request.RemoteEndPoint.ToString()} запрос {context.Request.Url.PathAndQuery}");
#if DEBUG
							string ip = context.Request.RemoteEndPoint.ToString();
							Console.WriteLine($"{context.Request.HttpMethod} {context.Request.Url.OriginalString} - {ip}");
#endif
							try {
								if (context.Request.IsWebSocketRequest) {
									//WebSocket webSocket = new WebSocket(context);
									Task<HttpListenerWebSocketContext> task = context.AcceptWebSocketAsync(null, new TimeSpan(1000 * 60 * 30));
									task.Wait();
									HttpListenerWebSocketContext webSocketContext = task.Result;
								}
								else {
									context.Response.Headers.Add(HttpResponseHeader.Server, "pavel6520/WebServerCore");
									packageManager.HttpContextWork(ref context);
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
#if DEBUG
							Console.WriteLine($"===========================HTTP END - {ip}");
#endif
						}
						catch (Exception e) {
							Log.Error("Ошибка обработки подключения", e);
						}
						SW.Stop();
						Console.WriteLine("TIME = " + Convert.ToString(SW.ElapsedMilliseconds));
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