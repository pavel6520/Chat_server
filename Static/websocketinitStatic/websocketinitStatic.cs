using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class websocketinitStatic : PluginWorker {
	public websocketinitStatic() {
		staticInclude = new string[] { "auth" };
	}

	public void Init() {
		var auth = _helper.GetPlugin("auth");
		auth._Work(_helper, "checkSession");
		_helper = auth._GetHelper();
	}
}