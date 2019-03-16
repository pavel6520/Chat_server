using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Connection {
    class ConnectionCloseException : Exception {
        public ConnectionCloseException(string message) : base(message) { }
        public ConnectionCloseException(string message, Exception innerException) : base(message, innerException) { }
    }
}
