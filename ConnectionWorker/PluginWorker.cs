using MySql.Data.MySqlClient;
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
		private List<int> contentType;
		private List<string> contentString;
		private List<MySqlDataObject> contentMySqlDataObject;
		private List<MSSqlDataObject> contentMSSqlDataObject;

		public void _SetContext(HelperClass helper) {
			_helper = helper;
		}

		public void _Work(string methodName) {
			GetType().GetMethod($"{methodName}Action").Invoke(this, null);
		}

		public void Echo(string s) {
			if (contentType == null) {
				contentType = new List<int>(0);
			}
			if (contentString == null) {
				contentString = new List<string>(0);
			}
			contentType.Add(0);
			contentString.Add(s);
		}

		public void EchoFromMySQL(MySqlConnection connect, MySqlDataReader reader, Func<MySqlDataReader, string> func) {
			if (contentType == null) {
				contentType = new List<int>(0);
			}
			if (contentMySqlDataObject == null) {
				contentMySqlDataObject = new List<MySqlDataObject>(0);
			}
			contentType.Add(1);
			contentMySqlDataObject.Add(new MySqlDataObject {
				connect = connect,
				reader = reader,
				func = func
			});
		}

		public void EchoFromMSSQL(SqlConnection connect, SqlDataReader reader, Func<SqlDataReader, string> func) {
			if (contentType == null) {
				contentType = new List<int>(0);
			}
			if (contentMSSqlDataObject == null) {
				contentMSSqlDataObject = new List<MSSqlDataObject>(0);
			}
			contentType.Add(2);
			contentMSSqlDataObject.Add(new MSSqlDataObject {
				connect = connect,
				reader = reader,
				func = func
			});
		}

		public string _GetNextContentString() {
			string s = null;
			if (contentType.Count > 0) {
				if (contentType[0] == 0) {
					contentType.RemoveAt(0);
					s = contentString[0];
					contentString.RemoveAt(0);
				}
				else if (contentType[0] == 1) {
					s = contentMySqlDataObject[0].func(contentMySqlDataObject[0].reader);
					if (s == null) {
						contentType.RemoveAt(0);
						contentMySqlDataObject[0].reader.Close();
						contentMySqlDataObject[0].connect.Close();
						contentMySqlDataObject.RemoveAt(0);
						s = _GetNextContentString();
					}
				}
				else if (contentType[0] == 2) {
					s = contentMSSqlDataObject[0].func(contentMSSqlDataObject[0].reader);
					if (s == null) {
						contentType.RemoveAt(0);
						contentMSSqlDataObject[0].reader.Close();
						contentMSSqlDataObject[0].connect.Close();
						contentMSSqlDataObject.RemoveAt(0);
						s = _GetNextContentString();
					}
				}
			}
			return s;
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