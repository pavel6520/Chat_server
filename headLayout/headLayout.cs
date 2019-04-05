using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class headLayout : LayoutWorker {
	public void Init() {
		Echo("<title>pavel6520 Chat</title>");
		Echo("<meta name=\"google-site-verification\" content=\"d8v35oUjgaOXuv6jhU1vz_lGMDm76ceHiBzvt6cCXbY\" />");
		Echo("<meta name=\"yandex-verification\" content=\"96a3ade8f953fa04\" />");
		Echo("<script src=\"https://code.jquery.com/jquery-3.3.1.min.js\"></script>");
		Echo("<meta charset=\"utf-8\">");
		Echo("<link href=\"/assets/css/style.css\" rel=\"stylesheet\">");
		Echo("<link href=\"/assets/css/jquery-ui.css\" rel=\"stylesheet\">");
		Echo("<link href=\"/assets/css/jquery-ui.structure.css\" rel=\"stylesheet\">");
		Echo("<link href=\"/assets/css/jquery-ui.theme.css\" rel=\"stylesheet\">");
		//Echo("<script src=\"/assets/css/jquery.js\"></script>");
		Echo("<script src=\"/assets/js/jquery-ui.js\"></script>");
		Echo("<script src=\"/assets/js/func.js\"></script>");
		Echo("<script src=\"/assets/js/struct.js\"></script>");
	}
}