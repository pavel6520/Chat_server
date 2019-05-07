using System;
using System.Reflection;
using WebServerCore;

namespace ChatTestServer {
	class Program {
		static Core core;

		static void Main(string[] args) {
			Console.WriteLine($"{DateTime.Now} Запуск");

			string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
			Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));

			core = new Core();
			try {
				core.Start();
			}
			catch (Exception e){
				Console.WriteLine($"{DateTime.Now} Ошибка запуска: {e}");
				Console.Beep();
				Console.ReadKey();
				return;
			}
			Console.WriteLine($"{DateTime.Now} Запущено");
			while (true) {
				string command = Console.ReadLine();
				if (command == "up") { }
				else if (command == "close") {
					Console.WriteLine($"{DateTime.Now} Программа остановлена");
					core.Close();
					break;
				}
				else Console.WriteLine("Error command");
			}
		}
	}
}