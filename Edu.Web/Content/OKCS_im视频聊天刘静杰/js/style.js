
 // 图片轮播
window.onload = function(){
        var mytimer = null;
        //复制一倍结点
        $("#wufeng ul,#wufeng01 ul").html( $("#wufeng ul,#wufeng01 ul").html() + $("#wufeng ul,#wufeng01 ul").html() );
        var long1 = $("#wufeng ul li,#wufeng01 ul li").eq($("#wufeng ul li,#wufeng01 ul li").length / 2).offset().left;
        var long2 = $("#wufeng ul,#wufeng01 ul").offset().left;
        var long = long2 - long1;
        
        dongdong();
        function dongdong(){
            window.clearInterval(mytimer);
            mytimer = window.setInterval(
                function(){
                    if(parseInt($("#wufeng ul,#wufeng01 ul").css("left")) != long + 10){
                        $("#wufeng ul,#wufeng01 ul").css("left","-=1px");
                    }else{
                        $("#wufeng ul,#wufeng01 ul").css("left","0");
                    }
                }
            ,10);
        }

        $("#wufeng ul li,#wufeng01 ul li").mouseenter(
            function(){
                window.clearInterval(mytimer);
            }
        );

        $("#wufeng ul li,#wufeng01 ul li").mouseleave(
            function(){
                dongdong();
            }
        );
     }


