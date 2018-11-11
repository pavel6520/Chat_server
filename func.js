function ClientWrapGet(mestype, body){
	var json = {
		mestype: mestype,
		body: body
	};
	return JSON.stringify(json);
};

function ClientWrapLoginWithPassGet(login, pass){
	var json = {
		login: login,
		withpass: true,
		pass: pass,
		key: ""
	};
	return ClientWrapGet("login", JSON.stringify(json));
};

function ClientWrapLoginWithKeyGet(login, key){
	var json = {
		login: login,
		withpass: false,
		pass: "",
		key: key
	};
	return ClientWrapGet("login", JSON.stringify(json));
};

function ClientWrapRegGet(login, pass, email){
	var json = {
		login: login,
		pass: pass,
		email: email
	};
	return ClientWrapGet("registr", JSON.stringify(json));
};

function authFunc(){
	var json = ClientWrapLoginWithPassGet(document.getElementById("authLogin").value, document.getElementById("authPass").value);
	
	ws.onmessage = function(evt){
		console.log(evt.data);
		if (evt.data == "LOGINED-SUCCSESS-ENTER-CHAT")
			enterChat();
		else
			document.getElementById("formErrorLogin").setAttribute("style", "");
	};
	ws.send(json);
};

function regFunc() {
	var json = ClientWrapRegGet(document.getElementById("regLogin").value, document.getElementById("regPass").value, document.getElementById("regEmail").value);
	
	ws.onmessage = function(evt){
		console.log(evt.data);
		if (evt.data == "REGISTR-SUCCSESS-ENTER-CHAT")
			enterChat();
		else
			document.getElementById("formErrorLogin").setAttribute("style", "");
	};
	ws.send(json);
};

function sendmessage(){
	ws.send($('input[id="chatInput"]').val());
	$('input[id="chatInput"]').val('');
};

function enterChat(){
	document.getElementById("formErrorLogin").setAttribute("style", "display:none");
	document.getElementById("formAuth").setAttribute("style", "display:none");
	
	$("#chatBtn").click(sendmessage);
	$("#chatInput").keyup(function(ev){if(ev.keyCode == 13){ev.preventDefault(); sendmessage();}});
	ws.onmessage = function(evt) { $("#chatText").append("<p>"+evt.data+"</p>"); };
	
	document.getElementById("formChat").setAttribute("style", "");
};