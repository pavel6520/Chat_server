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
		Echo("<div id=\"content\" class=\"container-fluid\" style=\"flex-grow:1;\">");
		Echo("<div class=\"navmenu navmenu-default navmenu-fixed-left offcanvas-sm\" style=\"\">" +
			"<div class=\"flex\">" +
   "<div class=\"btn-group marginchildoff\" role=\"group\">" +
   "<button type=\"button\" class=\"btn btn-primary col-3\">Dialog</button>" +
   "<button type=\"button\" class=\"btn btn-primary col-1 paddingoff\">+</button>" +
   "<button type=\"button\" class=\"btn btn-secondary col-3\">Rooms</button>" +
   "<button type=\"button\" class=\"btn btn-secondary col-1 paddingoff\">+</button>" +
   "<button type=\"button\" class=\"btn btn-dark col-4\">Public</button>" +
   "</div>" +
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
   "</ul></div></div>");
		Echo("");

		Echo("</div>");
	}
}
