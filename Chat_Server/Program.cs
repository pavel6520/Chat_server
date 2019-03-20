using System;
using System.Reflection;
using WebServerCore;

namespace Chat_server {
    class Program {
        static Core core;

        static void Main(string[] args) {
            string path = Assembly.GetExecutingAssembly().Location;
            Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
            
            //Config.Read("config.xml");
            core = new Core();
            core.Start();

            while (true) {
                string command = Console.ReadLine();
                if (command == "up") {

                }
                else if (command == "close") {
                    Console.WriteLine($"{DateTime.Now} Program close");
                    return;
                }
                else Console.WriteLine("Error command");
            }
        }
    }
}