import layer from './layer.js';

window.StartIM=function() {
    var height = $(window).height() - 65+'px';
    layer.open({
        type: 2,
        id: 'okcsim',
        shade: 0,
        closeBtn: 0,
        title: false, 
        area: ['0', height],
        offset: 'rb',
        shadeClose: false, 
        content: "http://" + window.location.host + "/im/main/index",
        success: function (layero, index) {
            /*查询是否有未读消息 如果没有未读消息 则去掉红点*/
            $.ajax({
                type: 'POST',
                url: '/im/main/QueryNotReadMsg',
                dataType : 'json',
                success: function(data) {
                    var eleTop = $("#okcsimIcon");
                    if (data.r) {
                        eleTop.addClass("new-circle"); 
                    } 
                }
            });
        }
    });
    return;
}


window.IM = function() {
    var height = $(window).height() - 65+'px';
    var ele = $("#okcsim");
    if (ele.length === 0) {
        layer.open({
            type: 2,
            id: 'okcsim',
            shade: 0,
            closeBtn: 0,
            title: false, 
            area: ['350px', height],
            offset: 'rb',
            shadeClose: false, 
            content: "http://" + window.location.host + "/im/main/index",
            success: function (layero, index) {
                var eleParent = $('#okcsim').parent();
                eleParent.addClass("hasshowed");
            }
        });
        return;
    } else {
        var eleP = parent.$('#okcsim').parent();
        var left;
        if (eleP.width()===0) {
            eleP.width(350);
            left = $(window).width() - 350;
            eleP.animate({ left: left });
            eleP.addClass("hasshowed");
        } else {
            if (eleP.hasClass("hasshowed")) {
                left = eleP.position().left + 350;
                eleP.animate({ left: left });
                eleP.removeClass("hasshowed");
                eleP.hide(100);
            } else {
                eleP.show();
                left = $(window).width() - 350;
                eleP.animate({ left: left });
                eleP.addClass("hasshowed");
            }
        }
    }
}

window.UpdateGroup = function(){
    var ele = window.document.getElementById("okcsim");
    var iframeele = ele.children[0];
    iframeele.contentWindow.UpdateWorkGroup();
}

window.layerPhotos = function(json){
    json = JSON.parse(JSON.stringify(json));
    layer.photos({
        photos: json,
        shift: 5,
        id:'layerimg',
        success: function (layero, index) {
            console.log("layer.photos  success");
            var elestr = "<span class=\"layui-layer-setwin\"><a class=\"layui-layer-ico layui-layer-close layui-layer-close1\" href=\"javascript:;\"></a></span>";
            var ele = $('#layerimg').children('div').eq(0);
            ele.append(elestr);
            var eleClose = ele.children("span").eq(0).children('a').eq(0);
            eleClose.on('click', function () {
                ele.parent().parent().prev().remove();
                ele.parent().parent().remove();
            });

        }
    });
}