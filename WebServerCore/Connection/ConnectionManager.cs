using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Connection.Http;
using PackageManager;

namespace WebServerCore.Connection {
    internal static class ConnectionManager {
        private static PackageManagerClass manager;

        internal static void LoadPackage() {
            manager = new PackageManagerClass($"{System.IO.Path.DirectorySeparatorChar}application{System.IO.Path.DirectorySeparatorChar}");
        }

        internal static void ClientStart(Socket client, X509Certificate certificate, int timeoutWS, ref string Domain) {
            ConnectionClass cc = new ConnectionClass(client, certificate);
            bool end = true;
            HttpContext context = null;
            try {
                while (end) {
                    context = new HttpContext(cc, ref Domain);
#if DEBUG
                    Console.WriteLine($"{context.Request.Method} {context.Request.Uri} {context.Request.Protocol}/{context.Request.ProtocolVersion} - {context.Request.UserAddress}");
#endif
                    if (context.Request.IsWebSocket) {
                        cc.ReceiveTimeout = timeoutWS;
                        WebSocket webSocket = new WebSocket(context);

                        break;
                    }
                    else {
                        manager.WorkHttp(ref context);
                    }
                }
            }
            catch (ConnectionCloseException) {
                Core.Log.Debug($"Клиент {client.RemoteEndPoint} разорвал соединение до отправки данных");
            }
            finally {
                cc.Close();
            }
#if DEBUG
            Console.WriteLine("===========================HTTP END");
#endif
        }
    }
}
