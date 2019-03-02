using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore {
    public static class Core {
        private static Server.Listener listener;
        public static int Start() {
            if (Log.Start() == 0)
                return 2;
            Console.WriteLine($"{DateTime.Now.ToString()} [INFO][Core] Program starting!");
            //Console.WriteLine(BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.Default.GetBytes(Console.ReadLine()))).Replace("-", ""));

            Config.Read("config.xml");
            DBClient.Create();
            try {
                if (DBClient.Check()) {
                    listener = new Server.Listener();
                    listener.Start();
                    Console.WriteLine($"{DateTime.Now.ToString()} [INFO][Core] Program running!");
                    Log.Write(false, "INFO", "Core", "Program running!");
                }
            }
            catch (Exception ex) {
                Log.Write(true, "FATAL", "Core", $"Error: {ex.Message}", ex.StackTrace);
            }
            return 1;
        }
    }
}
