using System.Collections;
using System.Collections.Generic;

namespace WebServerCore.Plugins {
	public partial class PluginManagerClass {
		private class ControllerTreeElement {
			public bool isLoad { get; private set; }
			public string Name;
			public string FullPath;
			//public ControllerSettings Settings;
			public string[] Actions;
			public Plugin plugin;
			public Hashtable elements;

			public ControllerTreeElement() {
				isLoad = false;
			}

			public void Load() {
				if (!isLoad && plugin != null) {
					plugin.LoadPlugin();
					isLoad = plugin.isLoad;
				}
			}

			public void Unload() {
				if (isLoad) {
					plugin.UnloadPlugin();
				}
			}

			public void Delete() {
				if (isLoad)
					plugin.UnloadPlugin();
				foreach(DictionaryEntry item in elements) {
					((ControllerTreeElement)item.Value).Delete();
					elements.Remove(item.Key);
				}
			}

			public void RemoveTree(string name) {
				((ControllerTreeElement)elements[name]).Delete();
				elements.Remove(name);
			}
		}
    }
}