using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat_server
{
    static class Log
    {
        private static object lockObj1 = new object();
        private static object lockObj2 = new object();

        public static void Write(bool error, string type, string module, string message)
        {
            DateTime time = DateTime.Now;
            string forWrite = $"{time.ToString()} [{type}][{module}] {message}\r\n";
            Task.Factory.StartNew(() =>
            {
                if (!error)
                    lock (lockObj1)
                        File.AppendAllText("logs/log.log", forWrite);
                else
                    lock (lockObj2)
                        File.AppendAllText("logs/logError.log", forWrite);
            });
        }
    }
}
