﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ChatWebService {
	public partial class ChatWebService : ServiceBase {
		WebServerCore.Core core;

		public ChatWebService() {
			InitializeComponent();
		}

		protected override void OnStart(string[] args) {
			string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
			Environment.CurrentDirectory = path.Substring(0, path.LastIndexOf(System.IO.Path.DirectorySeparatorChar));

			//System.Threading.Thread.Sleep(10000);
			core = new WebServerCore.Core();
			core.Start();
		}

		protected override void OnStop() {
			core.Close();
			core = null;
		}
	}
}
