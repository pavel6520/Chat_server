using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Client {
    public class ConnectionWrite : Connection {
        internal ConnectionWrite(Connection cc) : base(cc) {
        }

        private new void WriteByte(byte b) {
            base.WriteByte(b);
        }

        private new void Write(byte[] b) {
            base.Write(b);
        }

        private new void WriteLine(string s) {
            base.WriteLine(s);
        }
    }
}
