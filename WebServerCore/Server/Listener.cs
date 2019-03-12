using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Connection.Http;

namespace WebServerCore.Server {
    sealed class Listener {
        //TcpListener _listener;
        Socket socketListener;
        Socket socketListenerSSL;
        X509Certificate certificate;

        public Listener() { }

        public int Start() {
            certificate = new X509Certificate2("2.pfx", "6520");
            socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListener.Bind(new IPEndPoint(IPAddress.Any, Config.http.port));
            socketListener.Listen(1000);
            Task.Factory.StartNew(() => {
                while (true) {
                    Socket client;
                    try {
                        client = socketListener.Accept();
                        Task.Factory.StartNew(() => ClientStart(client), TaskCreationOptions.LongRunning);
                    }
                    catch (Exception e) {
                        Log.Write(LogType.ERROR, "Listener", $"Ошибка обработки подключения: {e.Message}", e.StackTrace);
                    }//TODO
                }
            }, TaskCreationOptions.LongRunning);
            socketListenerSSL = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListenerSSL.Bind(new IPEndPoint(IPAddress.Any, Config.https.port));
            socketListenerSSL.Listen(1000);
            Task.Factory.StartNew(() => {
                while (true) {
                    Socket client;
                    try {
                        client = socketListenerSSL.Accept();
                        Task.Factory.StartNew(() => ClientStartCrypt(client), TaskCreationOptions.LongRunning);
                    }
                    catch (Exception e) {
                        Log.Write(LogType.ERROR, "Listener", $"Ошибка обработки подключения: {e.Message}", e.StackTrace);
                    }//TODO
                }
            }, TaskCreationOptions.LongRunning);
            return 0;
        }

        private void ClientStart(Socket client) {
            Connection.Connection cc = new Connection.Connection(client);
            HttpContext context = null;
            try {
                context = new HttpContext(cc);
            }
            catch (FormatException) {
                Log.Write(LogType.DEBUG, "Listener", $"Клиент {client.RemoteEndPoint} разорвал соединение до отправки данных");
                return;
            }
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            Console.WriteLine($"{request.Method} {request.Path} {request.Protocol}/{request.ProtocolVersion} - {request.UserAddress}");
            for (int i = 0; i < request.Headers.Count; i++)
                Console.WriteLine($"{request.Headers.GetKey(i)} : {request.Headers.GetValues(i)[0]}");
            Console.WriteLine();

            response.StatusCode = 301;
            response.StatusDescription = "Moved";
            response.ContentLength64 = 0;
            response.RedirectLocation = "https://127.0.0.1/123";
            response.Close();
        }

        private void ClientStartCrypt(Socket client) {
            Connection.Connection cc = null;
            try {
                cc = new Connection.Connection(client, certificate);
            }
            catch (IOException) {
                Log.Write(LogType.DEBUG, "Listener", $"Клиент {client.RemoteEndPoint} разорвал соединение до завершения рукопожатия SSL");
                return;
            }
            bool end = true;
            HttpContext context = null;
            while (end) {
                try {
                    context = new HttpContext(cc);
                }
                catch (FormatException) {
                    Log.Write(LogType.DEBUG, "Listener", $"Клиент {client.RemoteEndPoint} разорвал соединение до отправки данных");
                    return;
                }
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;

                Console.WriteLine($"{request.Method} {request.Path} {request.Protocol}/{request.ProtocolVersion} - {request.UserAddress}");
                for (int i = 0; i < request.Headers.Count; i++)
                    Console.WriteLine($"{request.Headers.GetKey(i)} : {request.Headers.GetValues(i)[0]}");
                Console.WriteLine();

                response.StatusDescription = "OK";
                byte[] buf = Encoding.UTF8.GetBytes("12345");
                response.ContentLength64 = buf.Length;
                response.ContentType = "text/plain; charset=UTF8";
                Connection.ConnectionWrite write = response.GetWriteStream();
                write.Write(buf);
            }
        }
    }
}