using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore.Connection {
    public sealed class ConnectionRead : Connection {
        internal ConnectionRead(Connection cc) : base(cc) {
        }

        private new void ReadByte() {
            base.ReadByte();
        }

        private new void Read(int count = 10000) {
            base.Read(count);
        }
        
        private new void ReadLine() {
            base.ReadLine();
        }
    }
}
