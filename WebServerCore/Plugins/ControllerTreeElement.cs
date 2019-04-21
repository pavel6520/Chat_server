using ConnectionWorker;
using System.Collections;
using System.IO;
using System.Linq;

namespace WebServerCore.Plugins {
	public partial class PluginManagerClass {
		private class ControllerTreeElement {
			public static string[] Module;
			public static DirectoryInfo baseDirectory;

			public string[] Actions;
			public string[] ActionsWS;
			public string Name { get { return loader.Name; } }
			public string AbsPath { get { return loader == null ? "" : loader.AbsPath; } }
			public PluginLoader loader;
			public Plugin plugin { get { return loader.plugin; } }
			public bool isLoad { get { return loader == null ? false : loader.isLoad; } }
			public Hashtable elements;

			public ControllerTreeElement() {
				elements = new Hashtable();

				DirectoryInfo[] array = new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}{Path.DirectorySeparatorChar}").GetDirectories();
				for (int i = 0; i < array.Length; i++) {
					DirectoryInfo dirInTree = array[i];
					AddTree(dirInTree.Name);
				}
			}

			public ControllerTreeElement(string absPath, string name) {
				elements = new Hashtable();
				loader = new PluginLoader(ref Module, absPath, name);
				Load();

				DirectoryInfo[] array = new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}{AbsPath}").GetDirectories();
				for (int i = 0; i < array.Length; i++) {
					DirectoryInfo dirInTree = array[i];
					AddTree(dirInTree.Name);
				}
			}

			public void Load() {
				loader.Load();
				try {
					if (isLoad) {
						ControllerWorker controller = (ControllerWorker)loader.plugin.GetPluginRefObject();
						Actions = controller._GetActions();
						ActionsWS = controller._GetActionsWS();
					}
				}
				catch {
					loader.Unload();
				}
			}

			public void Unload() {
				loader.Unload();
			}

			public void AddTree(string name) {
				if (elements.Contains(name)) {
					((ControllerTreeElement)elements[name]).Delete();
					elements.Remove(name);
				}
				elements.Add(name, new ControllerTreeElement($"{AbsPath}{Path.DirectorySeparatorChar}{name}", name));
			}

			public void RemoveFiles() {
				loader.RemoveWorkFiles();
			}

			public void RemoveTree(string name) {
				((ControllerTreeElement)elements[name]).Delete();
				elements.Remove(name);
			}

			public void Delete() {
				foreach (DictionaryEntry item in elements) {
					(item.Value as ControllerTreeElement).Delete();
					elements.Remove(item.Key);
					if (elements.Count == 0) {
						break;
					}
				}
				loader.RemoveWorkDir();
			}

			public ControllerTreeElement Search(ref System.Collections.Generic.Queue<string> pathSplit, out string serched) {
				string action = pathSplit.Count == 0 ? "index" : pathSplit.Dequeue().ToLower();
				if (isLoad && Actions.Contains(action)) {
					serched = action;
					return this;
				}
				else {
					if (elements.Contains(action)) {
						ControllerTreeElement treeElement = (ControllerTreeElement)elements[action];
						return treeElement.Search(ref pathSplit, out serched);
					}
					else {
						serched = null;
						return null;
					}
				}
			}

			public ControllerTreeElement SearchWS(ref System.Collections.Generic.Queue<string> pathSplit, out string serched) {
				string action = pathSplit.Count == 0 ? "index" : pathSplit.Dequeue().ToLower();
				if (isLoad && ActionsWS.Contains(action)) {
					serched = action;
					return this;
				}
				else {
					if (elements.Contains(action)) {
						ControllerTreeElement treeElement = (ControllerTreeElement)elements[action];
						return treeElement.Search(ref pathSplit, out serched);
					}
					else {
						serched = null;
						return null;
					}
				}
			}

			public ControllerTreeElement SearchRoot(ref System.Collections.Generic.Queue<string> pathSplit) {
				string action = pathSplit.Count == 0 ? null : pathSplit.Dequeue().ToLower();
				if (pathSplit.Count == 0) {
					return this;
				}
				else {
					if (elements.Contains(action)) {
						ControllerTreeElement treeElement = (ControllerTreeElement)elements[action];
						return treeElement.SearchRoot(ref pathSplit);
					}
					else {
						return null;
					}
				}
			}
		}
    }
}