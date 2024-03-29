
window.getPasteImage = function (e) {
    return e.clipboardData && e.clipboardData.items && e.clipboardData.items.length == 1 && /^image\//.test(e.clipboardData.items[0].type) ? e.clipboardData.items : null;
}


window.upload = function (file,touid, callback) {
    //console.log(file);
    var formData = new FormData();
    formData.append('uploadimgname', file);
    formData.append('touid', touid);
    $.ajax({
        url: '/imwebapi/Home/Upload',
        data: formData,
        type: 'POST',
        contentType: false,
        processData: false,
        success: function (data) {
            if (callback) {
                callback(data.Content);
            }
        }
    });
}

window.listenPasteImage = function (e,touid, callback) {
    var hasImg = false, items;
    //获取粘贴板文件列表或者拖放文件列表
    items = getPasteImage(e);
    if (items) {
        var len = items.length,
            file;
        while (len--) {
            file = items[len];
            if (file.getAsFile) file = file.getAsFile();
            if (file && file.size > 0) {
                upload(file,touid, function (result) {
                    if (callback) {
                        //console.log("开始执行回调函数-->",data);
                        callback(result);
                    }
                });
                hasImg = true;
            }
        }
        hasImg && e.preventDefault();
    }
}


/*上传成功后 将图片添加到可编辑的div中*/
window.AfterUploadImg = function (result) {
    if (result.code === 0) {
        var imgurl = "http://"+window.location.host+"/im" + result.data["src"];
        var imgele = "<img src=\"" + imgurl + "\" onclick=\"ShowImg($(this))\"/>";
        $("#msgTextarea").append(imgele);
        set_focus();
    }
}

window.CreateImgElement = function (url) {
    var imgele = "<img class=\"imgA\" src=\"" + url + "\"/>";
    $("#msgTextarea").append(imgele);
    set_focus();
}



/*使用layerphotos弹出显示图片*/
window.ShowImg = function (ele) {
    if (ele.hasClass("emoji_icon")) return;
    var imgurl = ele.attr('src');
    if (imgurl) {
        var re = new RegExp("&amp;", "g"); //定义正则表达式
        var imgulrResult = imgurl.replace(re, "&");
        var resultJson = { "status": 1, "msg": "", "title": "", "id": 1, "start": 0, "data": [{ "alt": "", "pid": "", "src": imgulrResult, "thumb": "" }] }
        
        //parent.parent.ShowPhotos(resultJson);
        if (parent.parent.ShowPhotos) {
            parent.parent.ShowPhotos(resultJson);
        } else {
            parent.parent.layerPhotosNew(resultJson);
        }
    }
    //$.getJSON('/im/Common/GetImg?imgurl=' + imgurl, function (json) {
    //    parent.parent.layerPhotosNew(json);
    //});
}