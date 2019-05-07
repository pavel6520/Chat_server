using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class headerLayout : LayoutWorker {
	public void Init() {
		Echo("<header  class=\"page-header\">");
		if (_helper.Auth != null) {
			Echo($"<h3>{_helper.Auth.Login}</h3>");
		}
		Echo("</header>");
	}
}
