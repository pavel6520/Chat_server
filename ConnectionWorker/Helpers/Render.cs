using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectionWorker.Helpers {
	[Serializable]
	public class RenderClass {
		public bool isDefault { get; private set; }
		public bool isEnabled { get; private set; }

		public void DissableRender() { isEnabled = false; }
	}
}
