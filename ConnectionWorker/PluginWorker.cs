﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ConnectionWorker.Helpers;

namespace ConnectionWorker {
	public abstract class PluginWorker : MarshalByRefObject {
		public HelperClass _helper;
		private List<byte> contentType;
		private List<string> contentString;
		private List<MySqlDataObject> contentMySqlDataObject;
		private List<MSSqlDataObject> contentMSSqlDataObject;

		public void _SetContext(HelperClass helper) {
			_helper = helper;
			contentType = new List<byte>(0);
			contentString = new List<string>(0);
			contentMySqlDataObject = new List<MySqlDataObject>(0);
			contentMSSqlDataObject = new List<MSSqlDataObject>(0);
		}

		public void _Work(string methodName) {
			GetType().GetMethod(methodName).Invoke(this, null);
		}

		public void Echo(string s) {
			contentType.Add(0);
			contentString.Add(s);
		}

		public void EchoMySQLReader(MySqlCommand command, Func<MySqlDataReader, string> func) {
			MySqlConnection connect = new MySqlConnection("server=127.0.0.1;port=3306;user=root;password=6520;database=chat;");
			connect.Open();
			command.Connection = connect;
			contentType.Add(20);
			contentMySqlDataObject.Add(new MySqlDataObject {
				connect = connect,
				reader = command.ExecuteReader(),
				func = func
			});
		}

		public void EchoMSSqlReader(SqlCommand command, Func<SqlDataReader, string> func) {
			SqlConnection connect = new SqlConnection(@"Data Source=62.76.36.20;Initial Catalog=outside;User Id=web_man;Password=dVhj7!wKM");
			connect.Open();
			command.Connection = connect;
			contentType.Add(30);
			contentMSSqlDataObject.Add(new MSSqlDataObject {
				connect = connect,
				reader = command.ExecuteReader(),
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
						break;
					case 20:
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
			public MySqlDataReader reader;
			public Func<MySqlDataReader, string> func;
		}

		private class MSSqlDataObject {
			public SqlConnection connect;
			public SqlDataReader reader;
			public Func<SqlDataReader, string> func;
		}
	}
}