function cache(key, value) 
{
    if (typeof value == 'undefined') {
        return cache[key];
    }
    cache[key] = value;
}

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

function ClientWrapMessageGet(login, message){
	var json = {
		to: login,
		message: message
	};
	return ClientWrapGet("message", JSON.stringify(json));
};

function authFunc(){
	var json = ClientWrapLoginWithPassGet(document.getElementById("authLogin").value, document.getElementById("authPass").value);
	
	ws.onmessage = function(evt){
		console.log(evt.data);
		if (evt.data == "LOGINED-SUCCSESS-ENTER-CHAT")
			enterChat();
	};
	ws.send(json);
};

function regFunc() {
	var json = ClientWrapRegGet(document.getElementById("regLogin").value, document.getElementById("regPass").value, document.getElementById("regEmail").value);
	
	ws.onmessage = function(evt){
		console.log(evt.data);
		if (evt.data == "REGISTR-SUCCSESS-ENTER-CHAT")
			enterChat();
	};
	ws.send(json);
};


function enterChat(){
	//$("#chatText").append("<img src=\"/image/img\">");
	//document.getElementById("formErrorLogin").setAttribute("style", "display:none");
	$('#blockAuth').css('display', 'none');
	
	//$("#chatBtn").click(sendmessage);
	$('#ChatTextArea').keydown( function(ev){ inputControl.keyD(ev.which); if(ev.which == '13'){ inputControl.send(ev);} } )
		.keyup( function(ev){ inputControl.keyU(ev.which); if(ev.which == '13'){  inputControl.clear();} } );
	var block = $("#ChatBodyList2");
	ws.onmessage = function(evt) { 
		var json = JSON.parse(evt.data);
		if (json.mestype == 'mesToClient') {
			json = JSON.parse(json.body);
			date = new Date(json.date);
			//console.log((date.getHours() < 10 ? '0' + date.getHours() : date.getHours()) + ':' + (date.getMinutes() < 10 ? '0' + date.getMinutes() : date.getMinutes()));
			$('#ChatBodyList2').append('<li class="ChatBodyMessage"><div class="ChatBodyMessageLogin">' + json.from + '</div><div class="ChatBodyMessageText">' + json.message + '</div></li>');
			block[0].scrollTop = block[0].scrollHeight;
		}
	};
	
	document.getElementById("blockChat").setAttribute("style", "");
};