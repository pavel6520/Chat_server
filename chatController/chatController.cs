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
				_helper.Redirect("/auth/login");
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	public void indexAction() {
		Echo("<div id=\"blockChat\">");
		Echo("<div id=\"ChatBlockContacts\"><div>");
		Echo("<div class=\"ChatBTN\"><a href=\"/auth/logout\" class=\"refbutton\">EXIT</a></div>");
		Echo("<div class=\"ChatBTN\" data=\"public\">Public</div>");
		Echo("<div id=\"ChatList2\">");
		Echo("<ul id=\"ChatList3\"></ul>");
		Echo("</div> </div> </div>");
		Echo("<div id=\"ChatBody1\"><div><div id=\"ChatBodyList1\"></div><div id=\"ChatBodyInput\"><div><div id=\"ChatTextArea\">");
		Echo("</div></div></div></div></div>");
		Echo("</div>");
	}
}
