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
			auth._SetHelper(ref _helper);
			if ((bool)auth._Work("checkSession")) {
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

	public void indexAction() {
		if (_helper.Render.isEnabled) {
			Echo("<span>authController - loginAction WORKED!</span>");
			Echo("<div id=\"containerauth\">");
			Echo("<a class=\"hiddenanchor\" id=\"tosubscribe\"></a>");
			Echo("<a class=\"hiddenanchor\" id=\"tologin\"></a>");
			Echo("<div id=\"wrapper\">");
			Echo("<div id=\"login\" class=\"animate form\">");
			Echo("<form action=\"auth/login\" autocomplete=\"on\" method=\"POST\">");
			Echo("<h1>Log in</h1>");
			Echo("<p>");
			Echo("<label for=\"login\" class=\"uname\" data-icon=\"u\"> Your username </label>");
			Echo("<input id=\"login\" name=\"login\" required=\"required\" type=\"text\" placeholder=\"myusername\" />");
			Echo("</p>");
			Echo("<p>");
			Echo("<label for=\"password\" class=\"youpasswd\" data-icon=\"p\"> Your password </label>");
			Echo("<input id=\"password\" name=\"password\" required=\"required\" type=\"password\"/>");
			Echo("</p>");
			//Echo("<p class=\"keeplogin\"> ");
			//Echo("<input type=\"checkbox\" name=\"loginkeeping\" id=\"loginkeeping\" value=\"loginkeeping\" />");
			//Echo("<label for=\"loginkeeping\">Keep me logged in</label>");
			//Echo("</p>");
			Echo("<p class=\"login button\">");
			Echo("<input type=\"submit\" value=\"Login\" />");
			Echo("</p>");
			Echo("<p class=\"change_link\">");
			Echo("Not a member yet ?");
			Echo("<a href=\"#tosubscribe\" class=\"to_subscribe\">Join us</a>");
			Echo("</p>");
			Echo("</form>");
			Echo("</div>");

			Echo("<div id=\"reg\" class=\"animate form\">");
			Echo("<form  action=\"auth/signin\" autocomplete=\"on\" method=\"POST\">");
			Echo("<h1> Sign up </h1>");
			Echo("<p>");
			Echo("<label for=\"login\" class=\"uname\" data-icon=\"u\">Your username </label>");
			Echo("<input id=\"login\" name=\"login\" required=\"required\" type=\"text\" placeholder=\"mysuperusername690\" />");
			Echo("</p>");
			//Echo("<p>");
			//Echo("<label for=\"emailsignup\" class=\"youmail\" data-icon=\"e\" > Your email</label>");
			//Echo("<input id=\"emailsignup\" name=\"emailsignup\" required=\"required\" type=\"text\" placeholder=\"mysupermail@mail.com\"/>");
			//Echo("</p>");
			Echo("<p>");
			Echo("<label for=\"password\" class=\"youpasswd\" data-icon=\"p\">Your password </label>");
			Echo("<input id=\"password\" name=\"password\" required=\"required\" type=\"password\"/>");
			Echo("</p>");
			Echo("<p>");
			Echo("<label for=\"password_confirm\" class=\"youpasswd\" data-icon=\"p\">Please confirm your password </label>");
			Echo("<input id=\"password_confirm\" name=\"password_confirm\" required=\"required\" type=\"password\"/>");
			Echo("</p>");
			Echo("<p class=\"signin button\">");
			Echo("<input type=\"submit\" value=\"Sign up\"/>");
			Echo("</p><p class=\"change_link\">Already a member ?<a href=\"#tologin\" class=\"to_subscribe\">Go and log in </a></p>");
			Echo("</form></div></div></div>");
		}
	}

	public void loginAction() {
		if (_helper.isSecureConnection) {
			if (_helper.Request.HttpMethod == "POST") {
				PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
				auth._SetHelper(ref _helper);
				Console.WriteLine(Encoding.UTF8.GetString(_helper.Request.Content));
				if ((bool)auth._Work("loginUser")) {
					_helper = auth._GetHelper();
					_helper.Redirect("/chat");
				}
				else {
					_helper = auth._GetHelper();
					_helper.Redirect("/auth");
				}
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	public void signinAction() {
		if (_helper.isSecureConnection) {
			if (_helper.Request.HttpMethod == "POST") {
				PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
				auth._SetHelper(ref _helper);
				if ((bool)auth._Work("addUser")) {
					_helper = auth._GetHelper();
					_helper.Redirect("/");
				}
				else {
					_helper.Redirect("/auth");
				}
			}
		}
		else {
			_helper.Redirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
		//Echo("<span>authController - signinAction WORKED!</span>");
    }
}
