using System;
using System.Reflection;
using WebServerCore;

namespace Chat_server {
    class Program {
        static Core core;

        static void Main(string[] args) {
            {
                
				//Config.Read("config.xml");
				core = new Core();
                core.Start();
            }
            while (true) {
                string command = Console.ReadLine();
                if (command == "up") {

                }
                else if (command == "close") {
                    Console.WriteLine($"{DateTime.Now} Program close");
                    core.Close();
                    Environment.Exit(0);
                }
                else Console.WriteLine("Error command");
            }
        }
    }
}