using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class RenderClass {
		public string layout { get; private set; }
		public bool isEnabled { get; private set; }

		public RenderClass() {
			isEnabled = true;
			layout = "default";
		}

		public void DissableRender() { isEnabled = false; }

		public void SetLayout(string name) { layout = name; }
	}
}
