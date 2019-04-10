using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker {
	public class ControllerWorker : PluginWorker {
		public HttpListenerContext context;

		new public void _Work(string action, object[] args = null) {
			base._Work($"{action}Action", args);
		}
		
		public string[] _GetActionList() {
			List<string> names = new List<string>();
			foreach (var item in this.GetType().GetMethods()) {
				if (item.Name.Length > 6 && item.Name.Substring(item.Name.Length - 6, 6) == "Action")
					names.Add(item.Name.Substring(0, item.Name.Length - 6));
			}
			return names.ToArray();
		}

		//unsafe public void Test(/*IntPtr contextRef*/void* cRef) {
		//	//string s = System.Runtime.CLR.EntityPtr.ToInstance<string>(contextRef);
		//	char c = *(char*)cRef;
		//	Console.WriteLine(c);

		//	//HttpListenerContext context1 = System.Runtime.CLR.EntityPtr.ToInstance<HttpListenerContext>(contextRef);
		//}
	}
}
