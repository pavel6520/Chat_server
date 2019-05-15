using ConnectionWorker;

public class headLayout : LayoutWorker {
	public void Init() {
		Echo("<head>");
		Echo("<meta charset=\"utf-8\">");
		Echo("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1, shrink-to-fit=no\">");
		Echo("<meta name=\"google-site-verification\" content=\"d8v35oUjgaOXuv6jhU1vz_lGMDm76ceHiBzvt6cCXbY\" />");
		Echo("<meta name=\"yandex-verification\" content=\"96a3ade8f953fa04\" />");
		Echo("<link href=\"/assets/css/bootstrap.min.css\" rel=\"stylesheet\">");
		Echo("<link href=\"/assets/css/jasny-bootstrap.min.css\" rel=\"stylesheet\">");
		Echo("<link href=\"/assets/css/style.css\" rel=\"stylesheet\">");
		//Echo("<link href=\"/assets/css/jquery-ui.css\" rel=\"stylesheet\">");
		//Echo("<link href=\"/assets/css/jquery-ui.structure.css\" rel=\"stylesheet\">");
		//Echo("<link href=\"/assets/css/jquery-ui.theme.css\" rel=\"stylesheet\">");
		Echo("<script src=\"https://code.jquery.com/jquery-3.4.1.min.js\" integrity=\"sha256-CSXorXvZcTkaix6Yvo6HppcZGetbYMGWSFlBw8HfCJo=\" crossorigin=\"anonymous\"></script>");
		//Echo("<script src=\"/assets/js/jquery-3.4.1.min.js\"></script>");
		//Echo("<script src=\"/assets/js/jquery-ui.js\"></script>");
		//Echo("<script src=\"/assets/js/func.js\"></script>");
		//Echo("<script src=\"/assets/js/struct.js\"></script>");
		Echo("<title>pavel6520 Chat</title>");
		Echo("</head>");
	}
}