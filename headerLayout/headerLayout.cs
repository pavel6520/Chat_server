using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class headerLayout : LayoutWorker {
	public headerLayout() {
		staticInclude = new string[] { "auth" };
	}

	public void Init() {
		if (!_helper.isAuth) {
			PluginWorker auth = _helper.GetPlugin("auth");
			auth._Work(_helper, "checkSession");
			_helper = auth._GetHelper();
		}
		Echo("<header class=\"navbar navbar-fixed-top navbar-expand-md navbar-light bg-light\" style=\"z-index: 100;\">" +
			"<a class=\"navbar-brand\" href=\"/\">Chat by pavel6520</a>" +
   "<button class=\"navbar-toggler\" type=\"button\" data-toggle=\"collapse\" data-target=\"#navbarContent\" aria-controls=\"navbarSupportedContent\" aria-expanded=\"false\" aria-label=\"Toggle navigation\">" +
   "<span class=\"navbar-toggler-icon\"></span>" +
   "</button>" +
   "<div class=\"collapse navbar-collapse\" id=\"navbarContent\">" +
   "<ul class=\"navbar-nav mr-auto\">" +
   "<li class=\"nav-item dropdown\">" +
   "<a class=\"nav-link dropdown-toggle\" href=\"#\" id=\"navbarDropdown\" role=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\"" +
   $">{(_helper.isAuth && _helper.Auth.Status ? _helper.Auth.Login : "Account")}</a>" +
   "<div class=\"dropdown-menu\" aria-labelledby=\"navbarDropdown\">");
		if (_helper.isAuth && _helper.Auth.Status) {
			Echo("<a class=\"dropdown-item\" href=\"#\">Action</a>" +
			"<a class=\"dropdown-item\" href=\"#\">Another action</a>" +
			"<div class=\"dropdown-divider\"></div>" +
			"<a class=\"dropdown-item\" href=\"#\">Something else here</a>");
		}
		else {
			Echo("<a class=\"dropdown-item\" href=\"/auth/login\">Log-in</a>" +
			"<a class=\"dropdown-item\" href=\"/auth/signin\">Sign-in</a>");
		}
   Echo("</div>" +
   "</li>" +
   "<li class=\"nav-item\">" +
   "<a class=\"nav-link disabled\" href=\"#\">Disabled</a>" +
   "</li>" +
   "</ul>" +
   "<form class=\"form-inline my-lg-0\">" +
   "<input class=\"form-control mr-sm-2\" type=\"search\" placeholder=\"Search\" aria-label=\"Search\">" +
   "<button class=\"btn btn-outline-success my-2 my-sm-0\" type=\"submit\">Search</button>" +
   "</form>" +
   "</div>" +
   "</header>");
	}
}
