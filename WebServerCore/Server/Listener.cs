using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Connection;
using WebServerCore.Connection.Http;

namespace WebServerCore.Server {
    sealed class Listener {
        List<Socket> listenerList;
        Socket socketListener;
        Socket socketListenerSSL;

        public Listener() {
            listenerList = new List<Socket>(1);
        }

        public int StartListener(IPAddress address, int port, int maxQueue = 1000, X509Certificate certificate = null, int timeOutHTTP = 30000, int timeOutWS = 1800000) {
            try {
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(new IPEndPoint(address, port));
                listener.Listen(maxQueue);
                listenerList.Add(listener);
                Task.Factory.StartNew(() => {
                    try {
                        while (true) {
                            Socket client = socketListenerSSL.Accept();
                            client.ReceiveTimeout = timeOutHTTP;
                            Task.Factory.StartNew(() => {
                                try {
                                    ConnectionManager.ClientStart(client, certificate, timeOutWS);
                                }
                                catch (Exception e) {
                                    Log.Write(LogType.ERROR, "Listener", $"Ошибка обработки подключения: {e.Message}", e.StackTrace);
                                }
                            }, TaskCreationOptions.LongRunning);
                        }
                    }
                    catch (SocketException e) {
                        Log.Write(LogType.INFO, "Listener", $"Прослушиватель порта {port} остановлен: {e.Message}");
                    }
                    catch (Exception e) {
                        Log.Write(LogType.ERROR, "Listener", $"Ошибка приемника подключений: {e.Message}", e.StackTrace);
                    }
                }, TaskCreationOptions.LongRunning);
            }
            catch (Exception e) {
                Log.Write(LogType.ERROR, "Listener", $"Ошибка запуска прослушивателя: {e.Message}", e.StackTrace);
                return 1;
            }
            return 0;
        }

        public int Start() {
            socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListener.Bind(new IPEndPoint(IPAddress.Any, 80));
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
                    }
                }
            }, TaskCreationOptions.LongRunning);
            socketListenerSSL = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socketListenerSSL.Bind(new IPEndPoint(IPAddress.Any, 443));
            socketListenerSSL.Listen(1000);
            Task.Factory.StartNew(() => {
                while (true) {
                    Socket client;
                    client = socketListenerSSL.Accept();
                    Task.Factory.StartNew(() => {
                        try {
                            ClientStart(client, true);
                        }
                        catch (Exception e) {
                            Log.Write(LogType.ERROR, "Listener", $"Ошибка обработки подключения: {e.Message}", e.StackTrace);
                        }//TODO
                    }, TaskCreationOptions.LongRunning);
                }
            }, TaskCreationOptions.LongRunning);
            return 0;
        }
        static int count = 0;
        private void ClientStart(Socket client, bool crypt = false) {
            int num = count++;
            Console.WriteLine($"{num}===========================HTTP START");
            Connection.Connection cc;
            if (crypt) {
                cc = new Connection.Connection(client, new X509Certificate2("cert.pfx", "6520"));
            }
            else {
                cc = new Connection.Connection(client);
            }
            bool end = true;
            HttpContext context = null;
            try {
                while (end) {
                    context = new HttpContext(cc);
                    HttpRequest request = context.Request;
                    HttpResponse response = context.Response;

                    Console.WriteLine($"{num}==={request.Method} {request.Path} {request.Protocol}/{request.ProtocolVersion} - {request.UserAddress}");
                    //for (int i = 0; i < request.Headers.Count; i++)
                    //    Console.WriteLine($"{request.Headers.GetKey(i)} : {request.Headers.GetValues(i)[0]}");
                    //Console.WriteLine();

                    if (request.IsWebSocket) {
                        Console.WriteLine($"{num}===========================WEBSOCKET");
                        //Console.WriteLine($"{request.Method} {request.Path} {request.Protocol}/{request.ProtocolVersion} - {request.UserAddress}");
                        //for (int i = 0; i < request.Headers.Count; i++)
                        //    Console.WriteLine($"{request.Headers.GetKey(i)} : {request.Headers.GetValues(i)[0]}");
                        //Console.WriteLine();

                        WebSocket webSocket = new WebSocket(context);
                        webSocket.WriteText(Encoding.UTF8.GetBytes(DateTime.Now.ToString("r")));
                        webSocket.WriteText(Encoding.UTF8.GetBytes(DateTime.Now.ToString("r")));
                        webSocket.WriteText(Encoding.UTF8.GetBytes(DateTime.Now.ToString("r")));
                        byte[] b = null;
                        int res = webSocket.ReadText(ref b);
                        if (res == 1) {
                            Console.WriteLine($"{num}==================WEBSOCKET CLOSE");
                            return;
                        }
                        Console.WriteLine($"{num}=========WEBSOCKET{Encoding.UTF8.GetString(b)}");
                        b = new byte[0];
                        res = webSocket.ReadText(ref b);
                        if (res == 1) {
                            Console.WriteLine($"{num}==================WEBSOCKET CLOSE");
                            return;
                        }
                        Console.WriteLine($"{num}=========WEBSOCKET{Encoding.UTF8.GetString(b)}");
                        return;
                    }
                    else {
                        //response.StatusCode = 301;
                        //response.StatusDescription = "Moved";
                        //response.ContentLength64 = 0;
                        //response.RedirectLocation = "https://127.0.0.1/123";
                        //response.Close();
                        //response.Cookies.Add(new Cookie("test127001", "test1"));
                        response.StatusDescription = "OK";
                        byte[] buf = Encoding.UTF8.GetBytes("<html><body>" +
                            "<script>" +
                            "var count = 0;\r\n" +
                            "ws=new WebSocket('wss://192.168.0.106/websocket');\r\n" +
                            "ws.onopen = function() {\r\n" +
                            "console.log('connected');\r\n" +
                            "};\r\n" +
                            "ws.onmessage = function(evt) {\r\n" +
                            "console.log(evt.data);\r\n" +
                            "count++;\r\n" +
                            "if (count == 3){\r\n" +
                            "ws.send('testmessage 123');\r\n" +
                            "}\r\n" +
                            "};\r\n" +
                            "</script>" +
                            //"<link rel=\"stylesheet\" href=\"test1.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test2.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test3.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test4.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test5.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test6.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test7.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test8.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test9.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test10.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test11.css\">" +
                            //"<link rel=\"stylesheet\" href=\"test12.css\">" +
                            "<h1>TEST PAGE</span></body></html>");
                        response.ContentLength64 = buf.Length;
                        response.ContentType = "text/html; charset=UTF8";
                        Connection.ConnectionWrite write = response.GetWriteStream();
                        write.Write(buf);
                    }
                }
            }
            catch (ConnectionCloseException) {
                Log.Write(LogType.DEBUG, "Listener", $"Клиент {client.RemoteEndPoint} разорвал соединение до отправки данных");
            }
            Console.WriteLine($"{num}===========================HTTP END");
        }
    }
}