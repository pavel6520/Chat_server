using ConnectionWorker;
using ConnectionWorker.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class authController : ControllerWorker {
	PluginWorker auth;
	public authController() {
		staticInclude = new string[] { "auth" };
	}

	public void Init() {
		if (_helper.isSecureConnection) {
			auth = (PluginWorker)_helper.staticPlugins["auth"];
			bool res = (bool)auth._Work(_helper, "checkSession");
			_helper = auth._GetHelper();
			if (_helper.Request.HttpMethod == "GET" && _helper.Request.Url.AbsolutePath != "/auth/logout" && res) {
				_helper.AnswerRedirect("/chat");
			}
		}
		else {
			_helper.AnswerRedirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		}
	}

	//public void indexAction() {
	//	if (_helper.Render.isEnabled) {
	//	}
	//}

	public void loginAction() {
		if (_helper.returnType == ReturnType.DefaultContent) {
			if (_helper.Request.HttpMethod == "POST") {
				_helper.Render.DissableRender();
				var res = (bool)auth._Work(_helper, "loginUser");
				_helper = auth._GetHelper();
				if (res) {
					EchoJson(new { state = res, redirect = $"https://{_helper.domainName}/chat" });
				}
				else {
					EchoJson(new { state = res });
				}
			}
			else {
				Echo("<link href=\"/assets/css/styleform.css\" rel=\"stylesheet\">");
				Echo("<div id=\"containerauth\">");
				Echo("<div id=\"wrapper\">");
				Echo("<div id=\"authform\">");
				Echo($"<form name=\"login\" action=\"https://{_helper.domainName}/auth/login\" method=\"POST\">");
				Echo("<h1>Log in</h1>");
				//Echo("<p>");
				Echo("<label for=\"email\" class=\"uname\" data-icon=\"u\"> Your username </label>");
				Echo("<input type=\"text\" id=\"login\" name=\"email\" />");
				//Echo("</p>");
				//Echo("<p>");
				Echo("<label for=\"pass\" class=\"youpasswd\" data-icon=\"p\"> Your password </label>");
				Echo("<input type=\"password\" id=\"password\" name=\"pass\" />");
				//Echo("</p>");
				//Echo("<p class=\"keeplogin\"> ");
				//Echo("<input type=\"checkbox\" name=\"loginkeeping\" id=\"loginkeeping\" value=\"loginkeeping\" />");
				//Echo("<label for=\"loginkeeping\">Keep me logged in</label>");
				//Echo("</p>");
				Echo("</form>");
				Echo("<button class=\"btn btn-primary btn-lg\">Login</button>");
				Echo("<p class=\"change_link\">Not a member yet ?<a href=\"/auth/signin\" class=\"to_subscribe\">Join us</a></p>");
				formJS();
				Echo("</div></div></div>");
			}
		}
	}

	public void logoutAction() {
		if (_helper.isSecureConnection) {
			auth._Work(_helper, "delSession");
			_helper = auth._GetHelper();
			_helper.AnswerRedirect("/");
		}
	}

	public void signinAction() {
		if (_helper.returnType == ReturnType.DefaultContent) {
			if (_helper.Request.HttpMethod == "POST") {
				_helper.Render.DissableRender();
				var res = (bool)auth._Work(_helper, "addUser");
				_helper = auth._GetHelper();
				if (res) {
					EchoJson(new { state = res, redirect = $"https://{_helper.domainName}/chat" });
				}
				else {
					EchoJson(new { state = res });
				}
			}
			else {
				Echo("<link href=\"/assets/css/styleform.css\" rel=\"stylesheet\">");
				Echo("<div id=\"containerauth\">");
				Echo("<div id=\"wrapper\">");
				Echo("<div id=\"authform\">");
				Echo($"<form  action=\"https://{_helper.domainName}/auth/signin\" method=\"post\">");
				Echo("<h1> Sign up </h1>");
				Echo("<p>");
				Echo("<label for=\"login\" class=\"uname\" data-icon=\"u\">Your login </label>");
				Echo("<input id=\"login\" name=\"login\" type=\"text\"/>");
				Echo("</p>");
				//Echo("<p>");
				//Echo("<label for=\"emailsignup\" class=\"youmail\" data-icon=\"e\" > Your email</label>");
				//Echo("<input id=\"emailsignup\" name=\"emailsignup\" required=\"required\" type=\"text\" placeholder=\"mysupermail@mail.com\"/>");
				//Echo("</p>");
				Echo("<p>");
				Echo("<label for=\"password\" class=\"youpasswd\" data-icon=\"p\">Your password </label>");
				Echo("<input id=\"password\" name=\"password\" type=\"password\" />");
				Echo("</p>");
				Echo("<p>");
				Echo("<label for=\"password_confirm\" class=\"youpasswd\" data-icon=\"p\">Please confirm your password </label>");
				Echo("<input id=\"password_confirm\" name=\"password_confirm\" type=\"password\"/>");
				Echo("</p>");
				Echo("</form>");
				Echo("<button class=\"btn btn-primary btn-lg\">Sign up</button>");
				Echo("<p class=\"change_link\">Already a member ?<a href=\"/auth/login\" class=\"to_subscribe\">Go and log in </a></p>");
				formJS();
				Echo("</div></div></div>");
			}
		}
	}

	private void formJS() {
		Echo("<script>$('#authform > button.btn').on('click', function () {/*let formData = new FormData($('form')[0]);*/$.ajax({type: 'POST',url: $('form').attr('action'),processData: true,data: {login: $('#login').val(),password: $('#password').val(),password_confirm: $('#password_confirm').val()},/*data: formData,*/success: function (data) {console.log(data);if(data.state){$('form').attr('action', '/chat').submit();/*window.location.href = data.redirect;*/}else{/*отобразить ошибки*/}}});});</script>");
	}
}
