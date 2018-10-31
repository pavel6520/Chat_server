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
        byte[] jQuery;
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
            listener.Start();

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

            Console.WriteLine(DateTime.Now + " [INFO] HTTP-сервер запущен, порт " + port);
            return true;
        }

        public void Stop()
        {
            listener.Stop();
            Console.WriteLine(DateTime.Now + " [INFO] HTTP-сервер остановлен");
        }

        public void LoadFile()
        {
            indexPage = Encoding.Unicode.GetBytes(File.ReadAllText("index.html"));
            //jQuery = File.ReadAllBytes("jquery-3.3.1.js");
        }

        void FastResponse(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            ShowInfo(context);
            
            if (context.Request.Url.LocalPath == "/")
            {
                response.ContentType = "text/html; charset=Unicode";
                //response.ContentLength64 = indexPage.Length;
                response.OutputStream.Write(indexPage, 0, indexPage.Length);
            }
            /*else if (context.Request.Url.LocalPath == "/jquery-3.3.1.js")
            {
                //response.ContentType = "text/javascript; charset=Unicode";
                response.ContentLength64 = jQuery.Length;
                response.OutputStream.Write(jQuery, 0, jQuery.Length);
            }*/

            response.Close();
        }

        private void ShowInfo(HttpListenerContext context)
        {
            Console.WriteLine(DateTime.Now + " [DEBUG][HTTP] Запрос: " + context.Request.RemoteEndPoint + " " + context.Request.Url.AbsoluteUri);
        }
    }
}
