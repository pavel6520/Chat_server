using System;
using WebServerCore;

namespace Chat_server {
    class Program {
        static Core core;

        static void Main(string[] args) {
            core = new Core();
            core.Start();

            while (true) {
                string command = Console.ReadLine();
                if (command == "up") {

                }
                else if (command == "close")
                    return;
                else Console.WriteLine("Error command");
            }
        }
    }
}