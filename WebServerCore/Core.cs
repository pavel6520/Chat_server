using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Plugins;

namespace WebServerCore {
    public class Core {
        private Listener listener;
        private PluginManagerClass packageManager;
        public ILog Log;

        public Core(string pathToWorkDirectory) {
            Log = LogManager.GetLogger("SYSLOG");
            log4net.Config.XmlConfigurator.Configure();
            Console.WriteLine($"{DateTime.Now.ToString()} [INFO][Core] Program starting!");
            Log.Info("Program starting!");
            packageManager = new PluginManagerClass(ref Log, 
                $"{pathToWorkDirectory}application{System.IO.Path.DirectorySeparatorChar}");
        }

        public int Start() {
            //DBClient.Create();
            //try {
                //if (DBClient.Check()) {
                listener = new Listener(ref Log, ref packageManager);
                listener.Prefixes.Add("http://*/");
                listener.Prefixes.Add("https://*/");
                listener.Start();
                
                Console.WriteLine($"{DateTime.Now.ToString()} [INFO][Core] Program running!");
                Log.Info("Program running!");
                //}
            //}
            //catch (Exception ex) {
            //    Console.WriteLine($"{DateTime.Now.ToString()} [FATAL][Core] Error running!");
            //    Log.Fatal("Ошибка в главном потоке WebServerCore", ex);
            //    return 2;
            //}
            return 1;
        }

        public void Close() {
            listener.Stop();
            Log.Info("Program closed!");
        }
    }
}
