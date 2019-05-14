
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

var chatinputarea = $('<div id="chatinputarea" class="container p-0 bordersc"></div>').insertAfter(chatcontent);
var ciad = $('<div></div>').appendTo(chatinputarea);

var ccc = $('<div class="container-fluid p-0"></div>').appendTo(chatcontent);
$(document).ready(function () {
    ws = new WebSocket('wss://'+address+'/ws');
    ws.onopen = function(evt){
    	console.log('wsopened');
        console.log(evt);
    	console.log(ws);
    };
    ws.onclose = function(evt) {
    	console.log('wsclosed');
        console.log(evt);
    }
    ws.onerror = function(evt) {
        console.log('wserror');
    	console.log(evt);
    };
});