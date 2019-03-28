using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class defaultLayout : LayoutWorker {
	public void Init() {
		Echo("<html><head>");
		IncludeLayout("head");
		Echo("</head><body>");
		IncludeLayout("header");
		IncludeContent();
		Echo("</body></html>");
	}
}