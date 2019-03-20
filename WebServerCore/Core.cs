using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore {
    public class Core {
        private Server.Listener listener;
        public static ILog Log;

        public int Start() {
            Log = LogManager.GetLogger("SYSLOG");
            log4net.Config.XmlConfigurator.Configure();
            
            Console.WriteLine($"{DateTime.Now.ToString()} [INFO][Core] Program starting!");
            Log.Info("Program starting!");
            //Console.WriteLine(BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.Default.GetBytes(Console.ReadLine()))).Replace("-", ""));

            //DBClient.Create();
            try {
                //if (DBClient.Check()) {
                listener = new Server.Listener();
                listener.StartListener(System.Net.IPAddress.Any, 80, "127.0.0.1");

                //listener.Start();
                Console.WriteLine($"{DateTime.Now.ToString()} [INFO][Core] Program running!");
                Log.Info("Program running!");
                //}
            }
            catch (Exception ex) {
                Log.Fatal("Ошибка в главном потоке WebServerCore", ex);
                return 2;
            }
            return 1;
        }
    }
}
