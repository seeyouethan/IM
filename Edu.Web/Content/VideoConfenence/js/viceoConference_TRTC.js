//RtcMulti部分
DoConnection = function () {


    var connection = new RTCMultiConnection();



    connection.userid = window.uid;

    //connection.extra = { "uid": window.uid }//将oaokcs中的用户id值存放到extra中

    connection.socketURL = 'https://rtc.cnki.net/';//https://www.385073012.cn/  

    connection.socketMessageEvent = 'video-conference-demo-group01';

    connection.AppID = 'okcs-im';
    connection.StartDate = '2019-10-11 15:26:22';
    connection.UserName = 'okcs-im';




    connection.session = {
        data: true,
    };


    connection.iceServers = [
       {
           "urls": "stun:www.385073012.cn:3478",
       },
       {
           "urls": "turn:www.385073012.cn:3478",
           "username": "cnki",//可选
           "credential": "123456"//可选
       }];



    connection.onmessage = function (event) {
        //收到消息  
        /*
            0 表示聊天消息
            1 表示当前正在分享用户个数的消息
            2 表示xxx用户离线的消息
            3 表示xxx用户申请发言
            4 表示主持人同意发言
            5 表示主持人拒绝发言
            6 表示xxx用户停止发言
        */
        if (event.data.type === 0) {
            //聊天消息
            GetMessage(event.data.content);
        }
        else if (event.data.type === 1) {
            //当前正在分享用户个数的消息
            SetOnLiveUserList(event.data.content);
        }
        else if (event.data.type === 2) {
            //离线消息

            UserOffline(event.userid)
            var mediaElement = document.getElementById(event.userid);
            if (mediaElement) {
                mediaElement.parentNode.removeChild(mediaElement);
                console.warn(event.userid + "------onmessage");

            }
        } else if (event.data.type === 3) {
            //发言申请
            GetApplyForSpeech(event.data.content)
        } else if (event.data.type === 4) {
            //发言申请被通过
            GetAgreeSpeech(event.data.content)
        } else if (event.data.type === 5) {
            //发言申请被拒绝
            GetDisAgreeSpeech(event.data.content)
        } else if (event.data.type === 6) {
            //被停止发言
            GetStopSpeech(event.data.content)
        }
        else if (event.data.type === 7) {
            //将该用户的状态设置为发言状态   /  不发言状态
            //SetIsSpeaking(event.data.content.uid,event.data.content.)
        }
    };


    //用户进入房间即可触发（不分享摄像头也会触发）
    connection.onopen = function (event) {
        //收到其他用户上线，设置其为上线状态
        UserOnline(event.userid);
    };
    connection.onclose = function (event) {
        //收到其他用户离线，设置其为离线状态
        UserOffline(event.userid);
    };

    // ......................................................
    // ......................Handling Room-ID................
    // ......................................................


    //关闭浏览器
    connection.onleave = function (event) {
        if (event.userid == window.uid) {
            RemoveOnLiveUser(event.userid);
        }
    };

    connection.closeBeforeUnload = false;
    window.onbeforeunload = function (event) {
        RemoveOnLiveUser(event.userid)
        /*发送离线消息*/
        multiRtc.send({ "type": 2, "content": event.userid + " leave room" });        
    };


    connection.openOrJoin(window.groupid, function (isRoomExist, roomid, error) {
        if (error) {
            console.log(error);

            if (error == "Room not available") {
                //重新加入
                setTimeout(function () {
                    connection.openOrJoin(window.groupid, function (isRoomExist, roomid, error) {
                        if (error) {
                            alert("重新加入房间失败");
                        }
                        else {
                            //设置左侧自己上线
                            UserOnline(window.uid);

                        }
                        //分享摄像头/桌面
                        ShowCameraOrDesktop();
                        window.setInterval(function () {
                            //console.log("定时任务-----------")
                            connection.socket.emit('', '1');
                        }, 10000);
                    });
                }, 1000);
            }
        }
        else {
            //设置左侧自己上线
            UserOnline(window.uid);
        }
        //分享摄像头/桌面
        ShowCameraOrDesktop();
        window.setInterval(function () {
            connection.socket.emit('', '1');
        }, 10000);
    });






    function getTrueNameByUserid(uid) {
        var ele = $(".member-ul li[data-uid='" + uid + "']");
        if (ele.length !== 0) {
            return ele.attr("data-title");
        }
        return "";
    }


    return connection;
}




/*
var newsocket = io('https://rtc.cnki.net/');
newsocket.emit('check-presence', 'f348c8dc-87f5-4fdf-9990-921f7990df50', function(isRoomExist, _roomid, extra) {
                console.log('checkPresence.isRoomExist: ', isRoomExist, ' roomid: ', _roomid);
                
            });

*/