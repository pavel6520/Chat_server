using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class headerLayout : LayoutWorker {

	public void Init() {
		Echo("<header class=\"navbar navbar-light bg-light navbar-fixed-top\">");
		var url = _helper.Request.Url.LocalPath.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
		if (url.Length > 0 && url[0] == "chat") {
			Echo("<style>@media(min-width: 992px){body {padding: 0 0 0 300px;}header.navbar>button{display:none;}}</style>");
			Echo("<button class=\"navbar-toggler\" type=\"button\" data-toggle=\"offcanvas\" data-target=\".navmenu\"><span class=\"navbar-toggler-icon\"></span></button>");
		}
		Echo("<a class=\"navbar-brand mr-auto\" href=\"/\">Pavel6520 Chat</a>" +
   "<div class=\"nav-item\">" +
   "<a class=\"nav-link disabled\" href=\"#\">Disabled</a>" +
   "</div>" +
   "<div class=\"nav-item dropleft\">" +
   "<a class=\"nav-link dropdown-toggle\" href=\"#\" id=\"navbarDropdown\" role=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">" + (_helper.isAuth && _helper.Auth.Status ? _helper.Auth.Login : "Account") +
   "</a>" +
   "<div class=\"dropdown-menu\" aria-labelledby=\"navbarDropdown\">");
		if (_helper.isAuth && _helper.Auth.Status) {
			Echo("<a class=\"dropdown-item\" href=\"#\">Test1</a>" +
   "<a class=\"dropdown-item\" href=\"#\">Test2</a>" +
   "<div class=\"dropdown-divider\"></div>" +
   "<a class=\"dropdown-item\" href=\"/auth/logout\">Log-Out</a>");
		}
		else {
			Echo("<a class=\"dropdown-item\" href=\"/auth/login\">Log-in</a>" +
			"<a class=\"dropdown-item\" href=\"/auth/signin\">Sign-in</a>");
		}
		Echo("</div></div></header>");
	}
}
