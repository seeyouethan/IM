

$.connection.hub.url = "http://okcs.test.cnki.net/okcssignalr/";
$.connection.hub.logging = true;
$.connection.hub.qs = { 'conferenceid': window.groupid, 'uid': window.uid, 'livetype': 'camera', };
var chat = $.connection.conferenceChatHub;
$.connection.hub.start()
    .done(function () { })
    .fail(function () {
        console.log("conferenceChatHub------>Connection failed!");
    });
//�����Լ�����
chat.client.notifySelfUserOnline = function (model) {
    window.onlive = model;
    console.log("ConferenceChatHub ���ӳɹ����Ƿ�������",model);

}
//�յ������˵�����֪ͨ
chat.client.notifyOtherUserOnline = function (model) {
    UserOnline(model);
}

//�����Լ�����
chat.client.notifySelfUserOffline = function () {

}

//�յ������˵�����֪ͨ
chat.client.notifyOtherUserOnline = function (model) {
    UserOffline(model);
}

//������Ϣ
chat.client.notifyMessage = function (model) {
    console.log(model);
}