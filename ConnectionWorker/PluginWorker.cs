using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ConnectionWorker.Helpers;
using System.Collections;

namespace ConnectionWorker {
	public abstract class PluginWorker : MarshalByRefObject {
		public string[] staticInclude;
		public HelperClass _helper;
		protected List<byte> contentType;
		protected List<string> contentString;
		private List<MySqlDataObject> contentMySqlDataObject;
		private List<MSSqlDataObject> contentMSSqlDataObject;

		public void _LoadDefault() {
			new MySqlCommand();
			new SqlCommand();
			new Newtonsoft.Json.JsonSerializer();
			new System.Transactions.TransactionException();
		}

		public void _SetHelper(HelperClass helper) {
			_helper = helper;
		}

		public string[] _GetStaticInclude() {
			return staticInclude;
		}

		public HelperClass _GetHelper() {
			return _helper;
		}

		public object _Work(string methodName = null, object[] args = null) {
			contentType = new List<byte>(0);
			contentString = new List<string>(0);
			contentMySqlDataObject = new List<MySqlDataObject>(0);
			contentMSSqlDataObject = new List<MSSqlDataObject>(0);
			System.Reflection.MethodInfo m = GetType().GetMethod("Init");
			if (m != null) {
				m.Invoke(this, null);
			}
			if (methodName != null && _helper.Render.isEnabled) {
				return GetType().GetMethod(methodName).Invoke(this, args);
			}
			else {
				return null; 
			}
		}

		public object _WorkWS(string methodName = null, object[] args = null) {
			contentType = new List<byte>(0);
			contentString = new List<string>(0);
			contentMySqlDataObject = new List<MySqlDataObject>(0);
			contentMSSqlDataObject = new List<MSSqlDataObject>(0);
			System.Reflection.MethodInfo m = GetType().GetMethod("InitWS");
			if (m != null) {
				m.Invoke(this, null);
			}
			if (methodName != null && _helper.Render.isEnabled) {
				return GetType().GetMethod(methodName).Invoke(this, args);
			}
			else {
				return null;
			}
		}

		public void Echo(string s) {
			contentType.Add(0);
			contentString.Add(s);
		}

		public void IncludeLayout(string name) {
			contentType.Add(11);
			contentString.Add(name);
		}

		public void EchoMySQLReader(MySqlCommand command, Func<MySqlDataReader, string> func) {
			contentType.Add(20);
			contentMySqlDataObject.Add(new MySqlDataObject {
				command = command,
				func = func
			});
		}

		public void EchoMSSqlReader(SqlCommand command, Func<SqlDataReader, string> func) {
			contentType.Add(30);
			contentMSSqlDataObject.Add(new MSSqlDataObject {
				command = command,
				func = func
			});
		}

		public EchoClass _GetNextContent() {
			EchoClass ec = null;
			if (contentType.Count > 0) {
				switch (contentType[0]) {
					case 0:
						contentType.RemoveAt(0);
						ec = new EchoClass { type = EchoClass.EchoType.String, param = contentString[0] };
						contentString.RemoveAt(0);
						break;
					case 10:
						contentType.RemoveAt(0);
						ec = new EchoClass { type = EchoClass.EchoType.Content };
						break;
					case 11:
						contentType.RemoveAt(0);
						ec = new EchoClass { type = EchoClass.EchoType.Layout, param = contentString[0] };
						contentString.RemoveAt(0);
						break;
					case 20:
						if (contentMySqlDataObject[0].connect == null) {
							contentMySqlDataObject[0].connect = new MySqlConnection("server=127.0.0.1;port=3306;user=root;password=6520;database=chat;");
							contentMySqlDataObject[0].connect.Open();
							contentMySqlDataObject[0].command.Connection = contentMySqlDataObject[0].connect;
							contentMySqlDataObject[0].reader = contentMySqlDataObject[0].command.ExecuteReader();
						}
						if (contentMySqlDataObject[0].reader.Read()) {
							ec = new EchoClass { type = EchoClass.EchoType.String, param = contentMySqlDataObject[0].func(contentMySqlDataObject[0].reader) };
						}
						else {
							contentType.RemoveAt(0);
							contentMySqlDataObject[0].reader.Close();
							contentMySqlDataObject[0].connect.Close();
							contentMySqlDataObject.RemoveAt(0);
							ec = _GetNextContent();
						}
						break;
					case 30:
						if (contentMSSqlDataObject[0].connect == null) {
							contentMSSqlDataObject[0].connect = new SqlConnection(@"Data Source=62.76.36.20;Initial Catalog=outside;User Id=web_man;Password=dVhj7!wKM");
							contentMSSqlDataObject[0].connect.Open();
							contentMSSqlDataObject[0].command.Connection = contentMSSqlDataObject[0].connect;
							contentMSSqlDataObject[0].reader = contentMSSqlDataObject[0].command.ExecuteReader();
						}
						if (contentMSSqlDataObject[0].reader.Read()) {
							ec = new EchoClass { type = EchoClass.EchoType.String, param = contentMSSqlDataObject[0].func(contentMSSqlDataObject[0].reader) };
						}
						else {
							contentType.RemoveAt(0);
							contentMSSqlDataObject[0].reader.Close();
							contentMSSqlDataObject[0].connect.Close();
							contentMSSqlDataObject.RemoveAt(0);
							ec = _GetNextContent();
						}
						break;
				}
			}
			else {
				ec = new EchoClass(EchoClass.EchoType.End);
			}
			return ec;
		}

		private class MySqlDataObject {
			public MySqlConnection connect;
			public MySqlCommand command;
			public MySqlDataReader reader;
			public Func<MySqlDataReader, string> func;
		}

		private class MSSqlDataObject {
			public SqlConnection connect;
			public SqlCommand command;
			public SqlDataReader reader;
			public Func<SqlDataReader, string> func;
		}
	}
}