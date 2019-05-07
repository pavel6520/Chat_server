using ConnectionWorker;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

public class indexController : ControllerWorker {
	public void indexAction() {
		Echo($"<a href=\"https://{_helper.domainName}/auth/login\">Перейти к авторизации</a><br>");
		Echo($"<a href=\"https://{_helper.domainName}/auth/signin\">Перейти к регистрации</a>");
		//Echo("<button id=\"testb\"></button><script>$('#testb').on('click', function(){ws.send(JSON.stringify({path: 'index',type: 'test',body: 'test message ' + new Date().getTime()}));});</script>");
	}

	public void indexActionWS(string type, string body) {
	}
}