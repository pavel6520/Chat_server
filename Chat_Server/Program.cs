using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chat_server
{
    class Program
    {
        static HTTP_Server http;
        static TCP_server tcp;

        static void Main(string[] args)
        {
            http = new HTTP_Server(80);
            tcp = new TCP_server(8080);
            http.Start();
            tcp.Start();
            while (true)
            {
                Console.ReadKey();
                http.LoadFile();
            }
        }
    }
}
