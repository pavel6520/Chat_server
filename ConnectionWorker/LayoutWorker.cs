using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker {
	public class LayoutWorker : PluginWorker {
		private bool contentIncluded;

		public LayoutWorker() : base() { contentIncluded = false; }

		public void IncludeContent() {
			if (!contentIncluded) {
				contentIncluded = true;
				contentType.Add(10);
			}
		}
	}
}
