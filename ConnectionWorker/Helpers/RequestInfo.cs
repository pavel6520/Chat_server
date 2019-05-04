using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Security.Policy;
using WebSocketSharp.Net;
using WebSocketSharp.Net.WebSockets;

namespace ConnectionWorker.Helpers {
    [Serializable]
    public class RequestInfo {
        public string[] AcceptTypes;
        public Encoding ContentEncoding;
        public long ContentLength64;
        public string ContentType;
        public CookieCollection Cookies;
        public bool HasEntityBody;
        public NameValueCollection Headers;
        public string HttpMethod;
        public bool isLocal;
        public bool IsSecureConnection;
        public bool KeepAlive;
        public System.Net.IPEndPoint RemoteEndPoint;
        public string ServiceName;
        public Uri Url;
        public Uri UrlReferrer;
        public string UserAgent;
        public string UserHostAddress;
        public string UserHostName;
        public string[] UserLanguages;
		public byte[] Content;

		public RequestInfo(HttpListenerRequest request) {
			AcceptTypes = request.AcceptTypes;
			ContentEncoding = request.ContentEncoding;
			ContentLength64 = request.ContentLength64;
			ContentType = request.ContentType;
			Cookies = request.Cookies;
			HasEntityBody = request.HasEntityBody;
			Headers = request.Headers;
			HttpMethod = request.HttpMethod;
			isLocal = request.IsLocal;
			IsSecureConnection = request.IsSecureConnection;
			KeepAlive = request.KeepAlive;
			RemoteEndPoint = request.RemoteEndPoint;
			//ServiceName = request.ServiceName;
			Url = request.Url;
			UrlReferrer = request.UrlReferrer;
			UserAgent = request.UserAgent;
			UserHostAddress = request.UserHostAddress;
			UserHostName = request.UserHostName;
			UserLanguages = request.UserLanguages;
			if (ContentLength64 > 0) {
				Content = new byte[ContentLength64];
				request.InputStream.Read(Content, 0, Content.Length);
				Console.WriteLine(Encoding.UTF8.GetString(Content));
			}
		}
	}
}
