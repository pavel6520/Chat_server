using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Connection.Http {
    public sealed class HttpContext {
        private readonly ConnectionClass cc;

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public bool IsCrypt { get { return cc.Crypt; } }

        public HttpContext(ConnectionClass cc, ref string Domain) {
            this.cc = cc;
            Request = new HttpRequest(cc, ref Domain);
            Response = new HttpResponse(cc);
        }

        public ConnectionClass GetConnection() {
            return cc;
        }
    }
}
