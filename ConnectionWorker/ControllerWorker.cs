using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker {
	public class ControllerWorker : PluginWorker {
		new public void _Work(string action) {
			base._Work($"{action}Action");
		}
		
		public string[] _GetActionList() {
			List<string> names = new List<string>();
			foreach (var item in this.GetType().GetMethods()) {
				if (item.Name.Length > 6 && item.Name.Substring(item.Name.Length - 6, 6) == "Action")
					names.Add(item.Name.Substring(0, item.Name.Length - 6));
			}
			return names.ToArray();
		}
	}
}
