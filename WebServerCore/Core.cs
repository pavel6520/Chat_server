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

        public Core() {
			string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
			Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
			//if (Environment.CurrentDirectory[Environment.CurrentDirectory.Length - 1] != System.IO.Path.DirectorySeparatorChar) {
			//	Environment.CurrentDirectory = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar;
			//}

			Log = LogManager.GetLogger("SYSLOG");
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("Program starting!");

            packageManager = new PluginManagerClass(ref Log, $"{Environment.CurrentDirectory}{System.IO.Path.DirectorySeparatorChar}application{System.IO.Path.DirectorySeparatorChar}");
        }

        public int Start() {
            //DBClient.Create();
            try {
                //if (DBClient.Check()) {
                listener = new Listener(ref Log, ref packageManager);
                listener.Prefixes.Add("http://*/");
                listener.Prefixes.Add("https://*/");
                listener.Start();
                
                Log.Info("Program running!");
                //}
            }
            catch (Exception ex) {
                //Console.WriteLine($"{DateTime.Now.ToString()} [FATAL][Core] Error running!");
                Log.Fatal("Ошибка в главном потоке WebServerCore", ex);
                return 2;
            }
            return 1;
        }

        public void Close() {
            listener.Stop();
			packageManager.WatcherStop();
            Log.Info("Program closed!");
        }
    }
}
