using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Client.Http {
    public sealed class HttpResponse {
        private readonly Connection cc;
        private bool closed;
        
        public string ProtocolVersion { get; set; }
        public int StatusCode { get; set; }
        public string StatusDescription { get; set; }

        public Encoding ContentEncoding { get; set; }
        public long? ContentLength64;
        public string ContentType;
        public System.Net.CookieCollection Cookies { get; set; }
        public WebHeaderCollection Headers { get; set; }
        public string RedirectLocation { get; set; }

        internal HttpResponse(Connection cc) {
            this.cc = cc;
            Cookies = new CookieCollection();
            Headers = new WebHeaderCollection();
            ProtocolVersion = "1.1";
            StatusCode = 200;
            closed = false;
        }

        public void AddHeader(string name, string value) {
            Headers.Add(name, value);
        }

        //public void AppendHeader(string name, string value) {
        //}

        public void AppendCookie(Cookie cookie) {
            Cookies.Add(cookie);
        }

        //public void SetCookie(string name, string value) {
        //    Cookies.
        //}

        public ConnectionWrite GetWriteStream() {
            if (!closed)
                Close();
            return new ConnectionWrite(cc);
        }

        public void Close() {
            if (closed)
                throw new Exception(); //TODO
            cc.WriteLine($"HTTP/{ProtocolVersion} {StatusCode}{(StatusDescription == null ? "": $" {StatusDescription}")}");
            if (ContentEncoding != null)
                cc.WriteLine($"Content-Encoding: {ContentEncoding.WebName}");
            if (ContentLength64 != null)
                cc.WriteLine($"Content-Length: {ContentLength64}");
            if (ContentType != null)
                cc.WriteLine($"Content-Type: {ContentType}");
            if (RedirectLocation != null)
                cc.WriteLine($"Location: {RedirectLocation}");
            for (int i = 0; i < Headers.Count; i++)
                cc.WriteLine($"{Headers.GetKey(i)}: {Headers.GetValues(i)}");
            for (int i = 0; i < Cookies.Count; i++)
                cc.WriteLine($"Set-Cookie: {Cookies[i].Name}={Cookies[i].Value}{(Cookies[i].Path == null ? "" : $"; path={Cookies[i].Path};")}{(Cookies[i].Expires == null ? "" : $"; path={Cookies[i].Expires.ToString("R")};")}");
            closed = true;
            cc.WriteLine("");
        }
    }
}
