using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebServerCore;

namespace Chat_server {
    class Program {
        static void Main(string[] args) {
            Core.Start();

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