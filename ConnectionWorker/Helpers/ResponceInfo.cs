﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class ResponceInfo {
		public WebHeaderCollection Headers;
		public int StatusCode;
		public string StatusDescription;
		public string ContentType;

		public ResponceInfo() {
			Headers = new WebHeaderCollection();
			StatusCode = 200;
			StatusDescription = "OK";
			ContentType = "text/html; charset=UTF-8";
		}
	}
}
