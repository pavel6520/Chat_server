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
        private DirectoryInfo baseD;
        private DirectoryInfo workD;
        private AppDomain domain;
        private string FullName;
		private string type;

        public string[] Actions;
		public bool isLoad { get; private set; }
        public string UrlPath { get; private set; }
        public string Name { get; private set; }

        internal Plugin(DirectoryInfo baseD, DirectoryInfo workD, string name, string type) {
            this.baseD = baseD;
            this.workD = workD;
			this.type = type;
			Name = name;
            isLoad = false;
			try {
				isLoad = LoadPlugin();
			}
			catch { throw; }
			finally {
				if (!isLoad) {
					Actions = new string[0];
				}
			}
        }
		
        internal bool LoadPlugin() {
            string NameTypeDll = $"{Name}{type}.dll";
            FileInfo baseF = new FileInfo($"{baseD.FullName}{NameTypeDll}");
            if (baseF.Exists) {
				List<string> filesWork = new List<string>();
				foreach (var item in workD.GetFiles()) {
					filesWork.Add(item.Name);
				}
				foreach (var item in baseD.GetFiles()) {
					if (File.Exists($"{workD.FullName}{item.Name}")) {
						filesWork.Remove(item.Name);
						if (File.GetLastWriteTimeUtc($"{workD.FullName}{item.Name}") < item.LastWriteTimeUtc) {
							item.CopyTo($"{workD.FullName}{item.Name}", true);
						}
					}
					else {
						item.CopyTo($"{workD.FullName}{item.Name}", true);
					}
				}
				foreach (var item in filesWork) {
					File.Delete($"{workD.FullName}{item}");
				}

				FileInfo workF = new FileInfo($"{workD.FullName}{NameTypeDll}");
                workF.Refresh();

                if (workF.Exists) {
                    FullName = AssemblyName.GetAssemblyName(workF.FullName).FullName;

					var ds = new AppDomainSetup {
						ApplicationBase = workD.FullName,
						ApplicationName = $"WebServerCore {Name}{type}Worker"
					};
					domain = AppDomain.CreateDomain($"{Name}{type}Worker", AppDomain.CurrentDomain.Evidence, ds);

                    try {
                        ControllerWorker remoteWorker1 = (ControllerWorker)domain.CreateInstanceAndUnwrap(FullName, $"{Name}{type}");
                        Actions = remoteWorker1._GetActionList();
                    }
                    catch (TypeLoadException e) {
                        AppDomain.Unload(domain);
                        Log.Error("Ошибка загрузки контроллера", e);
                    }
                    catch (RemotingException e) {
                        AppDomain.Unload(domain);
                        Log.Error($"Не найден метод базового класса в контроллере {UrlPath}", e);
                    }
                    //catch (TargetInvocationException e) {
                    //    AppDomain.Unload(domain);
                    //}
                    return true;
                }
                else {
                    throw new FileNotFoundException("Ошибка загрузки контроллера: файл не найден", workF.FullName);
                }
            }
            return false;
        }

        internal PluginWorker GetCrossDomainObject() {
            return (PluginWorker)domain.CreateInstanceAndUnwrap(FullName, $"{Name}{type}");
        }
    }
}
