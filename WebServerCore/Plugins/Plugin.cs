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
						ApplicationBase = workD.FullName
					};
					domain = AppDomain.CreateDomain($"{Name}{type}Worker", AppDomain.CurrentDomain.Evidence, ds);
					((PluginWorker)GetPluginRefObject())._LoadDefault();
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
			}
		}

		public void _FileCompare(ref DirectoryInfo baseD, string module, string path) {
			DirectoryInfo treeD = new DirectoryInfo($"{baseD.FullName}{module}{path}{Path.DirectorySeparatorChar}");
			DirectoryInfo workD = new DirectoryInfo($"{baseD.FullName}{module}Work{path}{Path.DirectorySeparatorChar}");
			List<string> filesWork = new List<string>();
			foreach (var item in workD.GetFiles()) {
				filesWork.Add(item.Name);
			}
			foreach (var item in treeD.GetFiles()) {
				string workF = $"{workD.FullName}{item.Name}";
				if (filesWork.Contains(item.Name)) {
					filesWork.Remove(item.Name);
					if (File.GetLastWriteTimeUtc(workF) < item.LastWriteTimeUtc) {
						item.CopyTo(workF, true);
					}
				}
				else {
					while (true) {
						try {
							item.CopyTo(workF, true);
							break;
						}
						catch {
							Thread.Sleep(50);
						}
					}
				}
			}
			foreach (var item in filesWork) {
				File.Delete($"{workD.FullName}{item}");
			}
		}

		public void UnloadPlugin() {
			if (domain != null) {
				AppDomain.Unload(domain);
				domain = null;
			}
			isLoad = false;
		}

        internal object GetPluginRefObject() {
            return domain.CreateInstanceAndUnwrap(FullName, $"{Name}{type}");
        }
    }
}
