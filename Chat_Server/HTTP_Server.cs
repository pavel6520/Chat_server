using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat_server
{
    class HTTP_Server
    {
        private HttpListener listener;
        byte[] indexPage;
        byte[] JSfuncFile;
        byte[] indexPageTest;
        byte[] JSfuncFileTest;
        byte[] favicon;
        //byte[] jQuery;
        int port;

        public HTTP_Server(int port)
        {
            this.port = port;
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + port + "/");

            LoadFile();
        }

        public bool Start()
        {
            try {
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(DateTime.Now + " [ERROR][HTTP] Не удалось запустить сервер. " + ex.Message);
                return false;
            }

            Task.Factory.StartNew(() =>
            {
                try
                {
                    while (true)
                    {
                        HttpListenerContext context = listener.GetContext();

                        Task.Factory.StartNew(() =>
                        {
                            FastResponse(context);
                        });
                    }
                }
                catch (HttpListenerException) { }
            }, TaskCreationOptions.LongRunning);

            Console.WriteLine(DateTime.Now + " [INFO][HTTP] Сервер запущен, порт " + port);
            return true;
            }

        public void Stop()
        {
            listener.Stop();
            Console.WriteLine(DateTime.Now + " [INFO][HTTP] Сервер остановлен");
        }

        public void LoadFile()
        {
            indexPage = Encoding.Unicode.GetBytes(File.ReadAllText("index.html"));
            JSfuncFile = Encoding.Unicode.GetBytes(File.ReadAllText("func.js"));
            indexPageTest = Encoding.Unicode.GetBytes(File.ReadAllText("index1.html"));
            JSfuncFileTest = Encoding.Unicode.GetBytes(File.ReadAllText("func1.js"));
            favicon = File.ReadAllBytes("favicon.ico");
            //jQuery = File.ReadAllBytes("jquery-3.3.1.js");
        }

        void FastResponse(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            string RequestPath = context.Request.Url.LocalPath;

            if (Program.DEBUG)
            {
                Console.WriteLine(DateTime.Now + " [DEBUG][HTTP] Запрос: " + context.Request.RemoteEndPoint + " " + context.Request.Url.AbsoluteUri);
                //DEBUG
                /*for (int i = 0; i < context.Request.Headers.Count; i++)
                {
                    Console.WriteLine(context.Request.Headers.Keys[i] + ": " + context.Request.Headers[i]);
                }
                Console.WriteLine();*/
            }

            if (RequestPath == "/" || RequestPath == "/index.html")
            {
                response.ContentType = "text/html; charset=Unicode";
                //response.ContentLength64 = indexPage.Length;
                response.OutputStream.Write(indexPage, 0, indexPage.Length);
            }
            else if (RequestPath == "/func.js")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(JSfuncFile, 0, JSfuncFile.Length);
            }
            else if (RequestPath.Length > 6 && RequestPath.Substring(1, 5) == "image")
            {
                byte[] img = File.ReadAllBytes("image/" + RequestPath.Substring(7, RequestPath.Length - 7) + ".png");
                response.OutputStream.Write(img, 0, img.Length);
            }
            else if (RequestPath == "/test")
            {
                response.ContentType = "text/html; charset=Unicode";
                response.OutputStream.Write(indexPageTest, 0, indexPageTest.Length);
            }
            else if (RequestPath == "/func1.js")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(JSfuncFileTest, 0, JSfuncFileTest.Length);
            }
            else if (RequestPath == "/favicon.ico")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(favicon, 0, favicon.Length);
            }
            /*else if (context.Request.Url.LocalPath == "/jquery-3.3.1.js")
            {
                //response.ContentType = "text/javascript; charset=Unicode";
                response.ContentLength64 = jQuery.Length;
                response.OutputStream.Write(jQuery, 0, jQuery.Length);
            }*/

            response.Close();
        }
    }
}
