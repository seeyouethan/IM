
//emoji初始化
window.InitImEmoji = function () {

    $("#msgTextarea").emoji({
        button: "#emojiBtn",
        showTab: false,
        animation: 'slide',
        icons: [
            {
                name: "表情",
                path: "http://" + window.location.host + "/im/Tookit/jQuery-emoji/img/emoji/",
                maxNum: 84,
                file: ".png",
                placeholder: "#{alias}#",
                alias: {
                    1: "呵呵",
                    2: "色",
                    3: "失望",
                    4: "惊讶",
                    5: "嘻嘻",
                    6: "亲亲",
                    7: "调皮",
                    8: "皱眉",
                    9: "失望",
                    10: "尴尬",
                    11: "大哭",
                    12: "吐舌",
                    13: "恼火",
                    14: "不开心",
                    15: "轻蔑",
                    16: "哈哈",
                    17: "生病",
                    18: "脸红",
                    19: "流汗",
                    20: "笑哭",
                    21: "微笑",
                    22: "鼻涕",
                    23: "鬼脸",
                    24: "惊恐",
                    25: "抱歉",
                    26: "眩晕",
                    27: "自信笑",
                    28: "惊吓",
                    29: "叹气",
                    30: "委屈",
                    31: "可以",
                    32: "小鬼",
                    33: "妖怪",
                    34: "圣诞",
                    35: "小狗",
                    36: "小猪",
                    37: "小猫",
                    38: "大拇指",
                    39: "大拇指下",
                    40: "拳头",
                    41: "拳头上",
                    42: "耶",
                    43: "肌肉",
                    44: "鼓掌",
                    45: "向左",
                    46: "向上",
                    47: "向右",
                    48: "向下",
                    49: "OK",
                    50: "爱心",
                    51: "心碎",
                    52: "太阳",
                    53: "月亮",
                    54: "醒醒",
                    55: "雷电",
                    56: "乌云",
                    57: "红唇",
                    58: "玫瑰",
                    59: "咖啡",
                    60: "蛋糕",
                    61: "闹钟",
                    62: "啤酒",
                    63: "查询",
                    64: "手机",
                    65: "房屋",
                    66: "汽车",
                    67: "礼物",
                    68: "足球",
                    69: "炸弹",
                    70: "钻石",
                    71: "外星人",
                    72: "满分",
                    73: "现金",
                    74: "游戏",
                    75: "大便",
                    76: "SOS",
                    77: "睡觉",
                    78: "唱歌",
                    79: "雨伞",
                    80: "书本",
                    81: "祈祷",
                    82: "火箭",
                    83: "粥",
                    84: "西瓜"
                }
            }
        ]
    });
}

//格式化表情
window.EmojiParse = function (ele) {
    ele.emojiParse({
        icons: [
            {
                path: "http://" + window.location.host + "/im/Tookit/jQuery-emoji/img/emoji/",
                file: ".png",
                maxNum: 84,
                placeholder: "#{alias}#",
                alias: {
                    1: "呵呵",
                    2: "色",
                    3: "失望",
                    4: "惊讶",
                    5: "嘻嘻",
                    6: "亲亲",
                    7: "调皮",
                    8: "皱眉",
                    9: "失望",
                    10: "尴尬",
                    11: "大哭",
                    12: "吐舌",
                    13: "恼火",
                    14: "不开心",
                    15: "轻蔑",
                    16: "哈哈",
                    17: "生病",
                    18: "脸红",
                    19: "流汗",
                    20: "笑哭",
                    21: "微笑",
                    22: "鼻涕",
                    23: "鬼脸",
                    24: "惊恐",
                    25: "抱歉",
                    26: "眩晕",
                    27: "自信笑",
                    28: "惊吓",
                    29: "叹气",
                    30: "委屈",
                    31: "可以",
                    32: "小鬼",
                    33: "妖怪",
                    34: "圣诞",
                    35: "小狗",
                    36: "小猪",
                    37: "小猫",
                    38: "大拇指",
                    39: "大拇指下",
                    40: "拳头",
                    41: "拳头上",
                    42: "耶",
                    43: "肌肉",
                    44: "鼓掌",
                    45: "向左",
                    46: "向上",
                    47: "向右",
                    48: "向下",
                    49: "OK",
                    50: "爱心",
                    51: "心碎",
                    52: "太阳",
                    53: "月亮",
                    54: "醒醒",
                    55: "雷电",
                    56: "乌云",
                    57: "红唇",
                    58: "玫瑰",
                    59: "咖啡",
                    60: "蛋糕",
                    61: "闹钟",
                    62: "啤酒",
                    63: "查询",
                    64: "手机",
                    65: "房屋",
                    66: "汽车",
                    67: "礼物",
                    68: "足球",
                    69: "炸弹",
                    70: "钻石",
                    71: "外星人",
                    72: "满分",
                    73: "现金",
                    74: "游戏",
                    75: "大便",
                    76: "SOS",
                    77: "睡觉",
                    78: "唱歌",
                    79: "雨伞",
                    80: "书本",
                    81: "祈祷",
                    82: "火箭",
                    83: "粥",
                    84: "西瓜"
                }
            }
        ]
    });
}


//计算文件大小
window.formatFileSize = function (fileSize) {
    if (fileSize < 1024) {
        return fileSize + 'B';
    } else if (fileSize < (1024 * 1024)) {
        var temp = fileSize / 1024;
        temp = temp.toFixed(2);
        return temp + 'KB';
    } else if (fileSize < (1024 * 1024 * 1024)) {
        var temp = fileSize / (1024 * 1024);
        temp = temp.toFixed(2);
        return temp + 'MB';
    } else {
        var temp = fileSize / (1024 * 1024 * 1024);
        temp = temp.toFixed(2);
        return temp + 'GB';
    }
}

window.woohecc = {
    placeCaretAtEnd: function (el) {
        el.focus();
        if (typeof window.getSelection != "undefined"
            && typeof document.createRange != "undefined") {
            var range = document.createRange();
            range.selectNodeContents(el);
            range.collapse(false);
            var sel = window.getSelection();
            sel.removeAllRanges();
            sel.addRange(range);
        }
        else if (typeof document.body.createTextRange != "undefined") {
            var textRange = document.body.createTextRange();
            textRange.moveToElementText(el);
            textRange.collapse(false);
            textRange.select();
            set_focus();
        }
    }
}

/**
 * 可编辑Div 光标放到最后
 * @returns {} 
 */
window.set_focus = function () {
    el = document.getElementById('msgTextarea');
    //el=el[0];  //jquery 对象转dom对象
    el.focus();
    if ($.support.msie) {
        var range = document.selection.createRange();
        this.last = range;
        range.moveToElementText(el);
        range.select();
        document.selection.empty(); //取消选中
    }
    else {
        var range = document.createRange();
        range.selectNodeContents(el);
        range.collapse(false);
        var sel = window.getSelection();
        sel.removeAllRanges();
        sel.addRange(range);
    }
}


//高德地图初始化
//构造地图，构造聊页面中聊天记录中的地图
window.GaoDeMap = function (itemid, latitude) {
    new AMap.Map(itemid, {
        resizeEnable: true,
        zoom: 11,
        center: [latitude[1], latitude[0]],
    });
};


//复制到剪切板

window.copyToClip=function(content, cb) {
    var aux = document.createElement("input");
    aux.setAttribute("value", content);
    document.body.appendChild(aux);
    aux.select();
    document.execCommand("copy");
    document.body.removeChild(aux);
    if (cb) {
        cb();
    } else {
        alert('复制成功')
    }
}

window.InitHistory = function (uid, groupid) {


    var pageCount = 0;
    var pageNumber = 0;
    var msgSearch = "";
    var dateTime = (new Date()).toLocaleDateString();
    var hasNextDay = false;
    var hasBeforeDay = false;
    var imgPageNo = 1;
    var filePageNo = 1;


    layui.use('laydate', function () {
        var laydate = layui.laydate;

        //执行一个laydate实例
        laydate.render({
            elem: '#searchDatetime'
            , value: (new Date()).format('yyyy-MM-dd ')
            , done: function (value, date, endDate) {
                dateTime = value;
                GetHistoryChat(value, 1);
            }
        });
        window.setData = function (dt) {
            laydate.render({ elem: '#searchDatetime', value: dt });
        }
    });


    /*查看历史聊天记录*/
    $('#historyBtn').click(function () {
        if ($('#historyBtn').hasClass("cur")) {
            $('#historyBtn').removeClass("cur");
            $(".chatlist-tab a").removeClass('cur');
            $(".chatlist-tab a").eq(0).addClass('cur');
            $("#divbt").css('display', 'none');
            $("#historychatPanel").removeClass('hide');
            $("#historychatPanelPic").addClass('hide');
            $("#historychatPanelFile").addClass('hide');
            $("#historychatPanelPic").empty();
            $("#historychatPanelFile ul").empty();
        } else {
            $('#historyBtn').addClass("cur");
            $("#div0").addClass('show-video');
            $("#divbt").css('display', 'block');
            //第一次查询
            GetHistorChatLatest();
        }
    });

    $("#clearBtn").click(function () {
        $("#searchInput").val("");
        msgSearch = "";
    });
    $('#searchInput').on({
        keydown: function (e) {
            if (e.keyCode === 13) {
                SearchHistoryChat();
            }
        }
    });

    $("#pageFirst").click(function () {
        if ($("#pageFirst").hasClass("disabled")) return;
        GetHistoryChat(dateTime, 0);
    });
    $("#pageBefore").click(function () {
        if ($("#pageBefore").hasClass("disabled")) return;
        GetHistoryChat(dateTime, pageNumber - 1);
    });
    $("#pageNext").click(function () {
        if ($("#pageNext").hasClass("disabled")) return;
        GetHistoryChat(dateTime, pageNumber + 1);
    });
    $("#pageLast").click(function () {
        if ($("#pageLast").hasClass("disabled")) return;
        //GetHistoryChat(dateTime, pageCount);
        GetHistoryChat(dateTime, pageCount + 1);
    });

    /*点击搜索按钮 搜索对应的聊天记录*/
    $("#searchI").click(function () {
        $("#searchInput").val("");
        msgSearch = "";
        $("#searchDiv").slideToggle(100);
    });




    window.SearchHistoryChatByType = function (ele, type) {
        /*
            type=0 全部
            type=1 图片
            type=2 文件
        */
        $(".chatlist-tab a").removeClass('cur');
        $(ele).addClass('cur');
        if (type === 0) {
            $("#divbt").css('display', 'block');
            $("#historychatPanel").removeClass('hide');
            $("#historychatPanelPic").addClass('hide');
            $("#historychatPanelFile").addClass('hide');
            $("#historychatPanelPic").empty();
            $("#historychatPanelFile ul").empty();
        } else if (type === 1) {
            imgPageNo = 1;
            $("#divbt").css('display', 'none');
            $("#historychatPanelPic").removeClass('hide');
            $("#historychatPanel").addClass('hide');
            $("#historychatPanelFile").addClass('hide');
            $("#historychatPanelPic").empty();
            GetGroupHistoryChatImg(1);
        } else if (type === 2) {
            filePageNo = 1;
            $("#divbt").css('display', 'none');
            $("#historychatPanelFile").removeClass('hide');
            $("#historychatPanelPic").addClass('hide');
            $("#historychatPanel").addClass('hide');
            $("#historychatPanelFile ul").empty();
            $(".clickmore").remove();
            GetGroupHistoryChatFile(1);
        }
    }









    window.GetHistorChatLatest = function () {
        var load = layer.load();
        var strurl ='/im/main/GetGroupChatHistoryLatest';
        
        $.post(strurl, { fromuid: uid, touid: groupid }, function (result) {
            if (result.status === 0) {
                DoResult(result);
            } else {
                layer.msg('查询出错');
                console.log(result.msg);
            }
            layer.close(load);
        });
    }
    window.GetHistoryChat = function (valtime, pageNo) {
        if (valtime === "") return;
        var load = layer.load();
        var strurl = '/im/main/GetGroupChatHistory';
        
        if (pageNo !== 1 && pageNo > pageCount) {
            //跳页查询，查询有记录的下一天数据
            GetHistoryChatOverPageLast(valtime);
        } else if (pageNo === 0) {
            //跳页查询 查询有记录的上一天的数据
            GetHistoryChatOverPageBefore(valtime);
        } else {
            $.post(strurl, { fromuid: uid, touid: groupid, dt: valtime, pageNo: pageNo, msg: msgSearch }, function (result) {
                if (result.status === 0) {
                    DoResult(result);
                } else {
                    layer.msg('查询出错');
                    console.log(result.msg);
                }
                layer.close(load);
            });
        }
    }
    window.GetHistoryChatOverPageBefore = function (valtime) {
        if (valtime === "") return;
        var load = layer.load();
        var strurl = '/im/main/GetGroupChatHistoryOverPage';
        
        $.post(strurl, { fromuid: uid, touid: groupid, dt: valtime, addday: -1 }, function (result) {
            if (result.status === 0) {
                DoResult(result);
            } else {
                layer.msg('查询出错');
                console.log(result.msg);
            }
            layer.close(load);
        });
    }

    window.GetHistoryChatOverPageLast = function (valtime) {
        if (valtime === "") return;
        var load = layer.load();
        var strurl = '/im/main/GetGroupChatHistoryOverPage';
        
        $.post(strurl, { fromuid: uid, touid: groupid, dt: valtime, addday: 1 }, function (result) {
            if (result.status === 0) {
                DoResult(result);
            } else {
                layer.msg('查询出错');
                console.log(result.msg);
            }
            layer.close(load);
        });
    }

    /*点击加载更多图片、文件*/
    $(document).on('click', '.clickmore', function () {
        //隐藏加载更多
        $(".clickmore").remove();

        if ($("#historychatPanelFile").hasClass('hide')) {
            //查询图片

            GetGroupHistoryChatImg(imgPageNo);
        }
        else if ($("#historychatPanelPic").hasClass('hide')) {
            //查询文件

            GetGroupHistoryChatFile(filePageNo);
        }
    });


    /*查询人与人之间聊天记录的图片*/
    window.GetPersonalHistoryChatImg = function (pageNo) {
        var load = layer.load();
        $.post("/im/chat/GetImageMessage", { fromuid: uid, touid: groupid, isgroup: false, pageNo: pageNo }, function (result) {
            layer.close(load);
            if (result.Success && result.Content.length > 0) {
                DoResultImg(result);
            } else {
                if (pageNo === 1) {
                    var elestr = "<div class=\"nodata-tip isHidden\"><i class=\"iconfont icon-none\"></i><span class=\"text-none\">暂无内容</span></div>";
                    $("#historychatPanelPic").empty();
                    $("#historychatPanelPic").append(elestr);
                } else {
                    layer.msg("没有更多数据了", {
                        time: 1000
                    });
                }
            }
        });
    }

    /*查询群组内聊天记录的图片*/
    window.GetGroupHistoryChatImg = function (pageNo) {
        var load = layer.load();
        $.post("/im/chat/GetImageMessage", { fromuid: uid, touid: groupid, isgroup: true, pageNo: pageNo }, function (result) {
            layer.close(load);
            if (result.Success && result.Content.length > 0) {
                DoResultImg(result);
            } else {
                if (pageNo === 1) {
                    var elestr = "<div class=\"nodata-tip isHidden\"><i class=\"iconfont icon-none\"></i><span class=\"text-none\">暂无内容</span></div>";
                    $("#historychatPanelPic").empty();
                    $("#historychatPanelPic").append(elestr);
                } else {
                    layer.msg("没有更多数据了", {
                        time: 1000
                    });
                }
            }
        });
    }
    /*查询人与人聊天记录的文件*/
    window.GetPersonalHistoryChatFile = function (pageNo) {
        var load = layer.load();
        $.post("/im/chat/GetFileMessage", { fromuid: uid, touid: groupid, isgroup: false, pageNo: pageNo }, function (result) {
            layer.close(load);
            if (result.Success && result.Content.length > 0) {
                DoResultFile(result);
            } else {
                if (pageNo === 1) {
                    var elestr = "<div class=\"nodata-tip isHidden\"><i class=\"iconfont icon-none\"></i><span class=\"text-none\">暂无内容</span></div>";
                    $("#historychatPanelFile ul").empty();
                    $("#historychatPanelFile").prepend(elestr);
                } else {
                    layer.msg("没有更多数据了", {
                        time: 1000
                    });
                }
            }
        });
    }

    /*查询群组内聊天记录的文件*/
    window.GetGroupHistoryChatFile = function (pageNo) {
        var load = layer.load();
        $.post("/im/chat/GetFileMessage", { fromuid: uid, touid: groupid, isgroup: true, pageNo: pageNo }, function (result) {
            layer.close(load);
            if (result.Success && result.Content.length > 0) {
                DoResultFile(result);
            } else {
                if (pageNo === 1) {
                    var elestr = "<div class=\"nodata-tip isHidden\"><i class=\"iconfont icon-none\"></i><span class=\"text-none\">暂无内容</span></div>";
                    $("#historychatPanelFile ul").empty();
                    $("#historychatPanelFile").prepend(elestr);
                } else {
                    layer.msg("没有更多数据了", {
                        time: 1000
                    });
                }
            }
        });
    }

    /*点击搜索按钮*/
    window.SearchHistoryChat = function () {
        msgSearch = $("#searchInput").val();
        if ($.trim(msgSearch) === "") {
            return;
        }
        GetHistoryChat(dateTime, 1);
    }

    /*初始化 上一页下一页按钮 是否可用*/
    window.InitNextBtn = function () {
        $("#pageFirst").addClass("disabled");
        $("#pageBefore").addClass("disabled");
        $("#pageNext").addClass("disabled");
        $("#pageLast").addClass("disabled");
        if (pageNumber < pageCount) {
            $("#pageNext").removeClass("disabled");
        }
        if (pageNumber !== 1 && pageCount > 1) {
            $("#pageBefore").removeClass("disabled");
        }
        if (hasNextDay) {
            $("#pageNext").removeClass("disabled");
            $("#pageLast").removeClass("disabled");
        }
        if (hasBeforeDay) {
            $("#pageBefore").removeClass("disabled");
            $("#pageFirst").removeClass("disabled");
        }
    }
    /*抽取出来的 公共部分*/
    window.DoResult = function (result) {
        var elestr = "";
        pageCount = result.pageCount;
        pageNumber = result.currentPage;
        hasNextDay = result.hasNextDay;
        hasBeforeDay = result.hasBeforeDay;
        dateTime = result.datetime;
        setData(dateTime);
        if (result.data.length !== 0) {
            for (var i = 0; i < result.data.length; i++) {
                var item = result.data[i];

                if (item.MsgType === "4") {
                    /*语音类消息*/
                    elestr += "<div class=\"clp-item\"><p class=\"color-23 font-s12\"><span class=\"uname\">" + item.FromUserTrueName + "</span><span>" + item.CreateDate + "</span></p>" + item.Msg + "</div>";
                } else if (item.MsgType === "5") {
                    /*地图签到类消息*/
                    elestr += "<div class=\"clp-item\"><p class=\"color-23 font-s12\"><span class=\"uname\">" + item.FromUserTrueName + "</span><span>" + item.CreateDate + "</span></p>" + item.Msg + "</div>";
                } else {
                    elestr += "<div class=\"clp-item\"><p class=\"color-23 font-s12\"><span class=\"uname\">" + item.FromUserTrueName + "</span><span>" + item.CreateDate + "</span></p><p class=\"msg\">" + item.Msg + "</p><i class=\"iconfont icon-collection user-fav-history\" title=\"收藏\" data-msgid='" + item.ID + "'></i></div>";
                }
            }
            $("#historychatPanel").empty();
            $("#historychatPanel").append(elestr);
            EmojiParse($("#historychatPanel div .msg"))
            GaodeMapHistory();
            InitNextBtn();
        } else {
            elestr = "<div class=\"nodata-tip isHidden\"><i class=\"iconfont icon-none\"></i><span class=\"text-none\">暂无内容</span></div>";
            $("#historychatPanel").empty();
            $("#historychatPanel").append(elestr);
            pageCount = 0;
            pageNumber = 0;
        }
        InitNextBtn();
    }

    window.DoResultImg = function (result) {
        var elestr = "";
        for (var i = 0; i < result.Content.length; i++) {
            var item = result.Content[i];
            var dayid = item.msgtime.substring(0, 10);//以天为单位groupby
            var tele = $("#historychatPanelPic ul[data-id='" + dayid + "']");
            if (tele.length !== 0) {
                if (item.msgtype === 1) {
                    elestr = "<li><img class=\"imgA\" src = \"" + item.msg + "\" title=\"" + item.msgtime + " " + item.fromrealname + "\"/></li>";
                } else if (item.msgtype === 3) {
                    $.each(item.imglist, function (n, value) {
                        elestr += "<li><img class=\"imgA\" src = \"" + value + "\" title=\"" + item.msgtime + " " + item.fromrealname + "\"/></li>";
                    });
                }
                tele.prepend(elestr);
            } else {
                if (item.msgtype === 1) {
                    elestr = "<li><img class=\"imgA\" src = \"" + item.msg + "\" title=\"" + item.msgtime + " " + item.fromrealname + "\"/></li>";
                } else if (item.msgtype === 3) {
                    $.each(item.imglist, function (n, value) {
                        elestr += "<li><img class=\"imgA\" src = \"" + value + "\" title=\"" + item.msgtime + " " + item.fromrealname + "\"/></li>";
                    });
                }

                var elestrK = "<p><i class=\"iconfont icon-historyTime mr10 vam\"></i>" + dayid + "</p><ul data-id=\"" + dayid + "\" class=\"clearfix\">" + elestr + "<ul>";
                $("#historychatPanelPic").prepend(elestrK);
            }
        }
        if (result.Content.length > 0 && result.Total > result.Count) {
            $("#historychatPanelPic").prepend("<p class=\"clickmore\">点击加载更多</p>");
            imgPageNo++;
        }
    }
    //历史记录中的【文件】类型数据
    window.DoResultFile = function (result) {
        var elestr = "";
        for (var i = 0; i < result.Content.length; i++) {
            var item = result.Content[i];
            var ext = GetClassName(item.filename.substring(item.filename.lastIndexOf(".") + 1));
            //elestr = "<li class=\"cl-file-l\" title=\"" + item.filename + " " + item.fromrealname + "\"><a href=\"" + item.msg + "\"><i class=\"layui-icon\"></i><div class=\"cl-file-r\"><p class=\"mb5\">" + item.filename + "</p><p class=\"font-s12 color-9\"><span class=\"mr10\">" + item.msgtime + "</span></p></div></a></li>";
            elestr = "<li class=\"cl-file-l\" title=\"" + item.filename + " " + item.fromrealname + "\"><a href=\"" + item.msg + "\"><svg class=\"icon\" aria-hidden=\"true\"><use xlink:href=\"" + ext + "\"></use></svg><div class=\"cl-file-r\"><p class=\"mb5\">" + item.filename + "</p><p class=\"font-s12 color-9\"><span class=\"mr10\">" + item.msgtime + "</span></p></div></a></li>";
            $("#historychatPanelFile ul").prepend(elestr);
        }
        if (result.Content.length > 0 && result.Total > result.Count) {
            $("#historychatPanelFile").prepend("<p class=\"clickmore\">点击加载更多</p>");
            filePageNo++;
        }
    }
    window.GetClassName = function (fileCode) {
        var currentType = "." + fileCode;
        var key = this.calcFileTypeBySuffix(currentType);
        return "#icon-" + key;
    }

    var FILE_SUFFIX_DICT = {
        "doc": [".doc", ".docx"],
        "img": [".jpg", ".jpeg", ".png", ".tif", ".bmp", ".gif"],
        "ppt": [".ppt", ".pptx"],
        "pdf": [".pdf", ".caj", ".nh", ".kdh", ".tab"],
        "excel": [".xls", ".xlsx"]
    }
    window.calcFileTypeBySuffix = function (suffix) {
        if (suffix == null) {
            suffix = "";
        }
        var dict = FILE_SUFFIX_DICT;
        var isExist = false;
        var hasKey = "";
        $.each(dict, function (index, key) {
            var list = dict[index];
            isExist = list.some(function (item) {
                return item === suffix.toLowerCase() && (hasKey = index);
            });
            if (isExist) return false;//跳出循环
        });
        return hasKey || "other";
    }


    window.GaodeMapHistory = function () {
        var mapsinhistory = $(".mapi-history");
        $.each(mapsinhistory, function (n, item) {
            var itemid = $(item).attr('id');
            var latitude = $(item).attr('map-data').split(',');
            GaoDeMap(itemid, latitude);
        });

    }






    //导出按钮
    $(".chat-export").click(function (event) {

        GetChatHistory(dateTime + ' 00:00:00', dateTime + ' 23:59:59');

    });



    //右侧历史记录中的收藏click
    $(document).on('click', '.user-fav-history', function () {
        var msgid = $(this).attr("data-msgid");
        $.ajax({
            url: '/imwebapi/api/MainApi/AddUserFavorites',
            type: 'POST',
            headers: {
                'ignore-identity': "true"
            },
            data: {
                userId: uid,
                type: 1,
                content: '',
                cid: msgid,
            },
            dataType: 'json',
            success: function (result) {
                if (result.Success) {
                    layer.msg('收藏成功');
                } else {
                    layer.msg('收藏失败');
                }

            },
            error: function (xhr, textStatus) {

            }
        })
    });

    


}


