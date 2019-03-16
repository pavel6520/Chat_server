using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker {
    class PackageManager {
        private string Path;
        private PackageObject package;

        public PackageManager(string path) {
            Path = Environment.CurrentDirectory + path + "controllers";

            package = new PackageObject(Path);
        }
    }
}
