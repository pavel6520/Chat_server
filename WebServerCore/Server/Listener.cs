using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Client.Http;

namespace WebServerCore.Server {
    sealed class Listener {
        //TcpListener _listener;
        Socket _listener;
        Socket _listenerSSL;
        X509Certificate certificate;

        public Listener() {
        }

        public int Start() {
            certificate = new X509Certificate("1.pfx", "6520");
            Task.Factory.StartNew(() => {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listener.Bind(new IPEndPoint(IPAddress.Any, Config.http.port));
                _listener.Listen(1000);
                while (true) {
                    Socket client;
                    try {
                        client = _listener.Accept();
                        Task.Factory.StartNew(() => ClientStart(client), TaskCreationOptions.LongRunning);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(() => {
                _listenerSSL = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _listenerSSL.Bind(new IPEndPoint(IPAddress.Any, Config.https.port));
                _listenerSSL.Listen(1000);

                while (true) {
                    Socket client;
                    try {
                        client = _listenerSSL.Accept();
                        Task.Factory.StartNew(() => ClientStartSSL(client), TaskCreationOptions.LongRunning);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }
                }
            }, TaskCreationOptions.LongRunning);
            return 0;
        }

        private void ClientStart(Socket client) {
            Client.Connection cc = new Client.Connection(client);
            HttpContext context = null;
            try {
                context = new HttpContext(cc);
            }
            catch (FormatException) {
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

            //string strRequest;
            //{
            //    byte[] buffer = new byte[1000];
            //    int receivedBCount = client.Receive(buffer);
            //    strRequest = Encoding.UTF8.GetString(buffer, 0, receivedBCount);

            //    buffer = null;
            //}
            //// Парсим запрос
            //string httpMethod = strRequest.Substring(0, strRequest.IndexOf(" "));

            //int start = strRequest.IndexOf(httpMethod) + httpMethod.Length + 1;
            //int length = strRequest.LastIndexOf("HTTP") - start - 1;
            //string requestedUrl = strRequest.Substring(start, length);

            //byte[] read;

            //byte[] buf = Encoding.UTF8.GetBytes("HTTP/1.1 302 Found\r\n" +
            //            "Location: https://127.0.0.1/\r\n" +
            //            "\r\n");
            //client.Send(buf); // stream.Write(buf, 0, buf.Length);
            //client.Close();
        }

        private void ClientStartSSL(Socket client) {
            Client.Connection cc = new Client.Connection(client, certificate);
            bool end = true;
            HttpContext context = null;
            while (end) {
                try {
                    context = new HttpContext(cc);
                }
                catch (FormatException) {
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
                response.ContentLength64 = 5;
                Client.ConnectionWrite write = response.GetWriteStream();
                write.Write(buf);
            }

            //SslStream sslStream = new SslStream(new NetworkStream(client));
            //sslStream.AuthenticateAsServer(certificate, false, System.Security.Authentication.SslProtocols.Tls12, false);
            //byte[] read = new byte[1000];
            //int readed = sslStream.Read(read, 0, 1000);
            //Console.WriteLine(Encoding.ASCII.GetString(read, 0, readed));
            //byte[] buf = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n" +
            //    "Content-Length: 9\r\n" +
            //    "\r\n" +
            //    "12345\r\n\r\n");
            //sslStream.Write(buf, 0, buf.Length);
            //sslStream.Close();
        }
    }
}