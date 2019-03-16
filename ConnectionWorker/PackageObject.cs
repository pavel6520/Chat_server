using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;

namespace ConnectionWorker {
    class PackageObject {
        private MethodInfo[] actions;
        private Assembly assembly;
        private object instance;
        private object instanceType;
        private List<PackageObject> packages;
        private DirectoryInfo directory;
        
        public string Name { get { return directory.Name; } }

        internal PackageObject(string Directory) {
            packages = new List<PackageObject>(0);

            directory = new DirectoryInfo(Directory);

            assembly = Assembly.LoadFrom($"{directory.FullName}\\{Name}Controller.dll");
            instance = assembly.CreateInstance($"{Name}Controller");
            Type instanceType = assembly.GetType($"{Name}Controller");
            //instanceType.
            
            

            string[] dir = System.IO.Directory.GetDirectories(directory.FullName);
            foreach (var item in dir) {
                DirectoryInfo tmp = new DirectoryInfo(item);
                packages.Add(new PackageObject(item));
            }
        }
    }
}
