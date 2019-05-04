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
			log4net.Config.XmlConfigurator.Configure();
			Log = LogManager.GetLogger("INFOLOG");
			Log.Info("Program starting!");

			try {
				Config.Read("config.ini");
			}
			catch (Exception e) {
				Log.Fatal("Ошибка чтения конфигурации", e);
				Environment.Exit(1);
			}
			if (Config.Debug) {
				Log = LogManager.GetLogger("DEBUGLOG");
			}

			packageManager = new PluginManagerClass(ref Log, $"{Environment.CurrentDirectory}{System.IO.Path.DirectorySeparatorChar}application{System.IO.Path.DirectorySeparatorChar}");
        }

        public void Start() {
            //DBClient.Create();
            try {
                //if (DBClient.Check()) {
                listener = new Listener(ref Log, ref packageManager);
                listener.Prefixes.Add("http://*/");
				if (Config.SSLEnable) {
					listener.Prefixes.Add("https://*/");
				}
                listener.Start();
                
                Log.Info("Program running!");
                //}
            }
            catch (Exception ex) {
                Log.Fatal("Ошибка в главном потоке WebServerCore", ex);
				Environment.Exit(1);
			}
        }

        public void Close() {
            listener.Stop();
			packageManager.WatcherStop();
            Log.Info("Program closed!");
        }
    }
}
