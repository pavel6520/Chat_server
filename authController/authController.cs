using ConnectionWorker;
using ConnectionWorker.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class authController : ControllerWorker {
    public void loginAction() {
		Echo("<script src=\"https://code.jquery.com/jquery-3.3.1.min.js\"></script>");

		if (_helper.isSecureConnection) {
			Echo("<span>Load with SSL crypt</span><br>");
        }
        Echo("<span>IndexController - loginAction WORKED!</span>");
    }

    public void signinAction() {
        Echo("<html><body></body></html>");
    }
}
