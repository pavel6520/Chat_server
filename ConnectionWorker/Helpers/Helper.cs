using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class HelperClass {
		[NonSerialized]
		public readonly HttpListenerContext Context;
		public RenderClass _render;
		public RequestInfo Request;

		public bool isAuth { get; private set; }
		public bool isSecureConnection { get { return Request.IsSecureConnection; } }
		public string RedirectLocation { get; private set; }

		private ReturnType returnType;

		public HelperClass(ref HttpListenerContext context) {
			Context = context;
			Request = new RequestInfo(context.Request);
			returnType = ReturnType.Content;
		}

		public void Redirect(string url) {
			returnType = ReturnType.Redirect;
			RedirectLocation = url;
		}
	}

	public enum ReturnType {
		Content,
		NotFound404,
		Redirect,
		Error
	}
}
