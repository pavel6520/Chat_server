using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Connection {
    class ConnectionFormatException : Exception {
        public ConnectionFormatException(string message) : base(message) { }
        public ConnectionFormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
