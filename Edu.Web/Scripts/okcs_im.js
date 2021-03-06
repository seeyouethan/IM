
//预览图片方法，传入的json是layer.photos中的json格式
window.layerPhotosNew = function (json) {
    json = JSON.parse(JSON.stringify(json));
    layer.photos({
        photos: json,
        full:false,
        shift: 5,
        id: 'layerimg', area:'auto',
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
window.ShowPhotos = function (json) {
    json = JSON.parse(JSON.stringify(json));
    let image = new Image();
    image.src = json.data[0].src;
    // 图片先加载完，才可以得到图片宽度和高度
    image.onload = function () {
        layer.photos({
            photos: json,
            full: false,
            shift: 5,
            id: 'layerimg', area: [image.width,image.height],
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
}