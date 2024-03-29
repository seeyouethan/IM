﻿var activeBox = -1;  // nothing selected
var aspectRatio = 4 / 3;  // standard definition video aspect ratio
var maxCALLERS = 99;
var numVideoOBJS = maxCALLERS + 1;
var layout;
var uid = "";


easyrtc.dontAddCloseButtons(true);

function getIdOfBox(boxNum) {
    return "box" + boxNum;
}
function getIdOfDiv(boxNum) {
    return "otherDiv" + boxNum;
}
function getIdOfP(boxNum) {
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

//
// a negative percentLeft is interpreted as setting the right edge of the object
// that distance from the right edge of the parent.
// Similar for percentTop.
//
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


function handleWindowResize() {
    
}


function setReshaper(elementId, reshapeFn) {
    
}


function collapseToThumbHelper() {
   
}

function collapseToThumb() {
    collapseToThumbHelper();
    activeBox = -1;
    updateMuteImage(false);
    handleWindowResize();

}

function updateMuteImage(toggle) {
    
}


function expandThumb(whichBox) {
    
}

function prepVideoBox(whichBox) {
    
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
    for (var i in otherPeople) {
        console.log("easyrtcid= " + i + " belongs to user " + otherPeople[i].username);
        setMemberRtcid(otherPeople[i].username, i);
    }

    easyrtc.setRoomOccupantListener(null); // so we're only called once.

    var list = [];
    var connectCount = 0;
    for (var easyrtcid in otherPeople) {
        list.push(easyrtcid);
    }
    //
    // Connect in reverse order. Latter arriving people are more likely to have
    // empty slots.
    //
    function establishConnection(position) {
        function callSuccess() {
            connectCount++;
            if (connectCount < maxCALLERS && position > 0) {
                establishConnection(position - 1);
            }
        }
        function callFailure(errorCode, errorText) {
            easyrtc.showError(errorCode, errorText);
            if (connectCount < maxCALLERS && position > 0) {
                establishConnection(position - 1);
            }
        }
        easyrtc.call(list[position], callSuccess, callFailure);

    }
    if (list.length > 0) {
        establishConnection(list.length - 1);
    }
}


function loginSuccess(rtcid) {
    console.log("我的 easyrtcid is------>", rtcid);

    setMemberConnected(uid, rtcid);
}


function cancelText() {
    
}


function sendText(e) {
   
    return false;
}


function showTextEntry() {

}


function showMessage(startX, startY, content) {
    
}

function messageListener(easyrtcid, msgType, content) {
    for (var i = 0; i < maxCALLERS; i++) {
        if (easyrtc.getIthCaller(i) == easyrtcid) {
            var startArea = document.getElementById(getIdOfBox(i + 1));
            var startX = parseInt(startArea.offsetLeft) + parseInt(startArea.offsetWidth) / 2;
            var startY = parseInt(startArea.offsetTop) + parseInt(startArea.offsetHeight) / 2;
            showMessage(startX, startY, content);
        }
    }
}

function setMemberRtcid(uid, rtcid) {
    var ele = $("#videoMember li[id='" + "li" + uid + "']");
    if (ele.length !== 0) {
        ele.attr("rtcid", rtcid);
    } else {
        console.log("setMemberRtcid未找到成员！");
    }
}




function setMemberConnected(uid, rtcid) {
    var ele = $("#videoMember li[id='" + "li" + uid + "']");
    if (ele.length !== 0) {
        ele.attr("rtcid", rtcid);
        ele.find(".p2").addClass("inRoom").text("已加入");
    } else {
        console.log("setMemberConnected未找到成员！");
    }
}

function setMemberDisConnected(rtcid) {
    var ele = $("#videoMember li[rtcid='" + rtcid + "']");
    if (ele.length !== 0) {
        ele.attr("rtcid", "");
        ele.find(".p2").removeClass("inRoom").text("未加入");
    } else {
        console.log("setMemberDisConnected未找到成员！");
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
        }
    ];
    return { iceServers: iceList };
}

function getRealName(rtcid) {
    var ele = $("#videoMember li[rtcid='" + rtcid + "']");
    if (ele.length !== 0) {
        if (ele.find(".p1")) {
            return ele.find(".p1").html();
        }
        return "undefined";
    } else {
        return "undefined";
    }
}


function rtcAppInit(roomName, selfVideo, remoteVideos, username, iCount) {

    maxCALLERS = iCount;
    numVideoOBJS = maxCALLERS + 1;
    
    easyrtc.setUsername(username);

    uid = username;//我自己的uid
    easyrtc.setRoomOccupantListener(callEverybodyElse);
    easyrtc.easyApp(roomName, selfVideo, remoteVideos, loginSuccess);
    easyrtc.setIceUsedInCalls(getModifiedIceList());//使用打洞服务器
    easyrtc.setPeerListener(messageListener);
    easyrtc.setDisconnectListener(function () {
        easyrtc.showError("LOST-CONNECTION", "Lost connection to signaling server");
    });
    easyrtc.setOnCall(function (easyrtcid, slot) {
        console.log("getConnection username------> " + easyrtc.idToName(easyrtcid) + " connect success");
        console.log("getConnection easyrtcid------>" + easyrtcid + " connect success");
        setMemberConnected(easyrtc.idToName(easyrtcid),easyrtcid);
        boxUsed[slot + 1] = true;
        if (activeBox == 0) { // first connection
            collapseToThumb();
        }
        document.getElementById(getIdOfBox(slot + 1)).style.visibility = "visible";
        document.getElementById(getIdOfDiv(slot + 1)).classList.add("vShow");
        
        document.getElementById(getIdOfP(slot + 1)).innerText = getRealName(easyrtcid);
        handleWindowResize();
    });


    easyrtc.setOnHangup(function (easyrtcid, slot) {
        console.log("setOnHangup------> easyrtcid " + easyrtcid + " disconnect success");
        boxUsed[slot + 1] = false;
        if (activeBox > 0 && slot + 1 === activeBox) {
            collapseToThumb();
        }
        setTimeout(function () {
            document.getElementById(getIdOfBox(slot + 1)).style.visibility = "hidden";
            document.getElementById(getIdOfDiv(slot + 1)).classList.remove("vShow");

            setMemberDisConnected(easyrtcid);
        }, 20);
    });
}