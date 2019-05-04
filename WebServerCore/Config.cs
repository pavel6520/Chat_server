using SharpConfig;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Serialization;

namespace WebServerCore {
    public static class Config {
		private static string FileName;
		private static bool loaded;
		public static string Application;
		public static bool Debug;
		public static string Domain;
		public static bool SSLEnable;
		public static string SSLFileName;
		public static string SSLPass;
		public static string DBConnectionString;

        public static void Read(string file) {
			FileName = file;
			if (!(loaded = File.Exists(FileName))) {
				File.Create(FileName).Close();
			}
			var cfg = Configuration.LoadFromFile(FileName);
			if (cfg["global"]["Application"].IsEmpty) {
				cfg["global"]["Application"].StringValue = "development";
			}
			Application = cfg["global"]["Application"].StringValue;

			if (cfg[Application]["Debug"].IsEmpty) {
				cfg[Application]["Debug"].BoolValue = false;
			}
			Debug = cfg[Application]["Debug"].BoolValue;

			if (cfg[Application]["domain"].IsEmpty) {
				cfg[Application]["domain"].StringValue = "example.com";
			}
			Domain = cfg[Application]["domain"].StringValue;

			if (cfg[Application]["SSLEnable"].IsEmpty) {
				cfg[Application]["SSLEnable"].BoolValue = false;
			}
			SSLEnable = cfg[Application]["SSLEnable"].BoolValue;

			if (cfg[Application]["SSLFileName"].IsEmpty) {
				cfg[Application]["SSLFileName"].StringValue = "cert.pfx";
			}
			SSLFileName = cfg[Application]["SSLFileName"].StringValue;

			if (cfg[Application]["SSLPass"].IsEmpty) {
				cfg[Application]["SSLPass"].StringValue = "1234";
			}
			SSLPass = cfg[Application]["SSLPass"].StringValue;

			if (cfg[Application]["DBConnectionString"].IsEmpty) {
				cfg[Application]["DBConnectionString"].StringValue = "server=;port=;user=;password=;database=;";
			}
			DBConnectionString = cfg[Application]["DBConnectionString"].StringValue;
			if (!loaded) {
				cfg.SaveToFile(FileName, Encoding.UTF8);
			}
		}
    }
}