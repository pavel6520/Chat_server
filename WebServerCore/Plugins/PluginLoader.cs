using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Plugins {
	class PluginLoader {
		public static DirectoryInfo baseDirectory;

		public Plugin plugin;
		public readonly string[] Module;
		public readonly string AbsPath;
		public readonly string Name;

		public bool isLoad { get { return plugin == null ? false : plugin.isLoad; } }

		public PluginLoader(ref string[] module, string path, string name) {
			Module = module;
			AbsPath = path;
			Name = name;
		}

		private void Compare() {
			DirectoryInfo baseD = new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}{AbsPath}{Path.DirectorySeparatorChar}");
			DirectoryInfo workD = new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}Work{AbsPath}{Path.DirectorySeparatorChar}");
			if (!workD.Exists) {
				workD.Create();
			}
			List<string> filesWork = new List<string>();
			FileInfo[] arrayTmp = workD.GetFiles();
			for (int i = 0; i < arrayTmp.Length; i++) {
				filesWork.Add(arrayTmp[i].Name);
			}
			arrayTmp = baseD.GetFiles();
			for (int i = 0; i < arrayTmp.Length; i++) {
				FileInfo item = arrayTmp[i];
				string workF = $"{workD.FullName}{item.Name}";
				if (filesWork.Contains(item.Name)) {
					filesWork.Remove(item.Name);
					if (File.GetLastWriteTimeUtc(workF) < item.LastWriteTimeUtc) {
						item.CopyTo(workF, true);
					}
				}
				else {
					while (true) {
						try {
							item.CopyTo(workF, true);
							break;
						}
						catch {
							System.Threading.Thread.Sleep(50);
						}
					}
				}
			}
			for (int i = 0; i < filesWork.Count; i++) {
				File.Delete($"{workD.FullName}{filesWork[i]}");
			}
		}

		public void Load() {
			if (plugin == null) {
				plugin = new Plugin($"{Name}{Module[0]}");
			}
			Unload();
			Compare();
			var workD = new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}Work{AbsPath}{Path.DirectorySeparatorChar}");
			plugin.LoadPlugin(ref workD);
		}

		public void Unload() {
			if(plugin != null) {
				plugin.UnloadPlugin();
			}
		}

		public void RemoveWorkFiles() {
			Unload();
			FileInfo[] tmp = new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}Work{AbsPath}{Path.DirectorySeparatorChar}").GetFiles();
			for (int i = 0; i < tmp.Length; i++) {
				tmp[i].Delete();
			}
		}

		public void RemoveWorkDir() {
			RemoveWorkFiles();
			new DirectoryInfo($"{baseDirectory.FullName}{Module[1]}Work{AbsPath}{Path.DirectorySeparatorChar}").Delete(true);
		}
	}
}
