using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore {
    public static class Log {
        private static object lockObj1 = new object();
        private static object lockObj2 = new object();
        private static string path = "logs";
        private static string pathInfo = path + "/info.log";
        private static string pathDebug = path + "/debug.log";

        public static int Start() {
            try {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            }
            catch (Exception ex) {
                Console.WriteLine($"{DateTime.Now.ToString()} [FATAL][LOG] Не удалось создать каталог {path} с ошибкой: {ex.Message}");
                return 1;
            }
            try {
                lock (lockObj1)
                    File.AppendAllText(pathInfo, $"\r\n\r\n\r\n{DateTime.Now.ToString()} [INFO][Core] Program starting!\r\n");
                lock (lockObj2)
                    File.AppendAllText(pathDebug, $"\r\n\r\n\r\n{DateTime.Now.ToString()} [INFO][Core] Program starting!\r\n");
            }
            catch (Exception ex) {
                Console.WriteLine($"{DateTime.Now.ToString()} [FATAL][LOG] Не удалось создать каталог {path} с ошибкой: {ex.Message}");
                return 2;
            }
            return 0;
        }

        public static void Write(LogType type, string module, string message, string exTrace = null) {
            string forWrite = $"{DateTime.Now.ToString()} [{type.ToString()}][{module}] {message}\r\n{(exTrace == null ? "" : $"StackTrace------------------------\r\n{exTrace}\r\nStackTraceEnd---------------------\r\n")}";
            Task.Factory.StartNew(() => {
                try {
                    lock (lockObj1)
                        File.AppendAllText(pathDebug, forWrite);
                    if (type != LogType.DEBUG)
                        lock (lockObj2)
                            File.AppendAllText(pathInfo, forWrite);
                }
                catch (Exception ex) {
                    Console.WriteLine($"{DateTime.Now.ToString()} [FATAL][LOG] Не удалось записать файл лога с ошибкой: {ex.Message}");
                    Console.WriteLine("Press any key to exit");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            });
        }
    }

    public enum LogType {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4
    }
}
