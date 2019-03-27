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

namespace WebServerCore.Plugins {
    class Plugin {
        public static ILog Log;
        private DirectoryInfo workD;
        private AppDomain domain;
        private string FullName;
		private string type;
		
		public bool isLoad { get; private set; }
        public string Name { get; private set; }
		
		internal Plugin(DirectoryInfo workD, string name, string type) {
			this.workD = workD;
			this.type = type;
			Name = name;
		}

		public void LoadPlugin() {
			try {
				FileInfo workF = new FileInfo($"{workD.FullName}{Name}{type}.dll");

				if (workF.Exists) {
					FullName = AssemblyName.GetAssemblyName(workF.FullName).FullName;

					var ds = new AppDomainSetup {
						ApplicationBase = workD.FullName,
						ApplicationName = $"WebServerCore {Name}{type}Worker"
					};
					domain = AppDomain.CreateDomain($"{Name}{type}Worker", AppDomain.CurrentDomain.Evidence, ds);
					isLoad = true;
				}
				else {
					throw new FileNotFoundException("Ошибка загрузки контроллера: файл не найден", workF.FullName);
				}
			}
			catch {
				if (domain != null) {
					AppDomain.Unload(domain);
					domain = null;
				}
				isLoad = false;
				throw;
			}
		}

		public void UnloadPlugin() {
			AppDomain.Unload(domain);
			domain = null;
			isLoad = false;
		}

        internal object GetPluginRefObject() {
            return domain.CreateInstanceAndUnwrap(FullName, $"{Name}{type}");
        }
    }
}
