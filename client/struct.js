var struct = {
	CW: function(mestype, body){
		var json = {
			type: mestype,
			body: body
		};
		return JSON.stringify(json);
	},
	CWLoginWithPass: function(login, pass){
		var json = {
			login: login,
			withpass: true,
			pass: pass,
			key: ""
		};
		return this.CW("log", JSON.stringify(json));
	},
	CWLoginWithKeyGet: function(login, key){
		var json = {
			login: login,
			withpass: false,
			pass: "",
			key: key
		};
		return this.CW("log", JSON.stringify(json));
	},
	CWReg: function(login, pass, email){
		var json = {
			login: login,
			pass: pass,
			email: email
		};
		return this.CW("reg", JSON.stringify(json));
	},
	CWMessage: function(login, message){
		var json = {
			to: login,
			message: message
		};
		return this.CW("mes", JSON.stringify(json));
	}
}