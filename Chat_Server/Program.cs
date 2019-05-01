using System;
using System.Reflection;
using WebServerCore;

namespace Chat_server {
	class Program {
		static Core core;

		static void Main(string[] args) {
			Console.WriteLine($"{DateTime.Now} Запуск");

			string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
			Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));

			core = new Core();
			core.Start();
			Console.WriteLine($"{DateTime.Now} Запущено");
			while (true) {
				string command = Console.ReadLine();
				if (command == "up") { }
				else if (command == "close") {
					Console.WriteLine($"{DateTime.Now} Program close");
					core.Close();
					break;
				}
				else Console.WriteLine("Error command");
			}
		}
	}
}