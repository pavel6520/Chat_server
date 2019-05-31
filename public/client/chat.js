var body = $('body');
var chatcontent = $('#chatcontent');

var chatmodal = $(`<div class="modal fade" id="chatModal" tabindex="-1" role="dialog" aria-labelledby="chatModalLabel" aria-hidden="true"></div>`).prependTo(body);
var cmd = $('<div class="modal-dialog modal-lg" role="document"></div>').appendTo(chatmodal);
var cmdc = $('<div class="modal-content"></div>').appendTo(cmd);
var cmdch = $('<div class="modal-header"></div>').appendTo(cmdc);
var cmdcht = $('<h4 class="modal-title" id="chatModalLabel"></h4>').appendTo(cmdch);
var cmdchb = $('<button class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>').appendTo(cmdch);
var cmdcb = $('<div class="modal-body"></div>').appendTo(cmdc);
var cmdcb_dialogadd = $('<div data-modalblock="dialogadd" style="display: none;">').appendTo(cmdcb);
var cmdcb_dialogadd_form = $('<form><div class="form-group"><label for="inputDialogLogin">Login user</label>' +
    '<input type="login" class="form-control" id="inputDialogLogin" aria-describedby="loginHelp" placeholder="Enter login">' +
    '<small id="loginHelp" class="form-text text-muted">Enter user login for create dialog</small>' +
    '</div></form>').appendTo(cmdcb_dialogadd);
var cmdcb_dialogadd_button = $('<button type="submit" class="btn btn-primary">Invite</button>').appendTo(cmdcb_dialogadd);
var cmdcb_dialogadd_table = $('<table class="table table-bordered table-sm"></table>').appendTo(cmdcb_dialogadd);
var cmdcb_dialogadd_table_thead = $('<thead><tr><th>Login</th><th>Date</th><th style="width: 100px;">Accept</th></tr></thead>').appendTo(cmdcb_dialogadd_table);
var cmdcb_dialogadd_table_tbody = $('<tbody></tbody>').appendTo(cmdcb_dialogadd_table);
var cmdcb_roomadd = $('<div data-modalblock="roomadd" style="display: none;">').appendTo(cmdcb);
var cmdcb_roomadd_form = $('<form><div class="form-group"><label for="inputDialogLogin">Room</label>' +
    '<input type="login" class="form-control" id="inputDialogLogin" aria-describedby="loginHelp" placeholder="Enter id">' +
    '<small id="loginHelp" class="form-text text-muted">Enter room id for create</small>' +
    '</div></form>').appendTo(cmdcb_roomadd);
var cmdcb_roomadd_button = $('<button type="submit" class="btn btn-primary">Create</button>').appendTo(cmdcb_roomadd);
var cmdcf = $('<div class="modal-footer"></div>').appendTo(cmdc);
var cmdcfb = $('<button class="btn btn-primary" data-dismiss="modal">Close</button>').appendTo(cmdcf);

var chatleftmenu = $('<div id="chatleftmenu" class="navmenu navmenu-default navmenu-fixed-left offcanvas-sm bordersc"></div>').prependTo(body);
$('<style>@media(min-width: 800px){body {padding: 0 0 0 300px;}header.navbar>button{display:none;}}@media(min-width: 800px){.offcanvas-sm{display:block;}}</style>').prependTo(body);
var clmf = $('<div class="flex"></div>').appendTo(chatleftmenu);
var clmfnbs = $('<div class="btn-group mc-0" role="group"></div>').appendTo(clmf);
var clmfnbsr = $('<button class="btn btn-primary col-3">Rooms</button>').appendTo(clmfnbs);
var clmfnbsra = $('<button data-modalopen="roomadd" class="btn btn-primary col-1 p-0">+</button>').appendTo(clmfnbs);
var clmfnbsd = $('<button class="btn btn-secondary col-4">Dialogs</button>').appendTo(clmfnbs);
var clmfnbsda = $('<button data-modalopen="dialogadd" class="btn btn-secondary col-1 p-0">+</button>').appendTo(clmfnbs);
var clmfnbsp = $('<button class="btn btn-dark col-3">Public</button>').appendTo(clmfnbs);
var clmfcs = $('<ul class="nav navmenu-nav m-0" style="overflow:auto;"></ul>').appendTo(clmf);

var chatinputarea = $('<div id="chatinputarea" class="container-fluid row p-0 bordersc"></div>').insertAfter(chatcontent);
var ciad = $('<div class="inputarea col-11" contenteditable="true"></div>').appendTo(chatinputarea);
var ciab = $('<button class="btn col-1"><span class="glyphicon glyphicon-send"></span></button>').appendTo(chatinputarea);

var ccc = $('<div class="container-fluid p-0"></div>').appendTo(chatcontent);
var chat_mp = [];
var chat_d = {
    dialogs: [],
    invite: []
};
var chat_r = [];
var scrolledTop = true;

dateOptions = {
    year: 'numeric',
    month: 'numeric',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric'
};

function chatContentScroll(anyway = false) {
    if(anyway) {
        scrolledTop = true;
    }
    if (scrolledTop){
        chatcontent.animate({ scrollTop: ccc.outerHeight(true)}, 1000);
        //chatcontent.scrollTop(ccc.outerHeight(true) -  chatcontent.outerHeight(true));
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
        if (key === 16) this.lock = true;
    },
    keyU: function(key){
        if (key === 16) this.lock = false;
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
            }
        }
    },
    clear: function(){
        if (!this.lock) {
            ciad.html('');
        }
    }
};
var tmp;
function addMessage(arr, type, messages){
    for(var i = 0; i < messages.length; i++){
        messages[i].dt = longToDate(messages[i].id);
        if(arr[messages[i].id] === undefined){
            arr[messages[i].id] = [];
        }
        arr[messages[i].id][messages[i].idh] = messages[i];

    }

    for (var id in arr){
        for (var idh in arr[id]){
            if(!arr[id][idh].o){
                arr[id][idh].o = $('<table data-id="'+arr[id][idh].id+'" data-idh="'+arr[id][idh].idh+'" class="message m-0"><tbody><tr><td valign="top">' +
                    '<div style="background:url(\'/client/images/defavatar64.png\') no-repeat center center;" class="ava"></div>' +
                    '</td><td class="body" style="font-size: 14px;"><table class="w-100"><tbody><tr><td style="padding-right:16px;">' +
                    '<div class="nick">'+arr[id][idh].login+'</div></td><td class="vv"><div class="sdt">' +
                    //'<img src="https://chat-off.com/images/vv.png" class="vvi">  ' +
                    arr[id][idh].dt.toLocaleString("ru", dateOptions)+'</div>' +
                    //'<div class="sfd"><img src="/client/images/close.png" title="удалить" class="fd"></div>' +
                    '</td></tr></tbody></table><div class="text">'+arr[id][idh].message+'</div></td></tr></tbody></table>').appendTo(ccc);
            }

        }
    }
    chatContentScroll();
}

function addInvite(invite) {$('<tr><td>'+invite.login+'</td><td>'+longToDate(invite.dc).toLocaleString("ru", dateOptions)+'</td><td><button class="btn btn-sm btn-primary">Accept</button></td></tr>').appendTo(cmdcb_dialogadd_table_tbody);}

function connect(){
    ws = new WebSocket('wss://'+address+'/');
    ws.onopen = function(evt){
        loadMessage('public', 'get');
        ciad.keydown( function(ev){
            inputControl.keyD(ev.which);
            if(ev.which === 13){
                inputControl.send(ev);
            }
        } ).keyup( function(ev){
            inputControl.keyU(ev.which);
            if(ev.which === 13){
                inputControl.clear();
            }
        });
        ciab.on('click', function () {
            inputControl.send();
            inputControl.clear();
        });
        let tmp = {};
        tmp.type = 'dialog';
        tmp.act = 'list';
        sendAjaxJson('/chat', tmp, function (r) {
            if (r.state){
                for (var i in r.invite) {
                    chat_d.invite.push(r.invite[i]);
                    addInvite(r.invite[i]);
                }
            }
        });
        chatContentScroll(true);
    };
    ws.onmessage = function(evt) {
        var json = JSON.parse(evt.data);
        switch (json.type) {
            case "public":{
                switch (json.act) {
                    case "add":{
                        loadMessage(json.type, 'getone', 1, json.id, json.idh);
                        break;
                    }
                }
                break;
            }
            case "dialog":{
                switch (json.act) {
                    case "create":{
                        let o = {login: json.login, dc: longToDate(json.datecreate)};
                        chat_d.invite.push(o);
                        addInvite(o);
                        //$('<tr><td>'+o.login+'</td><td>'+o.dc.toLocaleString("ru", dateOptions)+'</td><td><button class="btn btn-sm btn-primary">Accept</button></td></tr>').appendTo(cmdcb_dialogadd_table_tbody);
                        break;
                    }
                    case "add":{
                        loadMessage(json.type, 'getone', 1, json.id, json.idh);
                        break;
                    }
                }
                break;
            }
        }
        if(json.type === 'public') {

        }
        else if(json.type === 'dialog'){

        }
    };
    ws.onclose = function(evt) {
        //console.log('wsclosed');
        //console.log(evt);
    };
    ws.onerror = function(evt) {
        //console.log('wserror');
        //console.log(evt);
    };
}

loadMessage = undefined;

$(document).ready(function () {
    chatcontent.on('scroll', function () {
        scrolledTop = chatcontent.scrollTop() === ccc.outerHeight(true) - chatcontent.outerHeight(true);
    });
    cmdcb_dialogadd_table_tbody.on('click', 'button.btn', function () {
        let o = $(this).parent().parent();
        let tmp = {type: 'dialog', act: 'accept', login: o.children('td:first').text()}
        sendAjaxJson('/chat', tmp, function (d) {
            o.remove();
        });
    });
    loadMessage = function(type, act, count = 50, id, idh, did, did2){
        let tmp = {};
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
            if(Array.isArray(d)){
                addMessage(chat_mp, 'public', d);
            }
        });
    };
    cmdcb_dialogadd_button.on('click', function () {
        let tmp = {};
        tmp.type = 'dialog';
        tmp.act = 'create';
        tmp.login = cmdcb_dialogadd_form.find('input[type="login"]').val();

        sendAjaxJson('/chat', tmp, function(d){
            //console.log(d);
        });
    });
    $('button[data-modalopen]').on('click', function () {
        let modal = $(this).attr('data-modalopen');
        switch (modal) {
            case 'dialogadd':
                cmdcht.html('Add new dialog');
                cmdcb_dialogadd_table_tbody.empty();
                for (var i in chat_d.invite) {
                    addInvite(chat_d.invite[i]);
                }
                break;
            case 'roomadd':
                cmdcht.html('Create new room');
                break;
        }
        $('.modal-body [data-modalblock]').css('display', 'none');
        $('.modal-body [data-modalblock="'+modal+'"]').removeAttr('style');
        chatmodal.modal('show');
    });
    connect();
});