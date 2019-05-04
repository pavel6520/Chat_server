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
		//if (_helper.isSecureConnection) {
			PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
			auth._SetHelper(_helper);
			if (_helper.Request.HttpMethod == "GET" && _helper.Request.Url.AbsolutePath != "/auth/logout" && (bool)auth._Work("checkSession")) {
				_helper = auth._GetHelper();
				_helper.AnswerRedirect("/chat");
			}
		//}
		//else {
		//	_helper.AnswerRedirect($"https://{_helper.domainName}{_helper.Request.Url.PathAndQuery}");
		//}
	}

	//public void indexAction() {
	//	if (_helper.Render.isEnabled) {
	//	}
	//}

	public void loginAction() {
		if (_helper.returnType == ReturnType.Content) {
			if (_helper.Request.HttpMethod == "POST") {
				_helper.Render.DissableRender();
				PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
				auth._SetHelper(_helper);
				var res = (bool)auth._Work("loginUser");
				_helper = auth._GetHelper();
				_helper.Answer(200, "OK");
				object tmp;
				if (res) {
					tmp = new { state = res, redirect = $"https://{_helper.domainName}/chat" };
				}
				else {
					tmp = new { state = res };
				}
				_helper.Responce.ContentType = "application/json; charset=UTF-8";
				Echo(Newtonsoft.Json.JsonConvert.SerializeObject(tmp));
			}
			else {
				Echo("<link href=\"/assets/css/styleform.css\" rel=\"stylesheet\">");
				//Echo("<link href=\"/assets/css/animateform.css\" rel=\"stylesheet\">");
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
				Echo("<p class=\"login button\"><button>Login</button></p>");
				Echo("<p class=\"change_link\">Not a member yet ?<a href=\"/auth/signin\" class=\"to_subscribe\">Join us</a></p>");
				formJS();
				Echo("</div></div></div>");
			}
		}
	}

	public void logoutAction() {
		if (_helper.isSecureConnection) {
			PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
			auth._SetHelper(_helper);
			auth._Work("delSession");
			_helper = auth._GetHelper();
			_helper.AnswerRedirect("/");
		}
	}

	public void signinAction() {
		if (_helper.returnType == ReturnType.Content) {
			if (_helper.Request.HttpMethod == "POST") {
				PluginWorker auth = (PluginWorker)_helper.staticPlugins["auth"];
				auth._SetHelper(_helper);
				if ((bool)auth._Work("addUser")) {
					_helper = auth._GetHelper();
					_helper.AnswerRedirect("/chat");
				}
				else {
					_helper.AnswerRedirect("/auth/signin");
				}
			}
			else {
				Echo("<link href=\"/assets/css/styleform.css\" rel=\"stylesheet\">");
				//Echo("<link href=\"/assets/css/animateform.css\" rel=\"stylesheet\">");
				Echo("<div id=\"containerauth\">");
				Echo("<div id=\"wrapper\">");
				Echo("<div id=\"authform\">");
				Echo("<form  action=\"https://{_helper.domainName}/auth/signin\" method=\"post\">");
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
				Echo("</form>");
				Echo("<p class=\"signin button\"><input type=\"submit\" name=\"submit\" value=\"Sign up\"/>");
				Echo("</p><p class=\"change_link\">Already a member ?<a href=\"/auth/login\" class=\"to_subscribe\">Go and log in </a></p>");
				formJS();
				Echo("</div></div></div>");
			}
		}
	}

	private void formJS() {
		Echo("<script>$('p.login.button > button').on('click', function () {/*let formData = new FormData($('form')[0]);*/$.ajax({type: 'POST',url: $('form').attr('action'),processData: true,data: {login: $('#login').val(),password: $('#password').val(),password_confirm: $('#password_confirm').val()},/*data: formData,*/success: function (data) {console.log(data);if(data.state){$('form').attr('action', '/chat').submit();/*window.location.href = data.redirect;*/}else{/*отобразить ошибки*/}}});});</script>");
	}
}
