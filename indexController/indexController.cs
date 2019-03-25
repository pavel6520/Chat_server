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
    public indexController() : base() { }

    public void indexAction() {
        Echo("IndexController - indexAction WORKED!<br>");
        MySqlConnection connection = new MySqlConnection("server=127.0.0.1;port=3306;user=root;password=6520;database=chat;");
        connection.Open();
        MySqlCommand command = new MySqlCommand("select * from testTable where string like '%ab%'", connection);
        EchoFromMySQL(connection, command.ExecuteReader(), reader => {
            if (reader.Read()) {
                return $"<span>ТЕСТОВЫЙ ВЫВОД => {reader[0].ToString()} === {reader[1].ToString()}</span><br>";
            }
            else {
                return null;
            }
        });
    }

	public void mssqlAction() {
		Echo("IndexController - mssqlAction WORKED!<br>");
		SqlConnection connection = new SqlConnection(@"Data Source=62.76.36.20;Initial Catalog=outside;User Id=web_man;Password=dVhj7!wKM");

		connection.Open();
		SqlCommand command = new SqlCommand("select w.idisuup, family, name, secondname, NameScienceLevel, NameScienceRank, date_edit, Address, Phone, Email " +
			"from worker2014 w " +
			"join employeeInfo2016 ei on w.idisuup = ei.employee_id " +
			"where activeEmpl = 'y'", connection);
		EchoFromMSSQL(connection, command.ExecuteReader(), reader => {
			if (reader.Read()) {
				string s = null;
				for (int i = 0; i < reader.FieldCount; i++) {
					s += $"<span><b>{reader.GetName(i)}</b>: { reader[i].ToString()}</span><br>";
				}
				s += "<br>";
				return s;
			}
			else {
				return null;
			}
		});
	}

    public void testAction() {
        Echo("<html><body><span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>" +
            "<span>IndexController - testAction WORKED!</span><br>");
    }
}