using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Plugins {
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
}
