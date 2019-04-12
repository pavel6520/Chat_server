using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using ConnectionWorker;
using System.Net;
using log4net;
using System.Runtime.Remoting;
using System.Threading;

namespace WebServerCore.Plugins {
    class Plugin {
        public static ILog Log;
        private AppDomain domain;
        private string FullName;
		
		public bool isLoad { get; private set; }
        public string Name { get; private set; }

		internal Plugin(string name) {
			Name = name;
		}

		public void LoadPlugin(ref DirectoryInfo workD) {
			try {
				FileInfo workF = new FileInfo($"{workD.FullName}{Name}.dll");

				if (workF.Exists) {
					FullName = AssemblyName.GetAssemblyName(workF.FullName).FullName;

					var ds = new AppDomainSetup {
						ApplicationBase = workD.FullName
					};
					domain = AppDomain.CreateDomain($"{Name}Worker", AppDomain.CurrentDomain.Evidence, ds);
					((PluginWorker)GetPluginRefObject())._LoadDefault();
					isLoad = true;
				}
				else {
					throw new FileNotFoundException("Ошибка загрузки плагина: файл не найден", workF.FullName);
				}
			}
			catch {
				if (domain != null) {
					AppDomain.Unload(domain);
					domain = null;
				}
				isLoad = false;
			}
		}

		public void UnloadPlugin() {
			if (isLoad) {
				if (domain != null) {
					AppDomain.Unload(domain);
					domain = null;
				}
				isLoad = false;
			}
		}

        internal object GetPluginRefObject() {
            return domain.CreateInstanceAndUnwrap(FullName, Name);
        }
    }
}
