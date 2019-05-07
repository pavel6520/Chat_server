using ConnectionWorker;

public class defaultLayout : LayoutWorker {
	public void Init() {
		Echo("<!doctype html><html>");
		IncludeLayout("head");
		Echo("<body>");
		IncludeLayout("header");
		IncludeContent();
		Echo("<script src=\"/assets/js/func.js\"></script>");
		Echo("<script src=\"/assets/js/popper.min.js\"></script>");
		Echo("<script src=\"/assets/js/bootstrap.js\"></script>");
		Echo("</body></html>");
	}
}