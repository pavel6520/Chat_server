using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using WebServerCore.Connection.Http;
using ConnectionWorker;

namespace PackageManager {
    class ControllerWorker {
        private DirectoryInfo baseDirectory;
        private AppDomain domain;
        private bool isLoadController;
        private List<ControllerWorker> packages;
        private string FullName;
        private string NameController;

        public string[] Actions;
        public bool Exist { get { return baseDirectory.Exists; } }
        public string urlPath { get; private set; }
        public string Name { get; private set; }

        internal ControllerWorker(ref DirectoryInfo baseDirectory, string urlPath, string Name) {
            this.baseDirectory = baseDirectory;
            this.urlPath = urlPath;
            this.Name = Name;
            isLoadController = false;
            
            DirectoryInfo baseD = new DirectoryInfo($"{baseDirectory.FullName}controllers{urlPath}");
            DirectoryInfo workD = new DirectoryInfo($"{baseDirectory.FullName}controllersWork{urlPath}");
            if (workD.Exists) {
                workD.Delete(true);
            }
            workD.Create();

            if (Name != null) {
                Console.WriteLine($"Загрузка контроллера {Name}");
                isLoadController = LoadController(baseD, workD);
                if (isLoadController) {
                    PackageManagerClass.Log.Info($"Загружен контроллер {urlPath}Controller");
                }
            }
            else {
                Actions = new string[0];
            }

            packages = new List<ControllerWorker>(0);
            foreach (var item in baseD.GetDirectories()) {
                if (Actions.Contains($"{item.Name}Action")) {
                    PackageManagerClass.Log.Error($"Попытка загрузить директорию, хотя уже есть такой Action: {urlPath}{item.Name}");
                }
                else {
                    packages.Add(new ControllerWorker(ref baseDirectory, $"{urlPath}{item.Name}{Path.DirectorySeparatorChar}", item.Name));
                }
            }
        }

        internal void CopyFileIfUpdate(DirectoryInfo baseD, DirectoryInfo workD) {
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
        }

        internal bool LoadController(DirectoryInfo baseD, DirectoryInfo workD) {
            NameController = $"{this.Name}Controller";
            string NameControllerDll = $"{this.Name}Controller.dll";
            FileInfo baseF = new FileInfo($"{baseD.FullName}{NameControllerDll}");
            if (baseF.Exists) {
                CopyFileIfUpdate(baseD, workD);
                
                FileInfo workF = new FileInfo($"{workD.FullName}{NameControllerDll}");
                workF.Refresh();

                if (workF.Exists) {
                    FullName = AssemblyName.GetAssemblyName(workF.FullName).FullName;

                    var ds = new AppDomainSetup {
                        ApplicationBase = workD.FullName,
                        ApplicationName = $"{NameController}Worker"
                    };
                    domain = AppDomain.CreateDomain($"{NameController}Worker", AppDomain.CurrentDomain.Evidence, ds);

                    //try {
                        ConnectionController_class remoteWorker1 = (ConnectionController_class)domain.CreateInstanceAndUnwrap(FullName, NameController);
                        Actions = remoteWorker1.GetActionList();
                    //}
                    //catch (ReflectionTypeLoadException e) {
                    //    AppDomain.Unload(domain);
                    //}
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

        internal void WorkHttp(ref HttpContext context, List<string> path) {
            string action;
            if (path.Count == 0) {
                action = "index";
            }
            else {
                action = path[0].ToLower();
                path.RemoveAt(0);
            }
            if (isLoadController && Actions.Contains($"{action}Action")) {
                try {
                    ConnectionController_class remoteWorker1 = (ConnectionController_class)domain.CreateInstanceAndUnwrap(FullName, NameController);
                    //remoteWorker1.SetContext(ref context);
                    string bufS = remoteWorker1.Render($"{action}Action");

                    context.Response.StatusDescription = "OK";
                    byte[] buf = Encoding.UTF8.GetBytes(bufS);
                    context.Response.ContentLength64 = bufS.Length;
                    context.Response.ContentType = "text/html; charset=UTF8";
                    WebServerCore.Connection.ConnectionWrite write = context.Response.GetWriteStream();
                    write.Write(buf);
                }
                finally {
                }
            }
            else {
                foreach(var package in packages) {
                    if (package.Name == action) {
                        package.WorkHttp(ref context, path);
                        return;
                    }
                }
                throw new Exception("not found error");
            }
        }
    }
}
