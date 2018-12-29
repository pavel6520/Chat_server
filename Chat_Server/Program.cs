using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Chat_server
{
    class Program
    {
        static HTTP_Server http;
        static TCP_Server tcp;
        public static MySql_Client mySql;
        public static bool DEBUG { get; private set; }
        public static List<string> errorLog;

        static void Main(string[] args)
        {
            DEBUG = true;
            StartLog();
            mySql = new MySql_Client("127.0.0.1", 3306, "root", "6520");
            try
            {
                if (mySql.CheckConnect())
                {
                    Console.WriteLine(DateTime.Now + " [INFO][MYSQL] Успешно подключено");

                    http = new HTTP_Server(8080);
                    tcp = new TCP_Server(30000);

                    http.Start();
                    tcp.Start();

                    while (true)
                    {
                        string command = Console.ReadLine();
                        if (command == "up")
                            http.LoadFile();
                        else if (command == "close")
                        {
                            http.Stop();
                            tcp.Stop();
                            return;
                        }
                        else Console.WriteLine("Error command");
                    }
                }
                else
                {
                    Console.WriteLine(DateTime.Now + " [ERROR][MYSQL] Ошибка подключения");
                    Console.ReadKey();
                }
            }
            catch(Exception ex)
            {
                errorLog.Add("[FATAL]" + ex.Message);
            }
        }

        public static void StartLog()
        {
            errorLog = new List<string>();
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    while (errorLog.Count > 0)
                    {
                        File.AppendAllText("error.log", DateTime.Now + " " + errorLog[0] + "\r\n");
                        errorLog.RemoveAt(0);
                    }
                }
            });
        }
    }
}
