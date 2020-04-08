﻿
//emoji初始化
window.InitImEmoji = function () {

    $("#emojiBtn").click(function () {
        $("#emojiBtn").unbind();
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
    });
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
