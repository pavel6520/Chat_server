using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Connection.Http;

namespace WebServerCore.Connection {
    static class ConnectionManager {
        internal static void ClientStart(Socket client, X509Certificate certificate, int timeoutWS) {
            Connection cc = new Connection(client, certificate);
            bool end = true;
            HttpContext context = null;
            try {
                while (end) {
                    context = new HttpContext(cc);
#if DEBUG
                    Console.WriteLine($"{context.Request.Method} {context.Request.Path} {context.Request.Protocol}/{context.Request.ProtocolVersion} - {context.Request.UserAddress}");
#endif
                    if (context.Request.IsWebSocket) {
                        cc.ReceiveTimeout = timeoutWS;
                        WebSocket webSocket = new WebSocket(context);

                        break;
                    }
                    else {

                        break;
                    }
                }
            }
            catch (ConnectionCloseException) {
                Log.Write(LogType.DEBUG, "Listener", $"Клиент {client.RemoteEndPoint} разорвал соединение до отправки данных");
            }
            Console.WriteLine("===========================HTTP END");
        }
    }
}
