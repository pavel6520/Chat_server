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
        byte[] FileIndex_html;
        byte[] FileFunc_js;
        byte[] FileStruct_js;
        byte[] FileStyle_css;
        byte[] FileFavicon;
        byte[] FileJQuery_js;
        byte[] FileJQueryUI_js;
        byte[] FileJQueryUI_css;
        byte[] FileJQueryUITheme_css;
        byte[] FileJQueryUIStructure_css;
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
            FileIndex_html = Encoding.UTF8.GetBytes(File.ReadAllText("client/index.html")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileStyle_css = Encoding.UTF8.GetBytes(File.ReadAllText("client/style.css")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileFunc_js = Encoding.UTF8.GetBytes(File.ReadAllText("client/func.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileStruct_js = Encoding.UTF8.GetBytes(File.ReadAllText("client/struct.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileJQuery_js = Encoding.UTF8.GetBytes(File.ReadAllText("client/jquery.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileJQueryUI_js = Encoding.UTF8.GetBytes(File.ReadAllText("client/jquery-ui.js")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileJQueryUI_css = Encoding.UTF8.GetBytes(File.ReadAllText("client/jquery-ui.css")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileJQueryUITheme_css = Encoding.UTF8.GetBytes(File.ReadAllText("client/jquery-ui.theme.css")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileJQueryUIStructure_css = Encoding.UTF8.GetBytes(File.ReadAllText("client/jquery-ui.structure.css")/*.Replace("\r\n", "").Replace("\t", "")*/);
            FileFavicon = File.ReadAllBytes("favicon.ico");
        }

        void FastResponse(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;
            string[] RequestPath = context.Request.Url.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            
                Console.WriteLine(DateTime.Now + " [DEBUG][HTTP] Запрос: " + context.Request.RemoteEndPoint + " " + context.Request.Url.AbsoluteUri);
                //DEBUG
                /*for (int i = 0; i < context.Request.Headers.Count; i++)
                    Console.WriteLine(context.Request.Headers.Keys[i] + ": " + context.Request.Headers[i]);
                Console.WriteLine();*/

            if (RequestPath.Length == 0)
            {
                response.ContentType = "text/html; charset=UTF8";
                response.OutputStream.Write(FileIndex_html, 0, FileIndex_html.Length);
            }
            else
            {
                switch (RequestPath[0])
                {
                    case "client":
                        if (RequestPath.Length == 1)
                        {
                            response.ContentType = "text/html; charset=UTF8";
                            response.OutputStream.Write(FileIndex_html, 0, FileIndex_html.Length);
                        }
                        else
                        {
                            switch (RequestPath[1])
                            {
                                case "jquery.js":
                                    response.ContentType = "text/javascript; charset=UTF8";
                                    response.OutputStream.Write(FileJQuery_js, 0, FileJQuery_js.Length);
                                    break;
                                case "style.css":
                                    response.ContentType = "text/css; charset=UTF8";
                                    response.OutputStream.Write(FileStyle_css, 0, FileStyle_css.Length);
                                    break;
                                case "func.js":
                                    response.ContentType = "text/javascript; charset=UTF8";
                                    response.OutputStream.Write(FileFunc_js, 0, FileFunc_js.Length);
                                    break;
                                case "struct.js":
                                    response.ContentType = "text/javascript; charset=UTF8";
                                    response.OutputStream.Write(FileStruct_js, 0, FileStruct_js.Length);
                                    break;
                                case "jquery-ui.css":
                                    response.ContentType = "text/css; charset=UTF8";
                                    response.OutputStream.Write(FileJQueryUI_css, 0, FileJQueryUI_css.Length);
                                    break;
                                case "jquery-ui.js":
                                    response.ContentType = "text/javascript; charset=UTF8";
                                    response.OutputStream.Write(FileJQueryUI_js, 0, FileJQueryUI_js.Length);
                                    break;
                                case "jquery-ui.structure.css":
                                    response.ContentType = "text/css; charset=UTF8";
                                    response.OutputStream.Write(FileJQueryUIStructure_css, 0, FileJQueryUIStructure_css.Length);
                                    break;
                                case "jquery-ui.theme.css":
                                    response.ContentType = "text/css; charset=UTF8";
                                    response.OutputStream.Write(FileJQueryUITheme_css, 0, FileJQueryUITheme_css.Length);
                                    break;
                                case "images":
                                    if (RequestPath.Length == 3)
                                    {
                                        try
                                        {
                                            byte[] img = File.ReadAllBytes($"{RequestPath[0]}/{RequestPath[1]}/{RequestPath[2]}");
                                            response.OutputStream.Write(img, 0, img.Length);
                                        }
                                        catch { }
                                    }
                                    else{
                                        //NOT FOUND
                                    }
                                    break;
                            }
                        }
                        break;
                    case "":
                        break;
                    case "index.html":
                        response.ContentType = "text/html; charset=UTF8";
                        response.OutputStream.Write(FileIndex_html, 0, FileIndex_html.Length);
                        break;
                    case "favicon.ico":
                        response.ContentType = "text/javascript; charset=UTF8";
                        response.OutputStream.Write(FileFavicon, 0, FileFavicon.Length);
                        break;
                }
            }

            response.Close();
        }
    }
}
