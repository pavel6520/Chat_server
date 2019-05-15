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
			if (!_helper.isAuth) {
				PluginWorker auth = _helper.GetPlugin("auth");
				auth._Work(_helper, "checkSession");
				_helper = auth._GetHelper();
			}
			if (!_helper.isAuth || !_helper.Auth.Status) {
				_helper.AnswerRedirect("/auth/login");
			}
		}
		else {
			_helper.AnswerRedirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	public void indexAction() {
		Echo("<div id=\"chatcontent\" class=\"container-fluid row align-items-end p-0 m-0\" style=\"flex-grow:1;\">");
		Echo("</div>");
		Echo("<script src=\"/client/chat.js\"></script>");
	}
}
