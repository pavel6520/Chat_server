
var body = $('body');
var chatcontent = $('#chatcontent');
var chatleftmenu = $('<div id="chatleftmenu" class="navmenu navmenu-default navmenu-fixed-left offcanvas-sm bordersc"></div>').prependTo(body);
$('<style>@media(min-width: 800px){body {padding: 0 0 0 300px;}header.navbar>button{display:none;}}@media(min-width: 800px){.offcanvas-sm{display:block;}}</style>').prependTo(body);
var clmf = $('<div class="flex"></div>').appendTo(chatleftmenu);
var clmfnbs = $('<div class="btn-group mc-0" role="group"></div>').appendTo(clmf);
var clmfnbsDialogs = $('<button type="button" class="btn btn-primary col-3">Dialogs</button>').appendTo(clmfnbs);
var clmfnbsDialogsAdd = $('<button type="button" class="btn btn-primary col-1 p-0">+</button>').appendTo(clmfnbs);
var clmfnbsRooms = $('<button type="button" class="btn btn-secondary col-3">Rooms</button>').appendTo(clmfnbs);
var clmfnbsRoomsAdd = $('<button type="button" class="btn btn-secondary col-1 p-0">+</button>').appendTo(clmfnbs);
var clmfnbsPublic = $('<button type="button" class="btn btn-dark col-4">Public</button>').appendTo(clmfnbs);
var clmfcs = $('<ul class="nav navmenu-nav m-0" style="overflow:auto;"></ul>').appendTo(clmf);

var chatinputarea = $('<div id="chatinputarea" class="container-fluid row p-0 bordersc"></div>').insertAfter(chatcontent);
var ciad = $('<div class="inputarea col-11"></div>').appendTo(chatinputarea);
var cib = $('<button class="btn col-1"><span class="glyphicon glyphicon-send"></span></button>').appendTo(chatinputarea);

var ccc = $('<div class="container-fluid p-0"></div>').appendTo(chatcontent);
chat_mp = [];
chatd = [];
chatr = [];
scrolledTop = true;

function chatContentScroll() {
    if (scrolledTop){
        chatcontent.scrollTop(ccc.outerHeight(true) -  chatcontent.outerHeight(true));
    }
}


function sendAjaxJson(url, obj, success) {
    jqxhr = $.ajax({
        type: 'POST',
        processData: false,
        contetnType: false,
        cache: false,
        url: url,
        data: JSON.stringify(obj),
        success: success,
        // error: AjaxErrorRequest
    });
}

var inputControl = {
    lock: false,
    keyD: function(key){
        if (key == 16) this.lock = true;
    },
    keyU: function(key){
        if (key == 16) this.lock = false;
    },
    send: function(ev){
        var tmpText = ciad.html();
        if (tmpText.length > 0) {
            if (tmpText.length > 1000) {
                tmpText = tmpText.substr(0, 1000);
            }
            if (ev) {
                if (!this.lock) {
                    ev.preventDefault();
                    sendAjaxJson('/chat', {type: 'public', act: 'add', message:tmpText});
                }
            } else {
                sendAjaxJson('/chat', {type: 'public', act: 'add', message: tmpText});
                //ws.send(struct.CWMessage(JSON.stringify({type: 'public', act: 'add', message: ciad.html()})));
            }
        }
    },
    clear: function(){
        ciad.html('');
    }
};

function addMessage(type, messages){
    /*for(var i = 0; i < messages.length; i++){
        messages[i].timemesJS = new Date(messages[i].timemes);
        console.log(new Date(messages[i].timemes).valueOf());
        var id = ((messages[i].timemesJS.valueOf()) << 8) + messages[i].helpid;
        console.log(id);
        chatp[id] = messages[i];
    }
    console.log(chatp);
    chatp.forEach(function (item, i, arr){
        console.log(item);
    });*/
    for(var i = 0; i < messages.length; i++){
        let id = new Date(messages[i].id).valueOf();
        if(chat_mp[id] === undefined){
            chat_mp[id] = [];
        }
        chat_mp[id][messages[i].idh] = messages[i];
        let tmp = ccc.children('[data-id="'+id+'"][data-idh="'+messages[i].idh+'"]');
        if(tmp.length == 0){
            ccc.append('<div data-id="'+id+'" data-idh="'+messages[i].idh+'" class="row message m-0"><div>'+messages[i].login+' '+messages[i].id+'</div><div class="col-12 messagebody">'+messages[i].message+'</div></div>');
        }
    }
    chatContentScroll();
    //console.log(chatp);
}

loadMessage = undefined;

$(document).ready(function () {
    chatcontent.on('scroll', function () {
        if(chatcontent.scrollTop() === ccc.outerHeight(true) -  chatcontent.outerHeight(true)){
            scrolledTop = true;
        }
        else {
            scrolledTop = false;
        }
    });
    loadMessage = function(type, act, count = 50, id, idh, did, did2){
        var tmp = {};
        tmp.type = type;
        tmp.act = act;
        tmp.count = count;
        tmp.id = id;
        tmp.idh = idh;
        tmp.up = true;
        if(type === 'room'){
            tmp.room = did;
        }
        if(type === 'dialog'){
            tmp.sender = did;
            tmp.recepient = did2;
        }

        sendAjaxJson('/chat', tmp, function(d){
            //console.log(d);
            if(Array.isArray(d)){
                addMessage('public', d);
            }
        });
    };

    // ccc.html('<div class="row message m-0"><div>login: </div><div class="col-12 messagebody">test test 123</div></div>' +
    //    '<div class="row message m-0"><div>login: </div><div class="col-12 messagebody">test test 123</div></div>' +
    //    '<div class="row message m-0"><div>login: </div><div class="col-12 messagebody">test test 123</div></div>' +
    //    '<div class="row message m-0"><div>login: </div><div class="col-12 messagebody">test test 123</div></div>');
    ws = new WebSocket('wss://'+address+'/');
    ws.onopen = function(evt){
        loadMessage('public', 'get');
        ciad.keydown( function(ev){
            inputControl.keyD(ev.which);
            if(ev.which === 13){
                inputControl.send(ev);}
        } ).keyup( function(ev){
            inputControl.keyU(ev.which);
            if(ev.which === 13){
                inputControl.clear();
            }
        });
        cib.on('click', function () {
            inputControl.send();
            inputControl.clear();
        });
    };
    ws.onmessage = function(evt) {
        var json = JSON.parse(evt.data);
        loadMessage('public', 'getone', 1, json.id, json.idh);
    };
    ws.onclose = function(evt) {
    	console.log('wsclosed');
        console.log(evt);
    };
    ws.onerror = function(evt) {
        console.log('wserror');
    	console.log(evt);
    };
});