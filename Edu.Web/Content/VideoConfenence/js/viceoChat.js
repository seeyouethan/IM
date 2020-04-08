
var connection = new RTCMultiConnection();
connection.userid = window.uid;
connection.socketURL = 'https://rtc.cnki.net/';//https://www.385073012.cn/  
connection.socketMessageEvent = 'video-chat-personal';

connection.AppID = 'okcs-im';
connection.StartDate = '2019-11-11 11:11:11';
connection.UserName = 'okcs-im-personal';

connection.mediaConstraints.video = true;
connection.addStream({
    video: true,
    audio: true,
});

connection.bandwidth = {
    audio: 128,//audio bitrates. Minimum 6 kbps and maximum 510 kbps
    video: 1024*2,//video framerates. Minimum 100 kbps; maximum 2000 kbps
    screen: 1024,//screen framerates. Minimum 300 kbps; maximum 4000 kbps
};
connection.processSdp = function (sdp) {
    sdp = BandwidthHandler.setApplicationSpecificBandwidth(sdp, connection.bandwidth, !!connection.session.screen);
    sdp = BandwidthHandler.setVideoBitrates(sdp, {
        min: connection.bandwidth.video,
        max: connection.bandwidth.video
    });
    sdp = BandwidthHandler.setOpusAttributes(sdp);
    sdp = BandwidthHandler.setOpusAttributes(sdp, {
        'stereo': 1,
        //'sprop-stereo': 1,
        'maxaveragebitrate': connection.bandwidth.audio * 1000 * 8,
        'maxplaybackrate': connection.bandwidth.audio * 1000 * 8,
        //'cbr': 1,
        //'useinbandfec': 1,
        //'usedtx': 1,
        'maxptime': 3
    });
    return sdp;
};

connection.sdpConstraints.mandatory = {
    OfferToReceiveAudio: true,
    OfferToReceiveVideo: true
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

connection.videosContainer = document.getElementById('videos-container');
connection.onstream = function (event) {
    console.log(event.userid + " enter room");
    var existing = document.getElementById(event.streamid);
    if (existing && existing.parentNode) {
        existing.parentNode.removeChild(existing);
    }
    event.mediaElement.removeAttribute('src');
    event.mediaElement.removeAttribute('srcObject');
    event.mediaElement.muted = true;
    event.mediaElement.volume = 0;

    var video = document.createElement('video');

    try {
        video.setAttributeNode(document.createAttribute('autoplay'));
        video.setAttributeNode(document.createAttribute('playsinline'));
    } catch (e) {
        video.setAttribute('autoplay', true);
        video.setAttribute('playsinline', true);
    }
    if (event.type === 'local') {
        video.volume = 0;
        try {
            video.setAttributeNode(document.createAttribute('muted'));
        } catch (e) {
            video.setAttribute('muted', true);
        }        
    } else {
        //其他成员的视频
        connection.videosContainer = document.getElementById('remotes-container');      
        var ele = $("#remotes-container div[data-uid='" + event.userid + "']");
        if (ele.length > 0) {
            ele.remove();
        }
    }
    video.srcObject = event.stream;
    var configOnMutedL, configOnUnMutedL, configOnGdL;
    if (event.type === 'local') {
        configOnMutedL = onMutedL;//静音 / 停止视频
        configOnUnMutedL = onUnMutedL;//取消静音  /  取消停止视频
        configOnGdL = onGdL;//挂断
    }
    var mediaElement = getHTMLMediaElement(video, {
        title: getTrueNameByUserid(event.userid),
        buttons: ['mute-audio', 'mute-video', 'mute-desk', 'full-screen'],
        showOnMouseEnter: false,
        uid: event.userid,
        onUnMuted: configOnUnMutedL,
        onMuted: configOnMutedL,
        localType: event.type,
        onGd: configOnGdL,
    });

    connection.videosContainer.appendChild(mediaElement);
    //隐藏其他图标
    if (event.type === 'local') {
        $("#span0").hide();
        $("#i0").hide();
    } else {
        $("#span1").hide();
        $("#i1").hide();
    }


    setTimeout(function () {
        mediaElement.media.play();
    }, 5000);

    mediaElement.id = event.streamid;
    localStorage.setItem(connection.socketMessageEvent, connection.sessionid);
};



connection.onstreamended = function (event) {
    var mediaElement = document.getElementById(event.streamid);
    if (mediaElement) {
        mediaElement.parentNode.removeChild(mediaElement);
    }

    if (event.type === 'local') {

    } else {
        //如果对方挂断了视频
        CloseChatVideo();
    }
};

connection.onMediaError = function (e) {
    if (e.message === 'Concurrent mic process limit.') {

        var mediaElement = document.getElementById(e.streamid);
        if (mediaElement) {
            mediaElement.parentNode.removeChild(mediaElement);

        }
        if (DetectRTC.audioInputDevices.length <= 1) {
            alert('Please select external microphone. Check github issue number 483.');
            return;
        }

        var secondaryMic = DetectRTC.audioInputDevices[1].deviceId;
        connection.mediaConstraints.audio = {
            deviceId: secondaryMic
        };

        connection.join(connection.sessionid);
    } else {
        var mediaElement = document.getElementById(e.streamid);
        if (mediaElement) {
            mediaElement.parentNode.removeChild(mediaElement);

        }
    }

};


// ......................................................
// ......................Handling Room-ID................
// ......................................................


connection.openOrJoin(window.roomid, function (isRoomExist, roomid, error) {
    if (error) {
        alert(error);
    }
    else {
    }
    if (connection.isInitiator === true) {
        //创建房间

    } else {
        //加入房间
    }
});

window.setInterval(function () {
    connection.socket.emit('', '1');
}, 10000);








function getTrueNameByUserid(uid) {
   var ele = $(".member-ul li[data-uid='" + uid + "']");
   if (ele.length !== 0) {
         return ele.attr("title");
   }
   return "";
}



function onUnMutedL(type){
      connection.attachStreams[0].unmute(type);
      
}

function onMutedL(type){
      connection.attachStreams[0].mute(type);
}
//挂断，直接离开房间（目前这块还有bug,挂断后，无法离开房间）
function onGdL() {
    connection.getAllParticipants().forEach(function (pid) {
        connection.disconnectWith(pid);
    });

    // stop all local cameras
    connection.attachStreams.forEach(function (localStream) {
        localStream.stop();
    });

    // close socket.io connection
    connection.closeSocket();
    connection.close();
    CloseChatVideo();
}