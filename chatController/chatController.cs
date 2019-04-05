using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class chatController : ControllerWorker {
	public chatController() {
		staticInclude = new string[] { "auth" };
	}

	public void Init() {
		if (_helper.isSecureConnection) {
			PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
			auth._SetHelper(ref _helper);
			if (!(bool)auth._Work("checkSession")) {
				_helper.Redirect("/auth");
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	public void indexAction() {
		Echo("");
	}
}
