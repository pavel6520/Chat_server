function cache(key, value) {if (typeof value == 'undefined') { return cache[key]; }cache[key] = value;}

function authFunc(){
	if (cache('inputsAuth')[0].value.length >= 4 && cache('inputsAuth')[1].value.length >= 4) {
		var json = struct.CWLoginWithPass(cache('inputsAuth')[0].value, cache('inputsAuth')[1].value);
		ws.onmessage = function(evt){
			console.log(evt.data);
			if (evt.data == "LOGINED-SUCCSESS-ENTER-CHAT")
				loadData();
		};
		ws.send(json);
	}
};

function regFunc() {
	if (cache('inputsAuth')[2].value.length >= 4 && cache('inputsAuth')[3].value.length >= 4 && cache('inputsAuth')[4].value.length >= 4) {
		var json = struct.CWReg(cache('inputsAuth')[2].value, cache('inputsAuth')[3].value, cache('inputsAuth')[4].value);
		ws.onmessage = function(evt){
			console.log(evt.data);
			if (evt.data == "REGISTR-SUCCSESS-ENTER-CHAT")
				loadData();
		};
		ws.send(json);
	}
};

function loadData(){
	cache('blockAuth').attr('style', 'display: none;');
	cache('blockErr').removeAttr('style');
	cache('blockErr').text('LOAD DATA...');
	ws.onmessage = function(evt) {
		var json = JSON.parse(evt.data);
		if (json.mestype == 'dat') {
			json = JSON.parse(json.body);
			login = json.login;
			//onlineList = json.online;
			//onlineList.forEach(function(item, i, arr){if (item == login) arr.splice(i, 1);});
			//console.log(onlineList);
			for (i = json.history.length - 1; i >= 0; i--)
			{
				date = new Date(json.history[i].date);
				date = (date.getHours() < 10 ? '0' + date.getHours() : date.getHours()) + ':' + (date.getMinutes() < 10 ? '0' + date.getMinutes() : date.getMinutes());
				cache('MessageList').append('<li class="ChatBodyMessage"><div class="ChatBodyMessageLogin">' + json.history[i].from + ' ' + date +
					'</div><div class="ChatBodyMessageText">' + json.history[i].message + '</div></li>');
				//console.log(json.history[i]);
				cache('MessageList')[0].scrollTop = cache('MessageList')[0].scrollHeight;
			}
			enterChat();
		}
		else cache('blockErr').text('ERROR LOAD');
	};
}

function enterChat(){
	/*$("#chatText").append("<img src=\"/image/img\">");*/
	cache('blockAuth').attr('style', 'display: none;');
	cache('blockErr').attr('style', 'display: none;');
	
	/*$("#chatBtn").click(sendmessage);*/
	cache('textArea').keydown( function(ev){ inputControl.keyD(ev.which); if(ev.which == '13'){ inputControl.send(ev);} } )
		.keyup( function(ev){ inputControl.keyU(ev.which); if(ev.which == '13'){  inputControl.clear();} } );
	
	ws.onmessage = function(evt) {
		var json = JSON.parse(evt.data);
		if (json.mestype == 'mes') {
			json = JSON.parse(json.body);
			date = new Date(json.date);
			date = (date.getHours() < 10 ? '0' + date.getHours() : date.getHours()) + ':' + (date.getMinutes() < 10 ? '0' + date.getMinutes() : date.getMinutes());
			cache('MessageList').append('<li class="ChatBodyMessage"><div class="ChatBodyMessageLogin">' + json.from + ' ' + date +
				'</div><div class="ChatBodyMessageText">' + json.message + '</div></li>');
			cache('MessageList')[0].scrollTop = cache('MessageList')[0].scrollHeight;
		}
	};
	cache('blockChat').removeAttr('style');
};