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
        public static bool DEBUG { get; private set; }
        public static List<string> errorLog;

        static void Main(string[] args)
        {
            DB.UserContext context = new DB.UserContext();
            DB.User user = new DB.User { Id = 2, Login = "test", Email = "pavmuz2012@yandex.ru", Password = "123", Salt = "123", RegDate = DateTime.Now };
            context.Users.Add(user);

            context.SaveChanges();

            //var users = context.Users;
            Console.WriteLine("Список объектов:");
            foreach (DB.User u in context.Users)
            {
                Console.WriteLine("{0} {1} : {2}", u.Id, u.Login, u.RegDate);
            }
            Console.ReadKey();


            //DEBUG = true;
            //StartLog();
            //try
            //{
            //    if (MySql_Client.SetData_Check("127.0.0.1", 3306, "root", "6520"))
            //    {
            //        Console.WriteLine(DateTime.Now + " [INFO][MYSQL] Успешно подключено");

            //        http = new HTTP_Server(8080);
            //        tcp = new TCP_Server(30000);

            //        http.Start();
            //        tcp.Start();

            //        while (true)
            //        {
            //            string command = Console.ReadLine();
            //            if (command == "up")
            //                http.LoadFile();
            //            else if (command == "close")
            //            {
            //                http.Stop();
            //                tcp.Stop();
            //                return;
            //            }
            //            else Console.WriteLine("Error command");
            //        }
            //    }
            //    else
            //    {
            //        Console.WriteLine(DateTime.Now + " [ERROR][MYSQL] Ошибка подключения");
            //        Console.ReadKey();
            //    }
            //}
            //catch (Exception e)
            //{
            //    errorLog.Add(DateTime.Now + " [FATAL]" + e.Message);
            //}


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
                        File.AppendAllText("app.log", errorLog[0] + "\r\n");
                        errorLog.RemoveAt(0);
                    }
                }
            });
        }
    }
}
