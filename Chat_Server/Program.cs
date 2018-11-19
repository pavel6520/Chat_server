using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chat_server
{
    class Program
    {
        static HTTP_Server http;
        static TCP_Server tcp;
        public static MySql_Client mySql;
        public static bool DEBUG { get; private set; }

        static void Main(string[] args)
        {
            DEBUG = true;
            mySql = new MySql_Client("127.0.0.1", 3306, "root", "65207634");

            if (mySql.CheckConnect())
            {
                Console.WriteLine(DateTime.Now + " [INFO][MYSQL] Успешно подключено");

                http = new HTTP_Server(8080);
                tcp = new TCP_Server(30000);

                http.Start();
                tcp.Start();

                while (true)
                {
                    Console.ReadKey();
                    http.LoadFile();
                }
            }
            else
            {
                Console.WriteLine(DateTime.Now + " [ERROR][MYSQL] Ошибка подключения");
                Console.ReadKey();
            }
        }
    }
}
