using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PackageManager {
    internal class PluginLoader : MarshalByRefObject {
        public string FilePath { get; set; }
        public string FullTypeName { get; set; }

        public void LoadInfos() {
            var assembly = AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(FilePath).FullName);
            foreach (var type in assembly.GetExportedTypes()) {
                if (type.IsAbstract)
                    continue;


                var currentBaseType = type.BaseType;
                while (currentBaseType != typeof(object)) {
                    if (string.Compare(currentBaseType.FullName, FullTypeName, StringComparison.OrdinalIgnoreCase) == 0) {
                        //pluginInfo.Add(new PluginInfo(assemblyPath, type.FullName));
                        break;
                    }
                    currentBaseType = currentBaseType.BaseType;
                }
            }
        }
    }

    [Serializable]
    public struct PluginInfo {
        public PluginInfo(string path, string[] pathSegments, string fullName, string typeName) {
            Path = path;
            PathSegments = pathSegments;
            FullName = fullName;
            TypeName = typeName;
        }

        public string Path { get; }
        public string[] PathSegments { get; }
        public string FullName { get; }
        public string TypeName { get; }
    }
}
