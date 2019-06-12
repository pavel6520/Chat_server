var C_c1 = 'public', C_c2 = 'dialog', C_c3 = 'room';
var body, chatcontent,
    chatmodal, cmd, cmdc, cmdch, cmdcht, cmdchb, cmdcb,
    cmdcb_dialogadd, cmdcb_dialogadd_form, cmdcb_dialogadd_button, cmdcb_dialogadd_table, cmdcb_dialogadd_table_thead, cmdcb_dialogadd_table_tbody,
    cmdcb_roomadd, cmdcb_roomadd_form1, cmdcb_roomadd_button1, cmdcb_roomadd_form2, cmdcb_roomadd_button2,
    cmdcf, cmdcfb,
    chatleftmenu, clmf, clmfnbs, clmfnbsr, clmfnbsra, clmfnbsd, clmfnbsda, clmfnbsp, clmfcs,
    ccc,
    chatinputarea, ciad, ciab;

dateOptions = {
    year: 'numeric',
    month: 'numeric',
    day: 'numeric',
    hour: 'numeric',
    minute: 'numeric'
};

function sendAjaxJson(url, obj, success, error) {
    jqxhr = $.ajax({
        method: 'POST',
        processData: false,
        contentType: false,
        cache: false,
        url: url,
        data: JSON.stringify(obj),
        success: success,
        error: error
    });
}
class ChatMessageClass{
    constructor(id, idh){
        this.showed = false;
        this.id = id;
        this.idh = idh;
    }
    load(type, chatid, chatch){
        this.type = type;
        this.chatid = chatid;
        this.chatch = chatch;
        let tmp = {type: type, act: 'getone', id: this.id, idh: this.idh};
        if(type === C_c2){
            tmp.chatid = chatid;
        }
        if(type === C_c3){
            tmp.chatid = chatid;
            tmp.chatch = chatch;
        }
        let o = this;
        sendAjaxJson('/chat', tmp, function(r){
            if(r.state === true) {
                o.login = r.o.login;
                o.message = r.o.message.replace(new RegExp('\n', 'g'), '<br>');
                o.dt = longToDate(o.id);
                o.o = $('<table data-id="' + o.id + '" data-idh="' + o.idh + '" class="message m-0"><tbody><tr><td valign="top"><div style="background:url(\'/client/images/defavatar64.png\') no-repeat center center;" class="ava"></div>' +
                    '</td><td class="body" style="font-size: 14px;"><table class="w-100"><tbody><tr><td style="padding-right:16px;"><div class="nick">' + o.login + '</div></td><td class="vv"><div class="sdt">' +
                    /*'<img src="https://chat-off.com/images/vv.png" class="vvi">  ' +*/ o.dt.toLocaleString("ru", dateOptions) + '</div>' +
                    /*'<div class="sfd"><img src="/client/images/close.png" title="удалить" class="fd"></div>' +*/ '</td></tr></tbody></table><div class="text">' + o.message + '</div></td></tr></tbody></table>');
                if (chat.chattype === o.type) {
                    switch (o.type) {
                        case C_c1:
                            o.show();
                            break;
                        case C_c2:
                            if(chat.chatid === o.chatid){
                                o.show();
                            }
                            break;
                        case C_c3:
                            if(chat.chatid === o.chatid && chat.chatchannel === o.chatch){
                                o.show();
                            }
                            break;
                    }
                    chat.chatScroll();
                }
            }
        });
    }
    init(login, message){
        this.login = login;
        this.message = message.replace(new RegExp('\n', 'g'),'<br>');
        this.dt = longToDate(this.id);
        this.o = $('<table data-id="'+this.id+'" data-idh="'+this.idh+'" class="message m-0"><tbody><tr><td valign="top"><div style="background:url(\'/client/images/defavatar64.png\') no-repeat center center;" class="ava"></div>' +
            '</td><td class="body" style="font-size: 14px;"><table class="w-100"><tbody><tr><td style="padding-right:16px;"><div class="nick">'+this.login+'</div></td><td class="vv"><div class="sdt">' +
            /*'<img src="https://chat-off.com/images/vv.png" class="vvi">  ' +*/ this.dt.toLocaleString("ru", dateOptions)+'</div>' +
            /*'<div class="sfd"><img src="/client/images/close.png" title="удалить" class="fd"></div>' +*/ '</td></tr></tbody></table><div class="text">'+this.message+'</div></td></tr></tbody></table>');
    }
    del(){ this.o.remove(); }
    hide(){ if(this.showed) {this.o.detach(); this.showed = false;} }
    show(){ if(!this.showed) {this.o.appendTo(ccc); this.showed = true;} }
}
function messagesShow(arr) {for (let i1 in arr) for (let i2 in arr[i1]) arr[i1][i2].show();}
function messagesHide(arr) {for (let i1 in arr) for (let i2 in arr[i1]) arr[i1][i2].hide();}
class ChatDialogInviteClass{
    constructor(login, dc){
        this.showed = false;
        this.login = login;
        this.dc = dc;
        this.o = $('<tr><td>'+this.login+'</td><td>'+longToDate(this.dc).toLocaleString("ru", dateOptions)+'</td><td><button class="btn btn-sm btn-primary">Accept</button></td></tr>');
    }
    del(){ this.o.remove(); }
    hide(){ if(this.showed) {this.o.detach(); this.showed = false;} }
    show(){ if(!this.showed) {this.o.appendTo(cmdcb_dialogadd_table_tbody); this.showed = true;} }
}
class ChatDialogClass{
    constructor(login){
        this.showed = false;
        this.login = login;
        this.list = [];
        this.load = false;
        this.o = $('<li class="col-12 p-0"><button data-dial="'+login+'" class="btn btn-sm btn-light col">'+login+'</button></li>');
    }
    del(){ this.o.remove(); }
    hide(){ if(this.showed) {this.o.detach(); this.showed = false;} }
    show(){ if(!this.showed) {this.o.prependTo(clmfcs); this.showed = true;} }
}
class ChatRoomClass{
    constructor(obj){
        this.showed = false;
        this.id = obj.id;
        this.name = obj.name;
        this.dc = obj.dc;
        this.owner = obj.owner;
        this.listch = [];
        this.o = $('<li class="col-12 p-0"><button data-room="'+this.id+'" class="btn btn-sm btn-light col">'+this.id+'</button></li>');
    }
    del(){ this.o.remove(); }
    hide(){ if(this.showed) {this.o.detach(); this.showed = false;} }
    show(){ if(!this.showed) {this.o.prependTo(clmfcs); this.showed = true;} }
}
class ChatChannelClass{
    constructor(obj){
        this.showed = false;
        this.id = obj.id;
        this.dc = obj.dc;
        this.list = [];
        this.load = false;
        this.o = $('<li class="col-12 p-0"><button data-roomch="'+this.id+'" class="btn btn-sm btn-light col">'+this.id+'</button></li>');
    }
    del(){ this.o.remove(); }
    hide(){ if(this.showed) {this.o.detach(); this.showed = false;} }
    show(){ if(!this.showed) {this.o.prependTo(clmfcs); this.showed = true;} }
}
class ChatClass{
    constructor(){
        body = $('body');
        chatcontent = $('#chatcontent');
        chatmodal = $(`<div class="modal fade" id="chatModal" tabindex="-1" role="dialog" aria-labelledby="chatModalLabel" aria-hidden="true"></div>`).prependTo(body);
        cmd = $('<div class="modal-dialog modal-lg" role="document"></div>').appendTo(chatmodal);
        cmdc = $('<div class="modal-content"></div>').appendTo(cmd);
        cmdch = $('<div class="modal-header"></div>').appendTo(cmdc);
        cmdch = $('<div class="modal-header"></div>').appendTo(cmdc);
        cmdcht = $('<h4 class="modal-title" id="chatModalLabel"></h4>').appendTo(cmdch);
        cmdchb = $('<button class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>').appendTo(cmdch);
        cmdcb = $('<div class="modal-body"></div>').appendTo(cmdc);
        cmdcb_dialogadd = $('<div data-modalblock="dialogadd" style="display: none;">').appendTo(cmdcb);
        cmdcb_dialogadd_form = $('<form><div class="form-group"><label for="inputDialogLogin">Login user</label><input type="text" class="form-control" id="inputDialogLogin" placeholder="Enter login">' +
            '<small id="loginHelp" class="form-text text-muted">Enter user login for create dialog</small></div></form>').appendTo(cmdcb_dialogadd);
        cmdcb_dialogadd_button = $('<button type="submit" class="btn btn-primary">Invite</button>').appendTo(cmdcb_dialogadd);
        cmdcb_dialogadd_table = $('<table class="table table-bordered table-sm"></table>').appendTo(cmdcb_dialogadd);
        cmdcb_dialogadd_table_thead = $('<thead><tr><th>Login</th><th>Date</th><th style="width: 100px;">Accept</th></tr></thead>').appendTo(cmdcb_dialogadd_table);
        cmdcb_dialogadd_table_tbody = $('<tbody></tbody>').appendTo(cmdcb_dialogadd_table);
        cmdcb_roomadd = $('<div data-modalblock="roomadd" style="display: none;">').appendTo(cmdcb);
        cmdcb_roomadd_form1 = $('<form><div class="form-group"><label for="inputRoomId">Room id</label><input type="text" data-t="id" class="form-control" id="inputRoomId" placeholder="Enter id" /><label for="inputRoomName">Room name</label>' +
            '<input type="text" data-t="name" class="form-control" id="inputRoomName" placeholder="Enter name" /><small id="loginHelp" class="form-text text-muted">Enter room id for create</small></div></form>').appendTo(cmdcb_roomadd);
        cmdcb_roomadd_button1 = $('<button type="submit" class="btn btn-primary">Create</button>').appendTo(cmdcb_roomadd);
        cmdcb_roomadd_form2 = $('<form><div class="form-group"><label for="inputRoomId">Room id</label><input type="text" class="form-control" id="inputRoomId" placeholder="Enter id" />' +
            '<small id="loginHelp" class="form-text text-muted">Enter room id for create</small></div></form>').appendTo(cmdcb_roomadd);
        cmdcb_roomadd_button2 = $('<button type="submit" class="btn btn-primary">Create</button>').appendTo(cmdcb_roomadd);
        cmdcf = $('<div class="modal-footer"></div>').appendTo(cmdc);
        cmdcfb = $('<button class="btn btn-primary" data-dismiss="modal">Close</button>').appendTo(cmdcf);
        chatleftmenu = $('<div id="chatleftmenu" class="navmenu navmenu-default navmenu-fixed-left offcanvas-sm bordersc"></div>').prependTo(body);
        $('<style>@media(min-width: 800px){body {padding: 0 0 0 300px;}header.navbar>button{display:none;}}@media(min-width: 800px){.offcanvas-sm{display:block;}}</style>').prependTo(body);
        clmf = $('<div class="flex"></div>').appendTo(chatleftmenu);
        clmfnbs = $('<div class="btn-group mc-0" role="group"></div>').appendTo(clmf);
        clmfnbsr = $('<button data-typesel="room" class="btn btn-primary col-3">Rooms</button>').appendTo(clmfnbs);
        clmfnbsra = $('<button data-modalopen="roomadd" class="btn btn-primary col-1 p-0">+</button>').appendTo(clmfnbs);
        clmfnbsd = $('<button data-typesel="dialog" class="btn btn-secondary col-4">Dialogs</button>').appendTo(clmfnbs);
        clmfnbsda = $('<button data-modalopen="dialogadd" class="btn btn-secondary col-1 p-0">+</button>').appendTo(clmfnbs);
        clmfnbsp = $('<button data-typesel="public" class="btn btn-dark col-3">Public</button>').appendTo(clmfnbs);
        clmfcs = $('<ul class="nav navmenu-nav m-0" style="overflow:auto;"></ul>').appendTo(clmf);
        ccc = $('<div class="container-fluid p-0"></div>').appendTo(chatcontent);
        chatinputarea = $('<div id="chatinputarea" class="container-fluid row p-0 bordersc"></div>').insertAfter(chatcontent);
        ciad = $('<div class="inputarea col-11" contenteditable="true"></div>').appendTo(chatinputarea);
        ciab = $('<button class="btn col-1"><span class="glyphicon glyphicon-send"></span>Send</button>').appendTo(chatinputarea);


        chatcontent.on('scroll', function () {
            chat.scroll = chatcontent.scrollTop() === ccc.outerHeight(true) - chatcontent.outerHeight(true);
        });
        cmdcb_dialogadd_table_tbody.on('click', 'button.btn', function () {
            let o = $(this).parent().parent();
            let tmp = {type: C_c2, act: 'accept', login: o.children('td:first').text()};
            sendAjaxJson('/chat', tmp, function (d) { o.remove();
                chat.dialog.dialogs.list[tmp.login] = new ChatDialogClass(tmp.login);
                if(chat.chattype === tmp.type){
                    chat.dialog.dialogs.list[tmp.login].show();
                }});
        });
        cmdcb_dialogadd_button.on('click', function () {
            let tmp = {type: C_c2, act: 'create'};
            tmp.login = cmdcb_dialogadd_form.find('input[type="text"]').val();
            sendAjaxJson('/chat', tmp, function(d){/*console.log(d);*/});
        });
        cmdcb_roomadd_button1.on('click', function () {
            let tmp = {type: C_c3, act: 'create'};
            tmp.id = cmdcb_roomadd_form1.find('input[type="text"][data-t="id"]').val();
            tmp.name = cmdcb_roomadd_form1.find('input[type="text"][data-t="name"]').val();
            sendAjaxJson('/chat', tmp, function(r){
                if(r.state && chat.room.load === true) {
                    tmp.dc = r.dc;
                    chat.room.list[tmp.id] = new ChatRoomClass(tmp);
                    chat.room.list[tmp.id].listch['main'] = new ChatChannelClass({id: 'main', dc: r.dc});
                    chat.room.list[tmp.id].listch['main'].load = true;
                    if(chat.chattype === C_c3 && chat.chatid === null){
                        chat.room.list[tmp.id].show();
                    }
                }
            });
        });
        cmdcb_roomadd_button2.on('click', function () {
            let tmp = {type: C_c3, act: 'join'};
            tmp.id = cmdcb_roomadd_form2.find('input[type="text"]').val();
            sendAjaxJson('/chat', tmp, function(r){
                if(r.state && chat.room.load === true) {
                    chat.room.list[r.room.id] = new ChatRoomClass(r.room);
                    for(let i in r.listch) {
                        chat.room.list[tmp.id].listch[r.listch[i].id] = new ChatChannelClass(r.listch[i]);
                    }
                    if (chat.chattype === C_c3 && chat.chatid === null) {
                        chat.room.list[r.room.id].show();
                    }
                }
            });
        });
        $('button[data-modalopen]').on('click', function () {
            let modal = $(this).attr('data-modalopen');
            switch (modal) {
                case 'dialogadd':
                    cmdcht.html('Add new dialog');
                    if(chat.dialog.invite.load === false){
                        var tmp = {};
                        tmp.type = C_c2;
                        tmp.act = 'invite';
                        sendAjaxJson('/chat', tmp, function (r) {
                            if (r.state){
                                for (var i in r.invite) {
                                    let tmp = r.invite[i];
                                    if (!chat.dialog.invite.list[tmp.login]) {
                                        chat.dialog.invite.list[tmp.login] = new ChatDialogInviteClass(tmp.login, longToDate(tmp.dc));
                                        chat.dialog.invite.list[tmp.login].show();
                                    }
                                }
                            }
                        });
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
        $('button[data-typesel]').on('click', function () {
            let typesel = $(this).attr('data-typesel');
            if (typesel !== chat.chattype) {
                switch (chat.chattype) {
                    case C_c1:
                        messagesHide(chat.public.list);
                        break;
                    case C_c2:
                        for (let i in chat.dialog.dialogs.list) {
                            chat.dialog.dialogs.list[i].hide();
                        }
                        if(chat.chatid !== null) {
                            messagesHide(chat.dialog.dialogs.list[chat.chatid].list);
                            chat.chatid = null;
                        }
                        break;
                    case C_c3:
                        if (chat.chatchannel !== null){
                            messagesHide(chat.room.list[chat.chatid].listch[chat.chatchannel].list);
                            for (let i in chat.room.list[chat.chatid].listch) {
                                chat.room.list[chat.chatid].listch[i].hide();
                            }
                            chat.chatchannel = null;
                            chat.chatid = null;
                        }
                        else if (chat.chatid !== null) {
                            for (let i in chat.room.list[chat.chatid].listch) {
                                chat.room.list[chat.chatid].listch[i].hide();
                            }
                            chat.chatid = null;
                        }
                        else {
                            for (let i in chat.room.list) {
                                chat.room.list[i].hide();
                            }
                        }
                        break;
                }
                switch (typesel) {
                    case C_c1:
                        messagesShow(chat.public.list);
                        chat.scroll = true;
                        chat.chatScroll(0);
                        break;
                    case C_c2:
                        if(chat.dialog.dialogs.load === false){
                            let tmp = {type: typesel, act: 'list'};
                            sendAjaxJson('/chat', tmp, function (r) {
                                if (r.state){
                                    for (let i in r.list) {
                                        let tmp = r.list[i];
                                        if (chat.dialog.dialogs.list[tmp.login] === undefined) {
                                            chat.dialog.dialogs.list[tmp.login] = new ChatDialogClass(tmp.login);
                                            chat.dialog.dialogs.list[tmp.login].show();
                                        }
                                    }
                                }
                            });
                            chat.dialog.dialogs.load = true;
                        }
                        for (let i in chat.dialog.dialogs.list) {
                            chat.dialog.dialogs.list[i].show();
                        }
                        break;
                    case C_c3:
                        if(chat.room.load === false){
                            let tmp = {type: typesel, act: 'list'};
                            sendAjaxJson('/chat', tmp, function (r) {
                                if (r.state){
                                    for (let i in r.list) {
                                        if (chat.room.list[r.list[i].id] === undefined) {
                                            chat.room.list[r.list[i].id] = new ChatRoomClass(r.list[i]);
                                            chat.room.list[r.list[i].id].show();
                                        }
                                    }
                                    for (let i in r.listch) {
                                        chat.room.list[r.listch[i].idr].listch[r.listch[i].id] = new ChatChannelClass(r.listch[i]);
                                    }
                                }
                            });
                            chat.room.load = true;
                        }
                        for (let i in chat.room.list) {
                            chat.room.list[i].show();
                        }
                        break;
                }
                chat.chattype = typesel;
            }
            else{
                switch (chat.chattype) {
                    case C_c1:
                        chat.chatScroll(0);
                        break;
                    case C_c2:
                        if(chat.chatid !== null) {
                            messagesHide(chat.dialog.dialogs.list[chat.chatid].list);
                            chat.chatid = null;
                        }
                        break;
                    case C_c3:
                        if (chat.chatchannel !== null){
                            messagesHide(chat.room.list[chat.chatid].listch[chat.chatchannel].list);
                            chat.chatchannel = null;
                        }
                        if (chat.chatid !== null) {
                            for (let i in chat.room.list[chat.chatid].listch) {
                                chat.room.list[chat.chatid].listch[i].hide();
                            }
                            chat.chatid = null;
                            for (let i in chat.room.list) {
                                chat.room.list[i].show();
                            }
                        }
                        break;
                }
            }
        });
        clmfcs.on('click', 'button[data-dial]', function () {
            chat.chatid = $(this).attr('data-dial');
            if (chat.dialog.dialogs.list[chat.chatid].load === false) {
                chat.loadMessage(chat.chattype, 50, chat.chatid, null);
                chat.dialog.dialogs.list[chat.chatid].load = true;
            }
            else{
                messagesShow(chat.dialog.dialogs.list[chat.chatid].list);
            }
        });
        clmfcs.on('click', 'button[data-room]', function () {
            chat.chatid = $(this).attr('data-room');
            for (let i in chat.room.list) {
                chat.room.list[i].hide();
            }
            for (let i in chat.room.list[chat.chatid].listch) {
                chat.room.list[chat.chatid].listch[i].show();
            }
        });
        clmfcs.on('click', 'button[data-roomch]', function () {
            chat.chatchannel = $(this).attr('data-roomch');
            if (chat.room.list[chat.chatid].listch[chat.chatchannel].load === false) {
                chat.loadMessage(chat.chattype, 50, chat.chatid, chat.chatchannel);
                chat.room.list[chat.chatid].listch[chat.chatchannel].load = true;
            }
            else{
                messagesShow(chat.room.list[chat.chatid].listch[chat.chatchannel].list);
            }
        });
        ciab.on('click', function () {chat.ciadsend();});
        this.inited = false;
    }
    init(){
        if (!this.inted) {
            chat.inited = true;
            ciad.keydown(chat.ciadown).keyup(chat.ciaup);
            ciab.on('click', chat.ciadsend);
        }
    }
    ciadown(evt){
        if (evt.which === 16 && chat.sendlock === false) chat.sendlock = true;
        if (evt.which === 13 && chat.sendlock === false && chat.enterlock === false) {
            chat.ciadsend();
            chat.enterlock = true;
            return false;
        }
    }
    ciaup(evt){
        if (evt.which === 16 && chat.sendlock === true) chat.sendlock = false;
        if (evt.which === 13 && chat.sendlock === false && chat.enterlock === true) {
            chat.enterlock = false;
            return false;
        }
    }
    ciadsend(){
        let text = ciad.html().trim();
        if(text.length > 0) {
            let tmp = {type: this.chattype, act: 'add', message: text};
            if (this.chattype === C_c1) {
                sendAjaxJson('/chat', tmp, function (r) {if (r.state === true) ciad.empty();});
            }
            if (this.chattype === C_c2 && chat.chatid !== null) {
                tmp.id = this.chatid;
                sendAjaxJson('/chat', tmp, function (r) {if (r.state === true) ciad.empty();});
            }
            if (this.chattype === C_c3 && chat.chatchannel !== null) {
                tmp.id = this.chatid;
                tmp.ch = this.chatchannel;
                sendAjaxJson('/chat', tmp, function (r) {if (r.state === true) ciad.empty();});
            }
        }
    }
    set(){
        this.init();
        this.connected = false;
        this.chattype = C_c1;
        this.chatid = null;
        this.chatchannel = null;
        this.sendlock = false;
        this.enterlock = false;
        this.scroll = true;
    }
    reset(){
        this.set();
    }
    connect(){
        this.ws = new WebSocket('wss://'+address+'/');
        this.ws.onopen = chat.load;
        this.ws.onmessage = chat.onWSmessage;
        this.ws.onclose = function(evt) {
            //console.log('wsclosed');
            //console.log(evt);
        };
        this.ws.onerror = function(evt) {
            //console.log('wserror');
            //console.log(evt);
        };
    }
    load(){
        chat.public = {load: false, list: []};
        chat.dialog = {
            invite:{load: false, list: []},
            dialogs:{load: false, list: []}
        };
        chat.room = {load: false, list: []};
        chat.connected = true;

        chat.loadMessage(chat.chattype, 50, null, null);
    }
    onWSmessage(evt){
        var json = JSON.parse(evt.data);
        switch (json.type) {
            case C_c1:{
                switch (json.act) {
                    case "add":{
                        if (chat.public.list[json.id] === undefined) {
                            chat.public.list[json.id] = [];
                        }
                        chat.public.list[json.id][json.idh] = new ChatMessageClass(json.id, json.idh);
                        chat.public.list[json.id][json.idh].load(json.type);
                        break;
                    }
                    case "del":{
                        if (chat.public.list[json.id][json.idh]){
                            chat.public.list[json.id][json.idh].o.remove();
                            delete chat.public.list[json.id][json.idh];
                            if (chat.public.list[json.id].length === 0){
                                delete chat.public.list[json.id];
                            }
                        }
                        break;
                    }
                }
                break;
            }
            case C_c2:{
                switch (json.act) {
                    case "create":{
                        if(!chat.dialog.invite.list[json.login]) {
                            chat.dialog.invite.list[json.login] = new ChatDialogInviteClass(json.login, longToDate(json.datecreate));
                            chat.dialog.invite.list[json.login].show();
                        }
                        break;
                    }
                    case "accept":{
                        chat.dialog.dialogs.list[json.login] = new ChatDialogClass(json.login);
                        if(chat.chattype === json.type){
                            chat.dialog.dialogs.list[json.login].show();
                        }
                        break;
                    }
                    case "add":{
                        if (json.cid1 === login)
                            json.chatid = json.cid2;
                        else
                            json.chatid = json.cid1;
                        if(chat.dialog.dialogs.list[json.chatid] !== undefined) {
                            if (chat.dialog.dialogs.list[json.chatid].list[json.id] === undefined) {
                                chat.dialog.dialogs.list[json.chatid].list[json.id] = [];
                            }
                            chat.dialog.dialogs.list[json.chatid].list[json.id][json.idh] = new ChatMessageClass(json.id, json.idh);
                            chat.dialog.dialogs.list[json.chatid].list[json.id][json.idh].load(json.type, json.chatid);
                        }
                        break;
                    }
                }
                break;
            }
            case C_c3:{
                switch (json.act) {
                    /*case "create":{
                        if(!chat.dialog.invite.list[json.login]) {
                            chat.dialog.invite.list[json.login] = new ChatDialogInviteClass(json.login, longToDate(json.datecreate));
                            chat.dialog.invite.list[json.login].show();
                        }
                        break;
                    }
                    case "accept":{
                        chat.dialog.dialogs.list[json.login] = new ChatDialogClass(json.login);
                        if(chat.chattype === json.type){
                            chat.dialog.dialogs.list[json.login].show();
                        }
                        break;
                    }*/
                    case "add":{
                        if(chat.room.list[json.cid1].listch[json.cid2] !== undefined) {
                            if (chat.room.list[json.cid1].listch[json.cid2].list[json.id] === undefined) {
                                chat.room.list[json.cid1].listch[json.cid2].list[json.id] = [];
                            }
                            chat.room.list[json.cid1].listch[json.cid2].list[json.id][json.idh] = new ChatMessageClass(json.id, json.idh);
                            chat.room.list[json.cid1].listch[json.cid2].list[json.id][json.idh].load(json.type, json.cid1, json.cid2);
                        }
                        break;
                    }
                }
                break;
            }
        }
    }
    loadMessage(type, count, chatid, chatch){
        var tmp = {type: type, act: 'get', count: count, up: true};
        if(chat.chattype === C_c2){
            tmp.id = chatid;
        }
        if(this.chattype === C_c3){
            tmp.id = chatid;
            tmp.idch = chatch;
        }
        sendAjaxJson('/chat', tmp, function(r) {
            if (r.state === true) {
                for (let i in r.list) {
                    let tmp = r.list[i];
                    switch (type) {
                        case C_c1: {
                            if (chat.public.list[tmp.id] === undefined) {
                                chat.public.list[tmp.id] = [];
                            }
                            chat.public.list[tmp.id][tmp.idh] = new ChatMessageClass(tmp.id, tmp.idh);
                            chat.public.list[tmp.id][tmp.idh].init(tmp.login, tmp.message);
                            if (chat.chattype === type) {
                                chat.public.list[tmp.id][tmp.idh].show();
                            }
                            break;
                        }
                        case C_c2: {
                            if(chat.dialog.dialogs.list[chatid] !== undefined) {
                                if (chat.dialog.dialogs.list[chatid].list[tmp.id] === undefined) {
                                    chat.dialog.dialogs.list[chatid].list[tmp.id] = [];
                                }
                                chat.dialog.dialogs.list[chatid].list[tmp.id][tmp.idh] = new ChatMessageClass(tmp.id, tmp.idh);
                                chat.dialog.dialogs.list[chatid].list[tmp.id][tmp.idh].init(tmp.logins, tmp.message);
                                if (chat.chattype === type && chat.chatid === chatid) {
                                    chat.dialog.dialogs.list[chatid].list[tmp.id][tmp.idh].show();
                                }
                            }
                            break;
                        }
                        case C_c3: {
                            if(chat.room.list[chatid].listch[chatch] !== undefined) {
                                if (chat.room.list[chatid].listch[chatch].list[tmp.id] === undefined) {
                                    chat.room.list[chatid].listch[chatch].list[tmp.id] = [];
                                }
                                chat.room.list[chatid].listch[chatch].list[tmp.id][tmp.idh] = new ChatMessageClass(tmp.id, tmp.idh);
                                chat.room.list[chatid].listch[chatch].list[tmp.id][tmp.idh].init(tmp.login, tmp.message);
                                if (chat.chattype === type && chat.chatid === chatid && chat.chatchannel === chatch) {
                                    chat.room.list[chatid].listch[chatch].list[tmp.id][tmp.idh].show();
                                }
                            }
                            break;
                        }
                    }
                }
                chat.chatScroll();
            }
        });
    }
    chatScroll(time){
        if (chat.scroll){
            chatcontent.stop();
            chatcontent.animate({ scrollTop: ccc.outerHeight(true)}, time !== undefined ? time : 1000);
        }
    }
}

var chat = new ChatClass();
chat.set();

$(document).ready(chat.connect);