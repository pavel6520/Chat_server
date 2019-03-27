using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class defaultLayout : LayoutWorker {
	public void Init() {
		Echo("<html><head><title>Default Layout WORKED!</title></head><body>");
		IncludeContent();
		Echo("</body></html>");
	}
}