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
		Echo("<div class=\"navmenu navmenu-default navmenu-fixed-left offcanvas-sm\" style=\"\">" +
			"<div class=\"flex\">" +
   "<ul class=\"nav navmenu-nav\">" +
   "<li class=\"col-4 paddingoff\"><button type=\"button\" class=\"btn btn-primary btn-block\">Left</button></li>" +
   "<li class=\"col-4 paddingoff\"><button type=\"button\" class=\"btn btn-primary btn-block\">Left</button></li>" +
   "<li class=\"col-4 paddingoff\"><button type=\"button\" class=\"btn btn-primary btn-block\">Left</button></li>" +
   "</ul>" +
   "<ul class=\"nav navmenu-nav\" style=\"overflow:auto;\">" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "<li class=\"col-12\"><button type=\"button\" class=\"btn btn-light btn-block\">Contact</button></li>" +
   "</ul>");
	}
}
