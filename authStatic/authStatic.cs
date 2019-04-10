using ConnectionWorker;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class authStatic : PluginWorker {
	public bool checkSession() {
		bool res= false;
		var authCookie = _helper.Request.Cookies["auth"];
		if (authCookie != null) {
			MySqlConnection connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
			MySqlCommand command = new MySqlCommand("select login, datecreate from userauthkey where hash = @hash", connection);
			command.Parameters.AddWithValue("@hash", authCookie.Value);
			MySqlDataReader reader = command.ExecuteReader();
			if (reader.Read()) {
				_helper.Auth = new ConnectionWorker.Helpers.AuthInfo(Convert.ToString(reader["login"]), DateTime.Parse(reader["datecreate"].ToString()));
				res = true;
			}

			reader.Close();
			connection.Close();
		}
		return res;
	}

	public void updateSession() {

	}

	public void delSession() {
		var authCookie = _helper.Request.Cookies["auth"];
		if (authCookie != null) {
			MySqlConnection connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
			MySqlCommand command = new MySqlCommand("delete from userauthkey where hash = @hash or datecreate < @datetime", connection);
			command.Parameters.AddWithValue("@hash", authCookie.Value);
			command.Parameters.AddWithValue("@datetime", DateTime.UtcNow.AddDays(-1));
			command.ExecuteNonQuery();
			connection.Close();
			_helper.Responce.Headers.Add(System.Net.HttpResponseHeader.SetCookie, $"{authCookie.Name}={authCookie.Value}; Max-Age=-1; path=/;");
		}
	}

	public void addSession() {
		var authCookie = _helper.Request.Cookies["auth"];
		if (authCookie != null) {
			_helper.Responce.Headers.Add(System.Net.HttpResponseHeader.SetCookie, $"{authCookie.Name}={authCookie.Value}; Max-Age=-1; path=/;");
		}
		DateTime time = DateTime.UtcNow;
		string hash = BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.Default.GetBytes($"{_helper.Auth.Login}{time}"))).Replace("-", "");

		MySqlConnection connection = new MySqlConnection(_helper.dbConnectString);
		connection.Open();
		MySqlCommand command;
		if (authCookie != null) {
			command = new MySqlCommand("update userauthkey set datecreate = @datetime where hash = @hash", connection);
			command.Parameters.AddWithValue("@hash", authCookie.Value);
		}
		else {
			command = new MySqlCommand("insert into userauthkey (login, datecreate, hash) values(@login, @datetime, @hash)", connection);
			command.Parameters.AddWithValue("@login", _helper.Auth.Login);
			command.Parameters.AddWithValue("@hash", hash);
		}
		command.Parameters.AddWithValue("@datetime", time);
		command.ExecuteNonQuery();
		//_helper.Responce.Headers.Add(System.Net.HttpResponseHeader.SetCookie, $"auth={hash}; secure; HttpOnly; domain={_helper.domainName}; path=/; Expires={time.AddDays(1).ToString("R")}");
		_helper.Responce.Headers.Add(System.Net.HttpResponseHeader.SetCookie, $"auth={hash}; secure; HttpOnly; path=/; Expires={time.AddDays(1).ToString("R")}");
		
		connection.Close();


		//Console.WriteLine(BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.Default.GetBytes(Console.ReadLine()))).Replace("-", ""));
	}

	public bool loginUser() {
		Uri uri;
		if (_helper.Request.Content != null) {
			UriBuilder uriB = new UriBuilder(_helper.Request.Url);
			uriB.Query = Encoding.UTF8.GetString(_helper.Request.Content);
			uri = uriB.Uri;
		}
		else {
			uri = _helper.Request.Url;
		}
		Dictionary<string, string> keyValues = ConnectionWorker.Helpers.UriHelper.DecodeQueryParameters(uri);
		bool res = false;
		string login = keyValues["logininput"];
		string pass = keyValues["passwordinput"];
		if (login != null && pass != null) {
			MySqlConnection connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
			MySqlCommand command = new MySqlCommand("select login from user where login = @login and hash = @pass", connection);
			command.Parameters.AddWithValue("@login", login);
			command.Parameters.AddWithValue("@pass", pass);
			//try {
			MySqlDataReader reader = command.ExecuteReader();
			if (reader.Read()) {
				_helper.Auth = new ConnectionWorker.Helpers.AuthInfo() { Login = Convert.ToString(reader["login"]) };
				addSession();
				res = true;
			}

			reader.Close();
			connection.Close();
			//}
			//catch { }
		}
		return res;
	}

	public bool addUser() {
		bool res = false;
		Uri uri;
		if (_helper.Request.Content != null) {
			UriBuilder uriB = new UriBuilder(_helper.Request.Url);
			uriB.Query = Encoding.UTF8.GetString(_helper.Request.Content);
			uri = uriB.Uri;
		}
		else {
			uri = _helper.Request.Url;
		}
		Dictionary<string, string> keyValues = ConnectionWorker.Helpers.UriHelper.DecodeQueryParameters(uri);
		string login = keyValues["logininput"];
		string pass = keyValues["passwordinput"];
		string passconf = keyValues["password_confirminput"];
		if (login != null && pass != null && passconf != null && pass == passconf) {
			MySqlConnection connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
			MySqlCommand command = new MySqlCommand("insert into user (login, hash, regdate) values(@login, @pass, @date)", connection);
			command.Parameters.AddWithValue("@login", login);
			command.Parameters.AddWithValue("@pass", pass);
			command.Parameters.AddWithValue("@date", DateTime.Now);
			try {
				command.ExecuteNonQuery();
				res = true;
				_helper.Auth = new ConnectionWorker.Helpers.AuthInfo() { Login = login };
				addSession();
			}
			catch { }
			connection.Close();
		}
		return res;
	}
}