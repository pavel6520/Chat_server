using System;
using System.Collections.Generic;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class WebSocketHelper {
		public bool Close;
		public DateTime ConnectTime;
		public List<WebSocketAct> Acts;
		public bool ActsForAll;

		public WebSocketHelper() {
			ConnectTime = DateTime.Now;
			Acts = new List<WebSocketAct>(0);
			ActsForAll = true;
		}
	}

	[Serializable]
	public class WebSocketAct {
		public List<string> Recepients;
		public string Body;

		public WebSocketAct() {
			Recepients = new List<string>(0);
		}
	}
}