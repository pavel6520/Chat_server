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
		Echo("<button id=\"testb\"></><script>$('#testb').on('click', function(){ws.send(JSON.stringify({path: 'index',type: 'test',body: 'test message ' + new Date().getTime()}));});</script>");
	}

	public void indexActionWS(string type, string body) {
		Console.WriteLine(body);
	}

	//    public void indexAction() {
	//        Echo("IndexController - indexAction WORKED!<br>");
	//        MySqlCommand command = new MySqlCommand("select * from testTable limit 1000");
	//        EchoMySQLReader(command, reader => {
	//            return $"<span>ТЕСТОВЫЙ ВЫВОД => {reader[0].ToString()} === {reader[1].ToString()}</span><br>";
	//        });
	//    }

	//public void mssqlAction() {
	//	Echo("IndexController - mssqlAction WORKED!<br>");
	//	SqlCommand command = new SqlCommand("select top 100 * " +
	//		"from worker2014 w " +
	//		"join employeeInfo2016 ei on w.idisuup = ei.employee_id " +
	//		"where activeEmpl = 'y'");
	//	EchoMSSqlReader(command, reader => {
	//		string s = null;
	//		for (int i = 0; i < reader.FieldCount; i++) {
	//			s += $"<span><b>{reader.GetName(i)}</b>: { reader[i].ToString()}</span><br>";
	//		}
	//		s += "<br>";
	//		return s;
	//	});
	//}

	//   public void testAction() {
	//	Echo("IndexController - testAction WORKED!<br>");
	//	MySqlConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;user=root;password=6520;database=chat;");
	//	connection.Open();
	//	MySqlCommand command = new MySqlCommand("select * from testTable limit 5000", connection);
	//	string s = null;
	//	MySqlDataReader dataReader = command.ExecuteReader();
	//	while (dataReader.Read()) {
	//		s += "<span>ТЕСТОВЫЙ ВЫВОД => " + dataReader[0].ToString() + " === " + dataReader[1].ToString() + "</span><br>";
	//	}
	//	Echo(s);
	//}

	//public void testAction() {
	//	Echo("IndexController - teestAction WORKED!<br>");
	//	MySqlConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;user=root;password=6520;database=chat;");
	//	connection.Open();
	//	MySqlCommand command = new MySqlCommand("select * from testTable limit 1000", connection);
	//	string s = null;
	//	MySqlDataReader dataReader = command.ExecuteReader();
	//	while (dataReader.Read()) {
	//		s += "<span>ТЕСТОВЫЙ ВЫВОД => " + dataReader[0].ToString() + " === " + dataReader[1].ToString() + "</span><br>";
	//	}
	//	Echo(s);
	//}

	//public void test1Action() {
	//	Echo("IndexController - teestAction WORKED!<br>");
	//	MySqlConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;user=root;password=6520;database=chat;");
	//	connection.Open();
	//	MySqlCommand command = new MySqlCommand("select * from testTable limit 1000", connection);
	//	string s = null;
	//	MySqlDataReader dataReader = command.ExecuteReader();
	//	while (dataReader.Read()) {
	//		Echo("<span>ТЕСТОВЫЙ ВЫВОД => " + dataReader[0].ToString() + " === " + dataReader[1].ToString() + "</span><br>");
	//	}
	//}

	//unsafe public void testunsafe() {
	//	Console.WriteLine(context.Request.Url);
	//	byte[] buf = Encoding.UTF8.GetBytes("TESTMETHOD - WORK WITH REFERENSE");
	//	context.Response.ContentType = "text/html; charset=UTF-8";
	//	context.Response.OutputStream.Write(buf, 0, buf.Length);
	//}
}