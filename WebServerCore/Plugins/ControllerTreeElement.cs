using System.Collections.Generic;

namespace WebServerCore.Plugins {
	public partial class PluginManagerClass {
		private class ControllerTreeElement {
			//public string Name;
			//public string FullPath;
			//public ControllerSettings Settings;
			//public string[] Actions;
			public Plugin plugin;
			public List<ControllerTreeElement> elements;
			//public System.Collections.Hashtable elementsTMP;
		}
    }
}