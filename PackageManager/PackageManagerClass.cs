using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Connection.Http;

namespace PackageManager {
    public class PackageManagerClass {
        private DirectoryInfo baseDirectory;
        private ControllerWorker controllerTree;

        public static ILog Log;

        public PackageManagerClass(string path) {
            Log = LogManager.GetLogger("SYSLOG");
            log4net.Config.XmlConfigurator.Configure();
            baseDirectory = new DirectoryInfo(Environment.CurrentDirectory + path);
            if (!baseDirectory.Exists) {
                baseDirectory.Create();
            }
            string tmp = $"{baseDirectory.FullName}controllers";
            if (!Directory.Exists(tmp)) {
                Directory.CreateDirectory(tmp);
            }
            tmp = $"{baseDirectory.FullName}controllersWork";
            if (!Directory.Exists(tmp)) {
                Directory.CreateDirectory(tmp);
            }
            //tmp = $"{baseDirectory.FullName}service";
            //if (!Directory.Exists(tmp)) {
            //    Directory.CreateDirectory(tmp);
            //}
            //tmp = $"{baseDirectory.FullName}serviceWork";
            //if (!Directory.Exists(tmp)) {
            //    Directory.CreateDirectory(tmp);
            //}

            controllerTree = new ControllerWorker(ref baseDirectory, $"{Path.DirectorySeparatorChar}", null);
        }

        public void WorkHttp(ref HttpContext context) {
            List<string> vs = new List<string>(context.Request.UriParse.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            if (vs.Count == 0) {
                vs.Add("index");
            }
            controllerTree.WorkHttp(ref context, vs);
        }
    }
}