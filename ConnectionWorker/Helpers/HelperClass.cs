﻿using System;
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
		public bool isAuth;

		public bool isSecureConnection { get { return Request.IsSecureConnection; } }
		public bool isWebSocket { get { return ContextWs != null; } }
		public string RedirectLocation { get; private set; }

		public ReturnType returnType;//{ get; private set; }
		public Hashtable staticPlugins;

		public HelperClass(ref HttpListenerContext context, string db, string domain) {
			Render = new RenderClass();
			dbConnectString = db;
			domainName = domain;
			isAuth = false;
			RedirectLocation = null;
			staticPlugins = null;
			Request = new RequestInfo(context.Request);
			Responce = new ResponceInfo();
			returnType = ReturnType.Content;
			Context = context;
		}

		public HelperClass(ref HttpListenerContext context, string db, string domain, HttpListenerWebSocketContext contextWs) 
			: this(ref context, db, domain) {
			ContextWs = contextWs;
		}

		public void AnswerRedirect(string url) {
			returnType = ReturnType.Info;
			Render.DissableRender();
			RedirectLocation = url;
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

		public void GetData(HelperClass helper) {
			Render = helper.Render;
			Auth = helper.Auth;
			Request = helper.Request;
			Responce = helper.Responce;
			isAuth = helper.isAuth;
			RedirectLocation = helper.RedirectLocation;
			staticPlugins = helper.staticPlugins;
			returnType = helper.returnType;
		}
	}

	public enum ReturnType {
		Content,
		Info,
		Special
	}
}