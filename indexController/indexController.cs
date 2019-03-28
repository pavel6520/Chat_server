using ConnectionWorker;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class indexController : ControllerWorker {
    public indexController() : base() {
		var t1 = typeof(JsonSerializer);
		var t2 = typeof(MySqlConnection);
		var t3 = typeof(SqlConnection);
	}

    public void indexAction() {
        Echo("IndexController - indexAction WORKED!<br>");
        MySqlCommand command = new MySqlCommand("select * from testTable limit 1000");
        EchoMySQLReader(command, reader => {
            return $"<span>ТЕСТОВЫЙ ВЫВОД => {reader[0].ToString()} === {reader[1].ToString()}</span><br>";
        });
    }

	//public void mssqlAction() {
	//	Echo("IndexController - mssqlAction WORKED!<br>");
	//	SqlCommand command = new SqlCommand("select w.idisuup, family, name, secondname, NameScienceLevel, NameScienceRank, date_edit, Address, Phone, Email " +
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
	
	public void testAction() {
		_helper._render.DissableRender();
		Echo("IndexController - teestAction WORKED!<br>");
		MySqlCommand command = new MySqlCommand("select * from testTable limit 1000");
		EchoMySQLReader(command, reader => {
			return $"<span>ТЕСТОВЫЙ ВЫВОД => {reader[0].ToString()} === {reader[1].ToString()}</span><br>";
		});
	}
}