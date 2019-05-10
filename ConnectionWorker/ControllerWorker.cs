using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker {
	public class ControllerWorker : PluginWorker {
		new public void _Work(Helpers.HelperClass helper, string action, object[] args = null) {
			base._Work(helper, $"{action}Action", args);
		}

		new public void _WorkWS(Helpers.HelperClass helper, string action, object[] args = null) {
			base._WorkWS(helper, $"{action}ActionWS", args);
		}

		public string[] _GetActions() {
			List<string> names = new List<string>();
			foreach (var item in this.GetType().GetMethods()) {
				if (item.Name.Length > 6 && item.Name.Substring(item.Name.Length - 6, 6) == "Action")
					names.Add(item.Name.Substring(0, item.Name.Length - 6));
			}
			return names.ToArray();
		}

		public string[] _GetActionsWS() {
			List<string> names = new List<string>();
			foreach (var item in this.GetType().GetMethods()) {
				if (item.Name.Length > 6 && item.Name.Substring(item.Name.Length - 6, 6) == "ActionWS")
					names.Add(item.Name.Substring(0, item.Name.Length - 6));
			}
			return names.ToArray();
		}
	}
}
