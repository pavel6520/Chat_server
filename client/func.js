$(function(){
	cache('textArea', $('#ChatTextArea'));
	cache('inputsAuth', $('#blockAuth input'));
	cache('blockAuth', $('#blockAuth'));
	cache('blockErr', $('#blockErr'));
	cache('blockChat', $('#blockChat'));
	cache('ListDialog', $('#ChatBodyList1'));
	cache('Contacts', $('#ChatList3'));
	$('#tabs-logsign').tabs();
	
	/*ws = new WebSocket('ws://192.168.0.55:30000/websocket');*/
	ws = new WebSocket('ws://127.0.0.1:30000/websocket');
	/*ws = new WebSocket('ws://pavel6520.hopto.org:30000/websocket');*/
	
	ws.onopen = function() {
		cache('blockAuth').removeAttr('style');
	};
	ws.onerror = function(evt) { 
		cache('blockErr').removeAttr('style'); 
		cache('blockErr').text(JSON.stringify(evt)); 
		console.log(evt);
	};
	ws.onclose = function() {
		cache('blockAuth').attr('style', 'display: none;');
		cache('blockChat').attr('style', 'display: none;');
		cache('blockErr').removeAttr('style');
		cache('blockErr').text('SERVER CLOSED CONNECTION');
	}
	
	$('#authBtn').on('click', authFunc);
	$('#regBtn').on('click', regFunc);
});

function cache(key, value) {if (typeof value == 'undefined') { return cache[key]; }cache[key] = value;}

function authFunc(){
	if (cache('inputsAuth')[0].value.length >= 4 && cache('inputsAuth')[1].value.length >= 4) {
		var json = struct.CWLoginWithPass(cache('inputsAuth')[0].value, cache('inputsAuth')[1].value);
		ws.onmessage = function(evt){
			console.log(evt.data);
			if (evt.data == "LOGINED-SUCCSESS-ENTER-CHAT")
				enterChat();
			else {
				
			}
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
				enterChat();
		};
		ws.send(json);
	}
};

function enterChat(){
	var Login;
	var dialogSel = 'public';
	cache('blockAuth').attr('style', 'display: none;');
	cache('blockErr').attr('style', 'display: none;');
	
	var inputControl = {
		lock: false,
		keyD: function(key){
			if (key == 16) this.lock = true;
		},
		keyU: function(key){
			if (key == 16) this.lock = false;
		},
		send: function(ev){
			if (!this.lock) {
				ev.preventDefault();
				ws.send(struct.CWMessage(dialogSel, cache('textArea').html()));
			}
		},
		clear: function(){
			if (!this.lock) cache('textArea').html('');
		}
	};
	cache('textArea').keydown( function(ev){ inputControl.keyD(ev.which); if(ev.which == '13'){ inputControl.send(ev);} } )
		.keyup( function(ev){ inputControl.keyU(ev.which); if(ev.which == '13'){  inputControl.clear();} } );
	
	cache('ListDialog').append('<ul class="ChatBodyList2" data="public"></ul>');
	cacheD = [];
	cacheD['public'] = cache('ListDialog').children('[data="public"]');
	
	$('#ChatBlockContacts > div').on('click', '.ChatBTN', function() {
		cacheD[dialogSel].attr('style', 'display: none;');
		dialogSel = $(this).attr('data');
		if (cacheD[dialogSel] === undefined){
				cache('ListDialog').append('<ul class="ChatBodyList2" data="'+dialogSel+'"></ul>');
				cacheD[dialogSel] = cache('ListDialog').children('[data="'+dialogSel+'"]');
				ws.send(struct.CW('getm', dialogSel));
			}
		cacheD[dialogSel].removeAttr('style');
	});
	
	ws.onmessage = function(evt) {
		var json = JSON.parse(evt.data);
		if (json.type == 'dat') {
			json = JSON.parse(json.body);
			Login = json.login;
			if (json.online !== 'undefined')
				for(i = 0; i < json.online.length; i++)
					if (json.online[i] != Login) {
						cache('Contacts').append('<li class="ChatBTN" + data="' + json.online[i] + '">' + json.online[i] + '</li>');
					}
		}
		else if (json.type == 'mes') {
			json = JSON.parse(json.body);
			/*console.log(json);*/
			date = new Date(json.date);
			date = (('0' + date.getHours()).slice(-2) + ':' + ('0' + date.getMinutes()).slice(-2));
			if (json.to == 'public'){
				cacheD['public'].append('<li class="ChatBodyMessage"><div class="ChatBodyMessageLogin">' + json.from + ' ' + date +
					'</div><div class="ChatBodyMessageText">' + json.message + '</div></li>');
				cacheD['public'][0].scrollTop = cacheD['public'][0].scrollHeight;
			}
			else {
				let dialogID;
				if (json.from == Login) dialogID = json.to;
				else dialogID = json.from;
				if (!(cache[dialogID] !== undefined)) {
					cacheD[dialogID].append('<li class="ChatBodyMessage"><div class="ChatBodyMessageLogin">'+ json.from +' '+ date +
						'</div><div class="ChatBodyMessageText">'+ json.message +'</div></li>');
					cacheD[dialogID][0].scrollTop = cacheD[dialogID][0].scrollHeight;
				}
			}
		}
		else if (json.type == 'onl'){
			cache('Contacts').append('<li class="ChatBTN" + data="' + json.body + '">' + json.body + '</li>');
			/*if (cacheD[json.body] === undefined){
				cache('ListDialog').append('<ul class="ChatBodyList2" data="'+json.body+'" style="display:none;"></ul>');
				cacheD[json.body] = cache('ListDialog').children('[data="'+json.body+'"]');
			}*/
		}
		else if (json.type == 'off'){
			cache('Contacts').children('[data="'+json.body+'"]').remove();
		}
	};
	cache('blockChat').removeAttr('style');
};