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
			"<ul class=\"nav navmenu-nav flex-column\">" +
   "<li class=\"nav-item\">" +
   "<div class=\"container-fluid\" style=\"padding: 0;\">" +
   "<div class=\"row btn-group col-12\" role=\"group\" aria-label=\"Chat channels\" style=\"padding:0;margin: 0;\">" +
   "<div class=\"col-4\" style=\"padding: 0;\"><button type=\"button\" class=\"btn btn-primary col-sm\">Left</button></div>" +
   "<div class=\"col-4\" style=\"padding: 0;\"><button type=\"button\" class=\"btn btn-primary col-sm\">Middle</button></div>" +
   "<div class=\"col-4\" style=\"padding: 0;\"><button type=\"button\" class=\"btn btn-primary col-sm\">Right</button></div></div></div></li>" +
   "<li class=\"nav-item\"><a class=\"nav-link\" href=\"../navmenu-push/\">Push</a>" +
   "</li>" +
   "</ul>" +
   "<ul class=\"nav navmenu-nav flex-column\">" +
   "<li class=\"nav-item\"><a class=\"nav-link\" href=\"#\">Link</a></li>" +
   "<li class=\"nav-item dropdown\"><a href=\"#\" class=\"nav-link dropdown-toggle\" data-toggle=\"dropdown\">Dropdown <b class=\"caret\"></b></a>" +
   "<ul class=\"dropdown-menu navmenu-nav\"><li class=\"nav-item\">" +
   "<a class=\"nav-link\" href=\"#\">Action</a></li><li class=\"nav-item\">" +
   "<a class=\"nav-link\" href=\"#\">Another action</a></li><li class=\"nav-item\">" +
   "<a class=\"nav-link\" href=\"#\">Something else here</a></li>" +
   "<li class=\"dropdown-divider\"></li>" +
   "<li class=\"dropdown-header\">Nav header</li>" +
   "<li class=\"nav-item\"><a class=\"nav-link\" href=\"#\">Separated link</a></li>" +
   "<li class=\"nav-item\"><a class=\"nav-link\" href=\"#\">One more separated link</a></li>" +
   "</ul>" +
   "</li>" +
   "</ul>" +
   "</div>");
	}
}
