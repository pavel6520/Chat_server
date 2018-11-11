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
            //jQuery = File.ReadAllBytes("jquery-3.3.1.js");
        }

        void FastResponse(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            if (Program.DEBUG)
                Console.WriteLine(DateTime.Now + " [DEBUG][HTTP] Запрос: " + context.Request.RemoteEndPoint + " " + context.Request.Url.AbsoluteUri);

            if (context.Request.Url.LocalPath == "/")
            {
                response.ContentType = "text/html; charset=Unicode";
                //response.ContentLength64 = indexPage.Length;
                response.OutputStream.Write(indexPage, 0, indexPage.Length);
            }
            else if (context.Request.Url.LocalPath == "/js/func.js")
            {
                response.ContentType = "text/javascript; charset=Unicode";
                response.OutputStream.Write(JSfuncFile, 0, JSfuncFile.Length);
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
