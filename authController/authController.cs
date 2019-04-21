using ConnectionWorker;
using ConnectionWorker.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class authController : ControllerWorker {
	public authController() {
		staticInclude = new string[] { "auth" };
	}

	public void Init() {
		if (_helper.isSecureConnection) {
			PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
			auth._SetHelper(_helper);
			if (_helper.Request.Url.AbsolutePath != "/auth/logout" && (bool)auth._Work("checkSession")) {
				_helper = auth._GetHelper();
				if (_helper.Auth != null) {
					_helper.Redirect("/chat");
				}
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	//public void indexAction() {
	//	if (_helper.Render.isEnabled) {
	//	}
	//}

	public void loginAction() {
		if (_helper.isSecureConnection) {
			if (_helper.Request.HttpMethod == "POST") {
				PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
				auth._SetHelper(_helper);
				if ((bool)auth._Work("loginUser")) {
					_helper = auth._GetHelper();
					_helper.Redirect("/chat");
				}
				else {
					_helper = auth._GetHelper();
					_helper.Redirect("/auth/login");
				}
			}
			else {
				Echo("<link href=\"/assets/css/styleform.css\" rel=\"stylesheet\">");
				Echo("<link href=\"/assets/css/animateform.css\" rel=\"stylesheet\">");
				Echo("<div id=\"containerauth\">");
				Echo("<div id=\"wrapper\">");
				Echo("<div id=\"loginform\" class=\"animate form\">");
				Echo("<form action=\"login\" method=\"POST\">");
				Echo("<h1>Log in</h1>");
				Echo("<p>");
				Echo("<label for=\"login\" class=\"uname\" data-icon=\"u\"> Your username </label>");
				Echo("<input id=\"login\" name=\"username\" required type=\"text\" autocomplete=\"username\"/>");
				Echo("</p>");
				Echo("<p>");
				Echo("<label for=\"password\" class=\"youpasswd\" data-icon=\"p\"> Your password </label>");
				Echo("<input id=\"password\" name=\"password\" required type=\"password\"/>");
				Echo("</p>");
				//Echo("<p class=\"keeplogin\"> ");
				//Echo("<input type=\"checkbox\" name=\"loginkeeping\" id=\"loginkeeping\" value=\"loginkeeping\" />");
				//Echo("<label for=\"loginkeeping\">Keep me logged in</label>");
				//Echo("</p>");
				Echo("<p class=\"login button\"><input type=\"submit\" value=\"Login\"  name=\"submit\" /></p>");
				Echo("<p class=\"change_link\">Not a member yet ?<a href=\"/auth/signin\" class=\"to_subscribe\">Join us</a></p>");
				Echo("</div></div></div>");
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	public void logoutAction() {
		if (_helper.isSecureConnection) {
			PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
			auth._SetHelper(_helper);
			auth._Work("delSession");
			_helper = auth._GetHelper();
			_helper.Redirect("/");
		}
	}

	public void signinAction() {
		if (_helper.isSecureConnection) {
			if (_helper.Request.HttpMethod == "POST") {
				PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
				auth._SetHelper(_helper);
				if ((bool)auth._Work("addUser")) {
					_helper = auth._GetHelper();
					_helper.Redirect("/chat");
				}
				else {
					_helper.Redirect("/auth/signin");
				}
			}
			else {
				Echo("<link href=\"/assets/css/styleform.css\" rel=\"stylesheet\">");
				Echo("<link href=\"/assets/css/animateform.css\" rel=\"stylesheet\">");
				Echo("<div id=\"containerauth\">");
				Echo("<!-- спрятанный якорь, чтобы избежать прыжков http://www.css3create.com/Astuce-Empecher-le-scroll-avec-l-utilisation-de-target#wrap4  -->");
				Echo("<a class=\"hiddenanchor\" id=\"toregister\"></a>");
				Echo("<a class=\"hiddenanchor\" id=\"tologin\"></a>");
				Echo("<div id=\"wrapper\">");

				Echo("<div id=\"reg\" class=\"animate form\">");
				Echo("<form  action=\"signin\" autocomplete=\"on\" method=\"POST\">");
				Echo("<h1> Sign up </h1>");
				Echo("<p>");
				Echo("<label for=\"login\" class=\"uname\" data-icon=\"u\">Your username </label>");
				Echo("<input id=\"login\" name=\"login\" required=\"required\" type=\"text\" placeholder=\"mysuperusername690\" autocomplete=\"username\"/>");
				Echo("</p>");
				//Echo("<p>");
				//Echo("<label for=\"emailsignup\" class=\"youmail\" data-icon=\"e\" > Your email</label>");
				//Echo("<input id=\"emailsignup\" name=\"emailsignup\" required=\"required\" type=\"text\" placeholder=\"mysupermail@mail.com\"/>");
				//Echo("</p>");
				Echo("<p>");
				Echo("<label for=\"password\" class=\"youpasswd\" data-icon=\"p\">Your password </label>");
				Echo("<input id=\"password\" name=\"password\" required=\"required\" type=\"password\" autocomplete=\"new-password\"/>");
				Echo("</p>");
				Echo("<p>");
				Echo("<label for=\"password_confirm\" class=\"youpasswd\" data-icon=\"p\">Please confirm your password </label>");
				Echo("<input id=\"password_confirm\" name=\"password_confirm\" required=\"required\" type=\"password\" autocomplete=\"new-password\"/>");
				Echo("</p>");
				Echo("<p class=\"signin button\">");
				Echo("<input type=\"submit\" value=\"Sign up\"/>");
				Echo("</p><p class=\"change_link\">Already a member ?<a href=\"/auth/login\" class=\"to_subscribe\">Go and log in </a></p>");
				Echo("</form></div></div></div>");
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
		//Echo("<span>authController - signinAction WORKED!</span>");
	}
}
