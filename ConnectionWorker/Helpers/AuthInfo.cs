using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class AuthInfo {
		public string Login;
		public DateTime TimeCreate;
		public bool Status { get; }

		public AuthInfo() {
			Status = false;
		}

		public AuthInfo(string login) {
			Login = login;
			Status = true;
		}
	}
}
