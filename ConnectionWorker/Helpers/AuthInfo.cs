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
		public bool isSessionActive { get { return TimeCreate > DateTime.UtcNow.AddDays(-1); } }

		public AuthInfo() { }

		public AuthInfo(string login, DateTime timeCreate) {
			Login = login;
			TimeCreate = timeCreate;
		}
	}
}
