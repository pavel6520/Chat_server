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
				listener.SslConfiguration.ServerCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2("cert.pfx", "6520");
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
						//var SW = new System.Diagnostics.Stopwatch();
						//SW.Start();
						try {
							Log.Info($"Подключение {context.Request.RemoteEndPoint.ToString()} запрос {context.Request.Url.PathAndQuery}");
#if DEBUG
							string ip = context.Request.RemoteEndPoint.ToString();
							//Console.WriteLine($"{context.Request.HttpMethod} {context.Request.Url.OriginalString} - {ip}");
#endif
							try {
								context.Response.Headers.Add(HttpResponseHeader.Server, "pavel6520/WebServerCore");
#if DEBUG
								string domain = "127.0.0.1";
#else
								string domain = "pavel6520.hopto.org";
								//string domain = "127.0.0.1";
#endif
								if (context.Request.IsWebSocketRequest) {
									packageManager.WorkWS(
										new HelperClass(
											ref context, 
											"server=127.0.0.1;port=3306;user=root;password=6520;database=chat;", 
											domain, 
											context.AcceptWebSocket("13")));
								}
								else {
									packageManager.Work(
										new HelperClass(
											ref context, 
											"server=127.0.0.1;port=3306;user=root;password=6520;database=chat;", 
											domain));
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
							//Console.WriteLine($"===========================HTTP END - {ip}");
#endif
						}
						catch (Exception e) {
							Log.Error("Ошибка обработки подключения", e);
						}
						//SW.Stop();
						//Console.WriteLine($"TIME {context.Request.Url.OriginalString} = {Convert.ToString(SW.ElapsedMilliseconds)}");
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