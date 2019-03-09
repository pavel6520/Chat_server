﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Client.Http {
    public sealed class HttpContext {
        private readonly Connection cc;

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }

        internal HttpContext(Connection cc) {
            this.cc = cc;
            Request = new HttpRequest(cc);
            Response = new HttpResponse(cc);
        }
    }
}
