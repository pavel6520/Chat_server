using ConnectionWorker;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Net;

public class authStatic : PluginWorker {
	public bool checkSession(MySqlConnection connection = null) {
		bool createConnection = connection == null;
		bool res = false;
		var authCookie = _helper.Request.Cookies["auth"];
		if (authCookie != null) {
			if (createConnection) {
				connection = new MySqlConnection(_helper.dbConnectString);
				connection.Open();
			}
			MySqlCommand command = new MySqlCommand("select login, datecreate from userauthkey where `key` = @key", connection);
			command.Parameters.AddWithValue("@key", authCookie.Value);
			MySqlDataReader reader = command.ExecuteReader();
			if (reader.Read()) {
				_helper.Auth = new ConnectionWorker.Helpers.AuthInfo(Convert.ToString(reader["login"])) {
					TimeCreate = DateTime.Parse(reader["datecreate"].ToString())
				};
				res = true;
			}
			else {
				_helper.Auth = new ConnectionWorker.Helpers.AuthInfo();
			}
			reader.Close();
			if (res) {
				updateSession(authCookie.Value, connection);
			}
			else {
				delSession(connection);
			}

			if (createConnection) {
				connection.Close();
			}
		}
		return res;
	}

	public void updateSession(string key, MySqlConnection connection = null) {
		bool createConnection = connection == null;
		if (createConnection) {
			connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
		}

		MySqlCommand command = new MySqlCommand("update userauthkey set datecreate = @datetime where `key` = @key", connection);
		command.Parameters.AddWithValue("@key", key);
		command.Parameters.AddWithValue("@datetime", DateTime.UtcNow);
		command.ExecuteNonQuery();

		if (createConnection) {
			connection.Close();
		}
	}

	public void delSession(MySqlConnection connection = null) {
		bool createConnection = connection == null;
		if (createConnection) {
			connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
		}

		var authCookie = _helper.Request.Cookies["auth"];
		if (authCookie != null) {
			MySqlCommand command = new MySqlCommand("delete from userauthkey where `key` = @key", connection);
			command.Parameters.AddWithValue("@key", authCookie.Value);
			command.Parameters.AddWithValue("@datecreate", DateTime.UtcNow.AddDays(-1));
			command.ExecuteNonQuery();
			connection.Close();
			_helper.Responce.Headers.Add(HttpResponseHeader.SetCookie, $"{authCookie.Name}={authCookie.Value}; Max-Age=-1; path=/;");
		}

		if (createConnection) {
			connection.Close();
		}
	}

	public void addSession(MySqlConnection connection = null) {
		bool createConnection = connection == null;
		var authCookie = _helper.Request.Cookies["auth"];
		if (authCookie != null) {
			_helper.Responce.Headers.Add(HttpResponseHeader.SetCookie, $"{authCookie.Name}={authCookie.Value}; Max-Age=-1; path=/;");
		}
		DateTime time = DateTime.UtcNow;
		string hash = BitConverter.ToString(new SHA256Managed().ComputeHash(Encoding.Default.GetBytes($"{_helper.Auth.Login}{time}"))).Replace("-", "");

		if (createConnection) {
			connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
		}
		MySqlCommand command;
		command = new MySqlCommand("insert into userauthkey (`key`, login, datecreate) value(@key, @login, @datecreate)", connection);
		command.Parameters.AddWithValue("@key", hash);
		command.Parameters.AddWithValue("@login", _helper.Auth.Login);
		command.Parameters.AddWithValue("@datecreate", time);
		command.ExecuteNonQuery();
		//_helper.Responce.Headers.Add(System.Net.HttpResponseHeader.SetCookie, $"auth={hash}; secure; HttpOnly; domain={_helper.domainName}; path=/; Expires={time.AddDays(1).ToString("R")}");
		_helper.Responce.Headers.Add(HttpResponseHeader.SetCookie, $"auth={hash}; secure; HttpOnly; path=/; Expires={time.AddDays(1).ToString("R")}");

		if (createConnection) {
			connection.Close();
		}
	}

	public bool loginUser() {
		Uri uri;
		if (_helper.Request.Content != null) {
			UriBuilder uriB = new UriBuilder(_helper.Request.Url);
			uriB.Query = _helper.Request.ContentEncoding.GetString(_helper.Request.Content);
			uri = uriB.Uri;
		}
		else {
			uri = _helper.Request.Url;
		}
		Dictionary<string, string> keyValues = ConnectionWorker.Helpers.UriHelper.DecodeQueryParameters(uri);
		bool res = false;
		string login = keyValues["login"];
		string pass = keyValues["password"];
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
				res = true;
				reader.Close();
				addSession(connection);
			}
			else {
				reader.Close();
			}

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
		string login = keyValues["login"];
		string pass = keyValues["password"];
		string passconf = keyValues["password_confirm"];
		if (login != null && pass != null && passconf != null && pass == passconf) {
			MySqlConnection connection = new MySqlConnection(_helper.dbConnectString);
			connection.Open();
			MySqlCommand command = new MySqlCommand("insert into user (login, hash, datereg) values(@login, @pass, @datereg)", connection);
			command.Parameters.AddWithValue("@login", login);
			command.Parameters.AddWithValue("@pass", pass);
			command.Parameters.AddWithValue("@datereg", DateTime.Now);
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