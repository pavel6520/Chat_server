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
			Log = LogManager.GetLogger("SYSLOG");
            log4net.Config.XmlConfigurator.Configure();
            Log.Info("Program starting!");

            packageManager = new PluginManagerClass(ref Log, $"{Environment.CurrentDirectory}{System.IO.Path.DirectorySeparatorChar}application{System.IO.Path.DirectorySeparatorChar}");
        }

        public void Start() {
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
                Log.Fatal("Ошибка в главном потоке WebServerCore", ex);
				throw;
            }
        }

        public void Close() {
            listener.Stop();
			packageManager.WatcherStop();
            Log.Info("Program closed!");
        }
    }
}
