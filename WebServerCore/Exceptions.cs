using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebServerCore {
    class PathNotFoundException : Exception {
        public PathNotFoundException(string path) : base(path) { }
    }
}
