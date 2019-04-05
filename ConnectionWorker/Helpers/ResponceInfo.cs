using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class ResponceInfo {
		public WebHeaderCollection Headers;

		public ResponceInfo() {
			Headers = new WebHeaderCollection();
		}
	}
}
