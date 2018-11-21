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
        byte[] HTMLindex;
        byte[] JSfunc;
        byte[] JSstruct;
        byte[] HTMLindexTest;
        byte[] JSfuncTest;
        byte[] CSSstyle;
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
            HTMLindex = Encoding.Unicode.GetBytes(File.ReadAllText("index.html")/*.Replace("\r\n", "").Replace("\t", "")*/);
            JSfunc = Encoding.Unicode.GetBytes(File.ReadAllText("js/func.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            JSstruct = Encoding.Unicode.GetBytes(File.ReadAllText("js/struct.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            HTMLindexTest = Encoding.Unicode.GetBytes(File.ReadAllText("index1.html")/*.Replace("\r\n", "").Replace("\t", "")*/);
            JSfuncTest = Encoding.Unicode.GetBytes(File.ReadAllText("js/func1.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            CSSstyle = Encoding.Unicode.GetBytes(File.ReadAllText("css/style.css")/*.Replace("\r\n", "").Replace("\t", "")*/);
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
                response.OutputStream.Write(HTMLindex, 0, HTMLindex.Length);
            }
            else if (RequestPath == "/js/func.js")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(JSfunc, 0, JSfunc.Length);
            }
            else if (RequestPath == "/js/struct.js")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(JSstruct, 0, JSstruct.Length);
            }
            else if (RequestPath == "/test")
            {
                response.ContentType = "text/html; charset=Unicode";
                response.OutputStream.Write(HTMLindexTest, 0, HTMLindexTest.Length);
            }
            else if (RequestPath == "/js/func1.js")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(JSfuncTest, 0, JSfuncTest.Length);
            }
            else if (RequestPath == "/css/style.css")
            {
                response.ContentType = "text/css; charset=Unicode";
                response.OutputStream.Write(CSSstyle, 0, CSSstyle.Length);
            }
            else if (RequestPath == "/favicon.ico")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(favicon, 0, favicon.Length);
            }
            else if (RequestPath.Length > 6 && RequestPath.Substring(1, 5) == "image")
            {
                byte[] img = File.ReadAllBytes("image/" + RequestPath.Substring(7, RequestPath.Length - 7) + ".png");
                response.OutputStream.Write(img, 0, img.Length);
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
