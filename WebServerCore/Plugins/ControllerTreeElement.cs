using ConnectionWorker;
using System.Collections;
using System.IO;

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

			public void Load(ref DirectoryInfo baseD) {
				if (!isLoad || plugin != null) {
					plugin._FileCompare(ref baseD, "controllers", FullPath);
					plugin.LoadPlugin();
					try {
						if (plugin.isLoad) {
							ControllerWorker controller = (ControllerWorker)plugin.GetPluginRefObject();
							Actions = controller._GetActionList();
						}
					}
					catch {
						plugin.UnloadPlugin();
					}
					isLoad = plugin.isLoad;
				}
			}

			public void Unload() {
				if (isLoad) {
					plugin.UnloadPlugin();
				}
			}

			public void Delete() {
				Unload();
				foreach (DictionaryEntry item in elements) {
					((ControllerTreeElement)item.Value).Delete();
					elements.Remove(item.Key);
					if (elements.Count == 0) {
						break;
					}
				}
			}

			public void RemoveTree(string name) {
				((ControllerTreeElement)elements[name]).Delete();
				elements.Remove(name);
			}
		}
    }
}