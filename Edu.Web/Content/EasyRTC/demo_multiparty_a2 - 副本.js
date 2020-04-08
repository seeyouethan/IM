var activeBox = -1;  // nothing selected
var aspectRatio = 4 / 3;  // standard definition video aspect ratio
var maxCALLERS = 99;
var numVideoOBJS = maxCALLERS + 1;
var layout;
var streamName = "default";
var myrtcid = "";
var othertcidArry = new Array();//已经连接的其他用户
var myvideo;
var uid = "";//我自己的uid

Array.prototype.remove = function (val) {
    var index = this.indexOf(val);
    if (index > -1) {
        this.splice(index, 1);
    }
};

Array.prototype.indexOf = function (val) {
    for (var i = 0; i < this.length; i++) {
        if (this[i] == val) return i;
    }
    return -1;
};


easyrtc.dontAddCloseButtons(true);

function getIdOfBox(boxNum) {
    return "box" + boxNum;
}
function getIdOfDiv(boxNum) {
    return "otherDiv" + boxNum;
}function getIdOfP(boxNum) {
    return "otherP" + boxNum;
}


function reshapeFull(parentw, parenth) {
    return {
        left: 0,
        top: 0,
        width: parentw,
        height: parenth
    };
}

function reshapeTextEntryBox(parentw, parenth) {
    return {
        left: parentw / 4,
        top: parenth / 4,
        width: parentw / 2,
        height: parenth / 4
    }
}

function reshapeTextEntryField(parentw, parenth) {
    return {
        width: parentw - 40
    }
}

var margin = 20;

function reshapeToFullSize(parentw, parenth) {
    var left, top, width, height;
    var margin = 20;

    if (parentw < parenth * aspectRatio) {
        width = parentw - margin;
        height = width / aspectRatio;
    }
    else {
        height = parenth - margin;
        width = height * aspectRatio;
    }
    left = (parentw - width) / 2;
    top = (parenth - height) / 2;
    return {
        left: left,
        top: top,
        width: width,
        height: height
    };
}

function setThumbSizeAspect(percentSize, percentLeft, percentTop, parentw, parenth, aspect) {

    var width, height;
    if (parentw < parenth * aspectRatio) {
        width = parentw * percentSize;
        height = width / aspect;
    }
    else {
        height = parenth * percentSize;
        width = height * aspect;
    }
    var left;
    if (percentLeft < 0) {
        left = parentw - width;
    }
    else {
        left = 0;
    }
    left += Math.floor(percentLeft * parentw);
    var top = 0;
    if (percentTop < 0) {
        top = parenth - height;
    }
    else {
        top = 0;
    }
    top += Math.floor(percentTop * parenth);
    return {
        left: left,
        top: top,
        width: width,
        height: height
    };
}


function setThumbSize(percentSize, percentLeft, percentTop, parentw, parenth) {
    return setThumbSizeAspect(percentSize, percentLeft, percentTop, parentw, parenth, aspectRatio);
}

function setThumbSizeButton(percentSize, percentLeft, percentTop, parentw, parenth, imagew, imageh) {
    return setThumbSizeAspect(percentSize, percentLeft, percentTop, parentw, parenth, imagew / imageh);
}


var sharedVideoWidth = 1;
var sharedVideoHeight = 1;

function reshape1of2(parentw, parenth) {
    if (layout == 'p') {
        return {
            left: (parentw - sharedVideoWidth) / 2,
            top: (parenth - sharedVideoHeight * 2) / 3,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        };
    }
    else {
        return {
            left: (parentw - sharedVideoWidth * 2) / 3,
            top: (parenth - sharedVideoHeight) / 2,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        }
    }
}



function reshape2of2(parentw, parenth) {
    if (layout == 'p') {
        return {
            left: (parentw - sharedVideoWidth) / 2,
            top: (parenth - sharedVideoHeight * 2) / 3 * 2 + sharedVideoHeight,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        };
    }
    else {
        return {
            left: (parentw - sharedVideoWidth * 2) / 3 * 2 + sharedVideoWidth,
            top: (parenth - sharedVideoHeight) / 2,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        }
    }
}

function reshape1of3(parentw, parenth) {
    if (layout == 'p') {
        return {
            left: (parentw - sharedVideoWidth) / 2,
            top: (parenth - sharedVideoHeight * 3) / 4,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        };
    }
    else {
        return {
            left: (parentw - sharedVideoWidth * 2) / 3,
            top: (parenth - sharedVideoHeight * 2) / 3,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        }
    }
}

function reshape2of3(parentw, parenth) {
    if (layout == 'p') {
        return {
            left: (parentw - sharedVideoWidth) / 2,
            top: (parenth - sharedVideoHeight * 3) / 4 * 2 + sharedVideoHeight,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        };
    }
    else {
        return {
            left: (parentw - sharedVideoWidth * 2) / 3 * 2 + sharedVideoWidth,
            top: (parenth - sharedVideoHeight * 2) / 3,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        }
    }
}

function reshape3of3(parentw, parenth) {
    if (layout == 'p') {
        return {
            left: (parentw - sharedVideoWidth) / 2,
            top: (parenth - sharedVideoHeight * 3) / 4 * 3 + sharedVideoHeight * 2,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        };
    }
    else {
        return {
            left: (parentw - sharedVideoWidth * 2) / 3 * 1.5 + sharedVideoWidth / 2,
            top: (parenth - sharedVideoHeight * 2) / 3 * 2 + sharedVideoHeight,
            width: sharedVideoWidth,
            height: sharedVideoHeight
        }
    }
}


function reshape1of4(parentw, parenth) {
    return {
        left: (parentw - sharedVideoWidth * 2) / 3,
        top: (parenth - sharedVideoHeight * 2) / 3,
        width: sharedVideoWidth,
        height: sharedVideoHeight
    }
}

function reshape2of4(parentw, parenth) {
    return {
        left: (parentw - sharedVideoWidth * 2) / 3 * 2 + sharedVideoWidth,
        top: (parenth - sharedVideoHeight * 2) / 3,
        width: sharedVideoWidth,
        height: sharedVideoHeight
    }
}
function reshape3of4(parentw, parenth) {
    return {
        left: (parentw - sharedVideoWidth * 2) / 3,
        top: (parenth - sharedVideoHeight * 2) / 3 * 2 + sharedVideoHeight,
        width: sharedVideoWidth,
        height: sharedVideoHeight
    }
}

function reshape4of4(parentw, parenth) {
    return {
        left: (parentw - sharedVideoWidth * 2) / 3 * 2 + sharedVideoWidth,
        top: (parenth - sharedVideoHeight * 2) / 3 * 2 + sharedVideoHeight,
        width: sharedVideoWidth,
        height: sharedVideoHeight
    }
}

var boxUsed = [true, false, false, false];
var connectCount = 0;


function setSharedVideoSize(parentw, parenth) {
    layout = ((parentw / aspectRatio) < parenth) ? 'p' : 'l';
    var w, h;

    function sizeBy(fullsize, numVideos) {
        return (fullsize - margin * (numVideos + 1)) / numVideos;
    }

    switch (layout + (connectCount + 1)) {
        case 'p1':
        case 'l1':
            w = sizeBy(parentw, 1);
            h = sizeBy(parenth, 1);
            break;
        case 'l2':
            w = sizeBy(parentw, 2);
            h = sizeBy(parenth, 1);
            break;
        case 'p2':
            w = sizeBy(parentw, 1);
            h = sizeBy(parenth, 2);
            break;
        case 'p4':
        case 'l4':
        case 'l3':
            w = sizeBy(parentw, 2);
            h = sizeBy(parenth, 2);
            break;
        case 'p3':
            w = sizeBy(parentw, 1);
            h = sizeBy(parenth, 3);
            break;
    }
    sharedVideoWidth = Math.min(w, h * aspectRatio);
    sharedVideoHeight = Math.min(h, w / aspectRatio);
}

var reshapeThumbs = [
    function (parentw, parenth) {

        if (activeBox > 0) {
            return setThumbSize(0.20, 0.01, 0.01, parentw, parenth);
        }
        else {
            setSharedVideoSize(parentw, parenth)
            switch (connectCount) {
                case 0: return reshapeToFullSize(parentw, parenth);
                case 1: return reshape1of2(parentw, parenth);
                case 2: return reshape1of3(parentw, parenth);
                case 3: return reshape1of4(parentw, parenth);
            }
        }
    },
    function (parentw, parenth) {
        if (activeBox >= 0 || !boxUsed[1]) {
            return setThumbSize(0.20, 0.01, -0.01, parentw, parenth);
        }
        else {
            switch (connectCount) {
                case 1:
                    return reshape2of2(parentw, parenth);
                case 2:
                    return reshape2of3(parentw, parenth);
                case 3:
                    return reshape2of4(parentw, parenth);
            }
        }
    },
    function (parentw, parenth) {
        if (activeBox >= 0 || !boxUsed[2]) {
            return setThumbSize(0.20, -0.01, 0.01, parentw, parenth);
        }
        else {
            switch (connectCount) {
                case 1:
                    return reshape2of2(parentw, parenth);
                case 2:
                    if (!boxUsed[1]) {
                        return reshape2of3(parentw, parenth);
                    }
                    else {
                        return reshape3of3(parentw, parenth);
                    }
                    /* falls through */
                case 3:
                    return reshape3of4(parentw, parenth);
            }
        }
    },
    function (parentw, parenth) {
        if (activeBox >= 0 || !boxUsed[3]) {
            return setThumbSize(0.20, -0.01, -0.01, parentw, parenth);
        }
        else {
            switch (connectCount) {
                case 1:
                    return reshape2of2(parentw, parenth);
                case 2:
                    return reshape3of3(parentw, parenth);
                case 3:
                    return reshape4of4(parentw, parenth);
            }
        }
    },
];


function killButtonReshaper(parentw, parenth) {
    var imagew = 128;
    var imageh = 128;
    if (parentw < parenth) {
        return setThumbSizeButton(0.1, -0.51, -0.01, parentw, parenth, imagew, imageh);
    }
    else {
        return setThumbSizeButton(0.1, -0.01, -0.51, parentw, parenth, imagew, imageh);
    }
}


function muteButtonReshaper(parentw, parenth) {
    var imagew = 32;
    var imageh = 32;
    if (parentw < parenth) {
        return setThumbSizeButton(0.10, -0.51, 0.01, parentw, parenth, imagew, imageh);
    }
    else {
        return setThumbSizeButton(0.10, 0.01, -0.51, parentw, parenth, imagew, imageh);
    }
}

function reshapeTextEntryButton(parentw, parenth) {
    var imagew = 32;
    var imageh = 32;
    if (parentw < parenth) {
        return setThumbSizeButton(0.10, 0.51, 0.01, parentw, parenth, imagew, imageh);
    }
    else {
        return setThumbSizeButton(0.10, 0.01, 0.51, parentw, parenth, imagew, imageh);
    }
}


function collapseToThumb() {
    collapseToThumbHelper();
    activeBox = -1;
    updateMuteImage(false);

}


function killActiveBox() {
    if (activeBox > 0) {
        var easyrtcid = easyrtc.getIthCaller(activeBox - 1);
        collapseToThumb();
        setTimeout(function () {
            easyrtc.hangup(easyrtcid);
        }, 400);
    }
}


function muteActiveBox() {
    updateMuteImage(true);
}




function callEverybodyElse(roomName, otherPeople, selfInfo) {
    console.log("[callEverybodyElse]开始通知其他用户------>");
    for (var i in otherPeople) {
        othertcidArry.remove(i);
        othertcidArry.push(i);
        console.log("[callEverybodyElse]easyrtcid = " + i + " belongs to user " + otherPeople[i].username);
        setMemberRtcid(otherPeople[i].username, i);
    }
    easyrtc.setRoomOccupantListener(null); // so we're only called once.

    var list = [];
    var connectCount = 0;
    for (var easyrtcid in otherPeople) {
        list.push(easyrtcid);
    }

    function establishConnection(position) {
        function callSuccess() {
            console.log("[callEverybodyElse]连接建立成功---->" + otherPeople[list[position]].username);
            connectCount++;
            if (connectCount < maxCALLERS && position > 0) {
                establishConnection(position - 1);
            }
        }
        function callFailure(errorCode, errorText) {
            console.log("[callEverybodyElse]Error!连接建立失败---->" + otherPeople[list[position]].username);
            easyrtc.showError(errorCode, errorText);
            if (connectCount < maxCALLERS && position > 0) {
                establishConnection(position - 1);
            }
        }
        var keys = easyrtc.getLocalMediaIds();
        console.log("[callEverybodyElse]开始连接用户【" + position + "】---->" + otherPeople[list[position]].username);
        easyrtc.call(list[position], callSuccess, callFailure, acceptedCB, keys);

    }
    if (list.length > 0) {
        establishConnection(list.length - 1);
    }
}


function loginSuccess(id) {
    //延迟两秒执行
    setTimeout(function() {
        streamName = "default";
        myrtcid = id;
        console.log("[loginSuccess]进入房间成功，我的easyrtcid----->", id);
        setMemberConnected(uid, id);
        /**
        * 如果没有本地摄像头，那么就需要打开桌面窗口提示
        */
        easyrtc.initMediaSource(
                function (stream) {
                    console.log("[loginSuccess]本地摄像头初始化成功----->", stream);
                    easyrtc.setVideoObjectSrc(myvideo, stream);
                    console.log("[loginSuccess]开始向房间内其他用户发送摄像头数据------>", othertcidArry);
                    for (var i = 0; i < othertcidArry.length; i++) {
                        console.log("[loginSuccess]------>【" + i + "】向房间内" + othertcidArry[i] + "用户发送摄像头数据------>", othertcidArry[i], streamName);
                        easyrtc.addStreamToCall(othertcidArry[i], streamName);
                    }
                    /**
                     * 第一次登陆的时候，有时候加载的其他用户视频不会自动播放，所以这里利用jquery再去play
                     */
                    $.each($('#submainVideo video'), function (n, value) {
                        $(value).get(0).play();
                    })
                },
                function (errCode, errText) {
                    /**
                     * 摄像头读取错误，使用屏幕窗口分享
                     */

                    alert("未检测到摄像头，开启桌面共享后才能加入会议，请手动点击屏幕下方[屏幕共享]按钮");
                    return;
                    console.log("[loginSuccess]本地摄像头初始化失败----->", errCode, errText);
                    console.log("[loginSuccess]开始使用桌面分享----->");
                    streamName = "screen";
                    easyrtc.initDesktopStream(
                        function (stream) {
                            console.log("[loginSuccess]本地桌面分享初始化成功----->", stream);
                            easyrtc.setVideoObjectSrc(myvideo, stream);
                            console.log("[loginSuccess]开始向房间内其他用户发送桌面分享数据------>", othertcidArry);
                            for (var i = 0; i < othertcidArry.length; i++) {
                                console.log("[loginSuccess]------>【" + i + "】向房间内" + othertcidArry[i] + "用户发送桌面分享数据------>", othertcidArry[i], streamName);
                                easyrtc.addStreamToCall(othertcidArry[i], streamName);
                            }
                            /**
                             * 第一次登陆的时候，有时候加载的其他用户视频不会自动播放，所以这里利用jquery再去play
                             */
                            $.each($('#submainVideo video'), function (n, value) {
                                $(value).get(0).play();
                            });

                        },
                        function (errCode, errText) {
                            console.log("[loginSuccess]本地桌面分享初始化失败----->", errCode, errText);
                            easyrtc.showError(errCode, errText);
                        },
                        streamName);
                }, streamName);
    }, 2000);
}

function loginFailure(errorCode, message) {
    console.log("[loginFailure]Error!进入房间失败，我的easyrtcid----->", errorCode, message);
    easyrtc.showError(errorCode, message);
}



function rtcAppInit(roomName, selfVideo,username, iCount,w,h,f) {
    myvideo = selfVideo;
    maxCALLERS = iCount;
    numVideoOBJS = maxCALLERS + 1;
    easyrtc.setUsername(username);
    uid = username;//我自己的uid
    easyrtc.setVideoDims(Number(w), Number(h),Number(f));//16:9 20  分辨率和帧率
    easyrtc.setRoomOccupantListener(callEverybodyElse);
    easyrtc.connect(roomName, loginSuccess, loginFailure);
    easyrtc.setAutoInitUserMedia(true);
    easyrtc.setIceUsedInCalls(getModifiedIceList());//使用打洞服务器
    easyrtc.setDisconnectListener(function () {
        console.log("[rtcAppInit]Error!连接丢失------>", "Lost connection to signaling server");
    });
    /**
     * 收到其他用户的视频流
     * 因为setOnStreamClosed这个回调无法触发，所以无法将用户原来的摄像头流切换成为桌面窗口流，所以这个在创建的时候，先删除一下，然后再创建一个新的
     */
    easyrtc.setStreamAcceptor(function (easyrtcid, stream, streamName) {
        console.log("------------收到其他用户的视频流消息------start---------------");
        console.log("easyrtcid------>", easyrtcid);
        console.log("stream------>", stream);
        console.log("streamName------>", streamName);
        console.log("------------收到其他用户的视频流消息------end---------------");
        removeVideo(easyrtcid);
        createVideo(easyrtcid, stream);

        //先删除下，在添加，防止添加重复的
        othertcidArry.remove(easyrtcid);
        othertcidArry.push(easyrtcid);
        setMemberConnected(easyrtc.idToName(easyrtcid), easyrtcid);
    });

    /**
     * 关闭流的响应，但是这个回调无法触发，具体原因未知
            easyrtc.closeLocalStream(streamName);
            easyrtc.closeLocalMediaStream()
        都无法触发这个回调
        只有在关闭页面的时候，才会触发这个回调函数
     * 
     */
    easyrtc.setOnStreamClosed(function (easyrtcid, stream, streamName) {
        console.log("------------远程用户关闭了视频流-------start---------------");
        console.log("easyrtcid------>", easyrtcid);
        console.log("stream------>", stream);
        console.log("streamName------>", streamName);
        console.log("------------远程用户关闭了视频流-------end---------------");
        removeVideo(easyrtcid);
    });

    //这个方法暂时没用到，
    easyrtc.setCallCancelled(function (easyrtcid, explicitlyCancelled) {
        console.log("------------setCallCancelled---------------");
        console.log("easyrtcid------>", easyrtcid);
        console.log("explicitlyCancelled------>", easyrtcid);
        if( explicitlyCancelled ){
            console.log(easyrtc.idToName(easyrtcid) + " stopped trying to reach you");
        }
        else{
            console.log("Implicitly called "  + easyrtc.idToName(easyrtcid));
        }
    });


    var screenShareButton = document.getElementById("ScreenShare");
    screenShareButton.onclick = function () {
        var keys = easyrtc.getLocalMediaIds();

        if (screenShareButton.innerText === "取消屏幕共享") {
            //关闭屏幕分享,启用视频设备 
            easyrtc.closeLocalStream(streamName);
            easyrtc.closeLocalMediaStream();
            screenShareButton.innerText = "屏幕共享";
            easyrtc.enableCamera(true, streamName);
            var copyArr = $.extend(true, [], othertcidArry);//拷贝一个数组
            /*
                因为closeLocalStream和closeLocalMediaStream都无法触发setOnStreamClosed这个回调，所以这里使用hangupAll 来触发其他用户的setOnStreamClosed这个方法
                先挂断房间内所有的用户，然后再重新连接房间内的每个用户
            */
            if (easyrtc.getConnectionCount() > 0) {
                easyrtc.hangupAll();
            }
            for (var i = 0; i < copyArr.length; i++) {
                easyrtc.call(copyArr[i], successCB, failureCB, acceptedCB, keys);
            }
            streamName = "default";
            easyrtc.initMediaSource(
                    function (stream) {
                        easyrtc.setVideoObjectSrc(myvideo, stream);
                        console.log("othertcidArry------>", othertcidArry);
                        for (var i = 0; i < copyArr.length; i++) {
                            easyrtc.addStreamToCall(copyArr[i], streamName);
                        }
                    },
                    function (errCode, errText) {
                        easyrtc.showError(errCode, errText);
                    }, streamName);

        } else if (screenShareButton.innerText === "屏幕共享") {
            //如果自己开启了摄像头，那么关闭摄像头
            easyrtc.closeLocalStream(streamName);
            easyrtc.closeLocalMediaStream();
            easyrtc.enableCamera(false, streamName);
            screenShareButton.innerText = "取消屏幕共享";
            var copyArr = $.extend(true, [], othertcidArry);//拷贝一个数组
            if (easyrtc.getConnectionCount() > 0) {
                easyrtc.hangupAll();

            }
            for (var i = 0; i < copyArr.length; i++) {
                easyrtc.call(copyArr[i], successCB, failureCB, acceptedCB, keys);
            }
            streamName = "screen";
            easyrtc.initDesktopStream(
                    function (stream) {
                        easyrtc.setVideoObjectSrc(myvideo, stream);
                        console.log("othertcidArry------>", othertcidArry);
                        for (var i = 0; i < copyArr.length; i++) {
                            easyrtc.addStreamToCall(copyArr[i], streamName);
                        }
                    },
                    function (errCode, errText) {
                        easyrtc.showError(errCode, errText);
                    },
                    streamName);
        }
    };
}


function createVideo(rtcid, stream) {
    var userid = easyrtc.idToName(rtcid);
    var nickname = $("#p" + userid).html();
    var newP = document.createElement("p");
    newP.innerHTML = nickname;
    newP.setAttribute("class", "videoTitle");
    var newVideo = document.createElement("video");
    //video的Id 必须设置个id,否则无法观看
    var id = "box" + rtcid;
    //div的Id
    var idDiv = "otherDiv" + rtcid;
    //p标签的Id
    var idP = "otherP" + rtcid;
    newP.setAttribute("id", idP);
    //设置video的属性
    newVideo.setAttribute("autoplay", "autoplay");
    newVideo.setAttribute("id", id);
    newVideo.setAttribute("class", "transit boxCommon thumbCommon k-video");
    newVideo.setAttribute("playsinline", "playsinline");
    newVideo.autoplay = true;
    easyrtc.setVideoObjectSrc(newVideo, stream);
    newVideo.onclick = function () { fullScreen(newVideo); }

    var newDiv = document.createElement("div");
    newDiv.setAttribute("id", idDiv);
    newDiv.setAttribute("class", "remoteMemberDiv");
    newDiv.classList.add("vShow");
    newDiv.appendChild(newP);
    newDiv.appendChild(newVideo);

    $('#submainVideo').append(newDiv);
}


function removeVideo(rtcid) {
    othertcidArry.remove(rtcid);
    setMemberDisConnected(rtcid);
    //div的Id
    var idDiv = "otherDiv" + rtcid;
    var item = document.getElementById(idDiv);
    if (item) {
        item.parentNode.removeChild(item);
    }
}



function setMemberRtcid(uid, rtcid) {
    var ele = $("#videoMember li[id='" + "li" + uid + "']");
    if (ele.length !== 0) {
        ele.attr("rtcid", rtcid);
    } else {
        //console.log("setMemberRtcid未找到成员！");
    }
}




function setMemberConnected(uid, rtcid) {
    var ele = $("#videoMember li[id='" + "li" + uid + "']");
    if (ele.length !== 0) {
        ele.attr("rtcid", rtcid);
        ele.find(".p2").addClass("inRoom").text("已加入");
    } else {
        //console.log("setMemberConnected未找到成员！");
    }
}

function setMemberDisConnected(rtcid) {
    var ele = $("#videoMember li[rtcid='" + rtcid + "']");
    if (ele.length !== 0) {
        ele.attr("rtcid", "");
        ele.find(".p2").removeClass("inRoom").text("未加入");
        //console.log("setMemberDisConnected成功！");
    } else {
        //console.log("setMemberDisConnected未找到成员！");
    }
}

function getModifiedIceList() {
    var iceList = [
        {
            "urls": "stun:www.385073012.cn:3478",
            "username": "cnki",
            "credential": "123456"//可选
        },
        {
            "urls": "turn:www.385073012.cn:3478",
            "username": "cnki",//可选
            "credential": "123456"//可选
        },
        {
            "urls": "turn:www.385073012.cn:3478?transport=tcp",
            "username": "cnki",//可选
            "credential": "123456"//可选
        }
    ];
    return { iceServers: iceList };
}



var successCB = function () {
    console.log("[successCB]------>call success");
};
var failureCB = function () {
    console.log("[failureCB]------>call failed");
};

var acceptedCB = function (accepted, easyrtcid) {
    if (!accepted) {
        easyrtc.showError("CALL-REJECTED", "Sorry, your call to " + easyrtc.idToName(easyrtcid) + " was rejected");
        console.log("[acceptedCB]------> Error! CALL-REJECTED", "Sorry, your call to " + easyrtc.idToName(easyrtcid) + " was rejected");
    }
    else {
        console.log("[acceptedCB]------> CALL-Accepted", "your call to " + easyrtc.idToName(easyrtcid) + " was accepted");
    }
};


//进入全屏
function fullScreen(ele) {
    if (ele.requestFullscreen) {
        ele.requestFullscreen();
    } else if (ele.mozRequestFullScreen) {
        ele.mozRequestFullScreen();
    } else if (ele.webkitRequestFullScreen) {
        ele.webkitRequestFullScreen();
    }
}



