using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Net;
using WebSocketSharp.Net.WebSockets;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class HelperClass {
		[NonSerialized]
		public HttpListenerContext Context;
		[NonSerialized]
		public WebSocketContext ContextWs;
		public RenderClass Render;
		public AuthInfo Auth;
		public RequestInfo Request;
		public ResponceInfo Responce;
		public string dbConnectString;
		public string domainName;
		public bool isDebug;
		public WebSocketHelper WShelper;

		public bool isAuth { get { return Auth != null; } }
		public bool isSecureConnection { get { return Request.IsSecureConnection; } }
		public bool isWebSocket { get { return ContextWs != null; } }

		public ReturnType returnType;//{ get; private set; }
		public Hashtable staticPlugins;

		public HelperClass(ref HttpListenerContext context, string db, string domain, bool isDebug) {
			Render = new RenderClass();
			dbConnectString = db;
			domainName = domain;
			staticPlugins = null;
			Request = new RequestInfo(context.Request);
			Responce = new ResponceInfo();
			returnType = ReturnType.DefaultContent;
			Context = context;
			this.isDebug = isDebug;
			WShelper = new WebSocketHelper();
		}

		public HelperClass(ref HttpListenerContext context, string db, string domain, bool isDebug, HttpListenerWebSocketContext contextWs)
			: this(ref context, db, domain, isDebug) {
			ContextWs = contextWs;
		}

		public JObject GetJsonContent() {
			//string tmp = Encoding.UTF8.GetString(Request.Content);
			return JObject.Parse(Encoding.UTF8.GetString(Request.Content));
		}

		public void AnswerRedirect(string url) {
			returnType = ReturnType.Info;
			Responce.StatusCode = 302;
			Responce.StatusDescription = "Moved Temporarily";
			Responce.RedirectLocation = url;
		}
		public void Answer(int code, string description) {
			Responce.StatusCode = code;
			Responce.StatusDescription = description;
		}

		public void AnswerInfo(int code, string description) {
			returnType = ReturnType.Info;
			Responce.StatusCode = code;
			Responce.StatusDescription = description;
		}

		public void Answer500(Exception e) {
			Responce.StatusCode = 500;
			Responce.StatusDescription = "Internal Server Error";
			//if (Auth != null) {
			//}
		}

		public PluginWorker GetPlugin(string name) {
			if (staticPlugins.ContainsKey(name)) {
				return (PluginWorker)staticPlugins["auth"];
			}
			else {
				return null;
			}
		}

		public void GetData(HelperClass helper) {
			Render = helper.Render;
			Auth = helper.Auth;
			Request = helper.Request;
			Responce = helper.Responce;
			staticPlugins = helper.staticPlugins;
			returnType = helper.returnType;
			WShelper = helper.WShelper;
		}
	}

	public enum ReturnType {
		DefaultContent,
		Info
	}
}
