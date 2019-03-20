using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebServerCore.Connection;
using WebServerCore.Connection.Http;

namespace ConnectionWorker {
    public abstract class ConnectionController_class : MarshalByRefObject {
        protected string content;

        public ConnectionController_class() { }

        public string[] GetActionList() {
            List<string> names = new List<string>();
            foreach (var item in this.GetType().GetMethods()) {
                if (item.Name.Length > 6 && item.Name.Substring(item.Name.Length - 6, 6) == "Action")
                    names.Add(item.Name);
            }
            return names.ToArray();
        }

        public string Render(string action) {
            var method = this.GetType().GetMethod(action);
            method.Invoke(this, null);
            return content;
        }
    }
}