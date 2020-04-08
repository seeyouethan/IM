

$.connection.hub.url = "http://okcs.test.cnki.net/okcssignalr/";
$.connection.hub.logging = true;
$.connection.hub.qs = { 'conferenceid': window.groupid, 'uid': window.uid, 'livetype': 'camera', };
var chat = $.connection.conferenceChatHub;
$.connection.hub.start()
    .done(function () { })
    .fail(function () {
        console.log("conferenceChatHub------>Connection failed!");
    });
//告诉自己上线
chat.client.notifySelfUserOnline = function (model) {
    window.onlive = model;
    console.log("ConferenceChatHub 连接成功！是否开启分享：",model);

}
//收到其他人的上线通知
chat.client.notifyOtherUserOnline = function (model) {
    UserOnline(model);
}

//告诉自己离线
chat.client.notifySelfUserOffline = function () {

}

//收到其他人的离线通知
chat.client.notifyOtherUserOnline = function (model) {
    UserOffline(model);
}

//其他消息
chat.client.notifyMessage = function (model) {
    console.log(model);
}