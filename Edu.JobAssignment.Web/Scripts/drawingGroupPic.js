function drawingGroupPic(data, callback) { //需要组合的头像个数
    var groupPic = {
        case1: {
            size: [
                [1, 1]
            ],
            position: [
                [0, 0]
            ]
        },
        case2: {
            size: [
                [1 / 2, 1],
                [1 / 2, 1]
            ],
            position: [
                [0, 0],
                [1 / 2, 0]
            ]
        },
        case3: {
            size: [
                [1, 1 / 2],
                [1 / 2, 1 / 2],
                [1 / 2, 1 / 2]
            ],
            position: [
                [0, 0],
                [0, 1 / 2],
                [1 / 2, 1 / 2]
            ]
        },
        case4: {
            size: [
                [1 / 2, 1 / 2],
                [1 / 2, 1 / 2],
                [1 / 2, 1 / 2],
                [1 / 2, 1 / 2]
            ],
            position: [
                [0, 0],
                [1 / 2, 0],
                [0, 1 / 2],
                [1 / 2, 1 / 2]
            ]
        },
        case5: {
            size: 1 / 3,
            position: [
                [1 / 2, 1 / 2],
                [3 / 2, 1 / 2],
                [0, 3 / 2],
                [1, 3 / 2],
                [2, 3 / 2]
            ]
        },
        case6: {
            size: 1 / 3,
            position: [
                [0, 1 / 2],
                [1, 1 / 2],
                [2, 1 / 2],
                [0, 3 / 2],
                [1, 3 / 2],
                [2, 3 / 2]
            ]
        },
        case7: {
            size: 1 / 3,
            position: [
                [1, 0],
                [0, 1],
                [1, 1],
                [2, 1],
                [0, 2],
                [1, 2],
                [2, 2]
            ]
        },
        case8: {
            size: 1 / 3,
            position: [
                [1 / 2, 0],
                [3 / 2, 0],
                [0, 1],
                [1, 1],
                [2, 1],
                [0, 2],
                [1, 2],
                [2, 2]
            ]
        },
        case9: {
            size: 1 / 3,
            position: [
                [0, 0],
                [1, 0],
                [2, 0],
                [0, 1],
                [1, 1],
                [2, 1],
                [0, 2],
                [1, 2],
                [2, 2]
            ]
        },
    }
    var c = document.createElement('canvas'); //创建一个canvas
    var ctx = c.getContext('2d');
    // let container = document.getElementById('container');
    // container.innerHTML = '';
    // container.append(c);
    var len = data.length < 5 ? data.length : 4; //获取需要组合的头像图片的张数最多4张
    var x, y, h, w; //初始化需要组合头像的左上角坐标和长宽
    c.width = 300; //定义canvas画布的宽度
    c.height = 300; //定义canvas画布的高度
    ctx.rect(0, 0, c.width, c.height); //画矩形
    ctx.fillStyle = '#fff'; //设置矩形颜色
    ctx.fill(); //关闭并填充该路径


    function drawing(n) { //画第几个
        if (n < len) { //当n<需要组合头像图片个数时就不再重复这个函数
            w = groupPic['case' + len].size[n][0] * c.width;
            h = groupPic['case' + len].size[n][1] * c.width;
            x = groupPic['case' + len].position[n][0] * c.width, y = groupPic['case' + len].position[n][1] * c.width
            var img = new Image(); //创建一个image对象
            img.crossOrigin = 'Anonymous'; //解决跨域
            img.src = data[n]; //将图片地址赋值给image对象的src
            img.onload = function () { //图片预加载
                ctx.drawImage(img, x, y, w, h);
                drawing(n + 1);
            };
            img.onerror = function () {
                drawing(n + 1);
            }
        } else {
            callback && callback(c.toDataURL('image/jpeg', 0.5));
        }
    }
    drawing(0);
}