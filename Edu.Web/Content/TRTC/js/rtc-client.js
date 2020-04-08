/* eslint-disable require-jsdoc */

class RtcClient {
  constructor(options) {
    this.sdkAppId_ = options.sdkAppId;
    this.userId_ = options.userId;
    this.userSig_ = options.userSig;
    this.roomId_ = options.roomId;

    this.isJoined_ = false;
    this.isPublished_ = false;
    this.localStream_ = null;
    this.remoteStreams_ = [];

    // check if browser is compatible with TRTC
    TRTC.checkSystemRequirements().then(result => {
      if (!result) {
        alert('Your browser is not compatible with TRTC! Please download Chrome M72+');
      }
    });
  }

  async join() {
    if (this.isJoined_) {
      console.warn('duplicate RtcClient.join() observed');
      return;
    }

    // create a client for RtcClient
    this.client_ = TRTC.createClient({
      mode: 'videoCall', // 实时通话模式
      sdkAppId: this.sdkAppId_,
      userId: this.userId_,
      userSig: this.userSig_
    });

    // 处理 client 事件
    this.handleEvents();
    try {
      // join the room
      await this.client_.join({ roomId: this.roomId_ });
      console.log('join room success');
      //Toast.notify('进房成功！');
      this.isJoined_ = true;
    } catch (error) {
      console.error('failed to join room because: ' + error);
      alert(
        '加入房间失败！'
      );
      //Toast.error('进房错误！');
      return;
    }
  }

  async leave() {
    if (!this.isJoined_) {
      console.warn('leave() - leave without join()d observed');
      //Toast.error('请先加入房间！');
      return;
    }

    if (this.isPublished_) {
      // ensure the local stream has been unpublished before leaving.
      await this.unpublish(true);
    }

    try {
      // leave the room
      await this.client_.leave();
      //Toast.notify('退房成功！');
      this.isJoined_ = false;

    } catch (error) {
      console.error('failed to leave the room because ' + error);
      location.reload();
    } finally {
      // 停止本地流，关闭本地流内部的音视频播放器
      this.localStream_.stop();
      // 关闭本地流，释放摄像头和麦克风访问权限
      this.localStream_.close();
      this.localStream_ = null;
    }
  }

  async publish(profile) {
      var selfRtc = this;
      var uid = this.localStream_.getUserId();
    if (!this.isJoined_) {
      //Toast.error('请先加入房间再点击开始推流！');
      await this.join();
      this.publish();
      console.warn('publish() - please join() firstly');
      return;
    }
    if (this.isPublished_) {
        this.unpublish(true);
        // 停止本地流，关闭本地流内部的音视频播放器
        this.localStream_.stop();
        // 关闭本地流，释放摄像头和麦克风访问权限
        this.localStream_.close();
        console.log("停止前一个推流");
        //console.warn('duplicate RtcClient.join() observed');
        //return;
    }

    try {
        // 采集摄像头和麦克风视频流
        var camera_config= { audio: true, video: true };
        var video_profile='480p'
        if(profile){
            video_profile=profile;
        }
        await this.createLocalStream(camera_config,video_profile);
      //Toast.info('摄像头及麦克风采集成功！');
      console.log('createLocalStream with audio/video success');
    } catch (error) {
      console.error('createLocalStream with audio/video failed: ' + error);
      alert(
        '无法获取麦克风/摄像头/分享桌面权限，请确认已经连接麦克风/摄像头并授予其访问权限，然后刷新浏览器'
      );
    }

    try {
        this.localStream_.on('player-state-changed', event => {
            console.log(`local stream ${event.type} player is ${event.state}`);
            if (event.type === 'video' && event.state === 'PLAYING') {
                // dismiss the remote user UI placeholderSTOPPED
            } else if (event.type === 'video' && event.state === 'STOPPED') {
                // show the remote user UI placeholder                
                RemovePlayerElement(uid);
                selfRtc.unpublish();
            }
        });

      //添加自己的播放视频的容器
      var width = $("#videos-container").width();
      if (getFocusedStatus()) {
          //主持人状态
          width = width - 16;
      } else {
          //平铺状态
          width = width / 2 - 14;
      }
      var config = {
          uid:uid,//当前用户的uid
          realName:getTrueNameByUserid(uid),//真实姓名      
          videoid:selfRtc.localStream_.getId(),//trtc.js生成的video的id          
          width:width,//宽度
          muteAudio:
              function(){
                  selfRtc.localStream_.muteAudio()//静音方法
              },
          unmuteAudio:
              function(){                  
                  selfRtc.localStream_.unmuteAudio()//关闭静音方法
              },
          muteVideo:
              function(){
                  selfRtc.localStream_.muteVideo()//静音视频方法
              },          
          unmuteVideo:
              function(){
                  selfRtc.localStream_.unmuteVideo()//关闭静音视频方法
              },
          gdVideo:
              function(){
                  selfRtc.unpublish()//挂断方法
              },
          gd:true,//是否有挂断标签
      };
      

      CreatePlayerElement(config);

        // 在id为 uid\ class='media-box' 的 div 容器上播放本地音视频
      this.localStream_.play(uid);
      // 发布本地流
      await this.client_.publish(this.localStream_);
      //Toast.info('发布本地流成功！');
      this.isPublished_ = true;
      //在数据库中添加缓存，表示当前用户正在直播
      SetOnLiveUser(uid);
        //给video标签添加单击和双击事件

      setTimeout(function(){
          var videoElement = document.getElementById("video_" + selfRtc.localStream_.getId())
          if(videoElement){
              //单击
              videoElement.onclick = function () {
                  SetFocus(selfRtc.localStream_.getUserId());
              };
              //双击全屏
              videoElement.ondblclick = function () {
                  launchFullscreen(videoElement);
              };
          }         
      },3000);
     

    } catch (error) {
      console.error('failed to publish local stream ' + error);
      //Toast.error('发布本地流失败！');
      this.isPublished_ = false;

      var ele = $("#videos-container div[data-uid='" + selfRtc.localStream_.getUserId() + "']");
      if (ele.length > 0) {
          ele.remove();
      }
    }
  }

  async unpublish(isLeaving) {
    if (!this.isJoined_) {
      console.warn('unpublish() - please join() firstly');
      //Toast.error('请先加入房间再停止推流！');
      return;
    }
    if (!this.isPublished_) {
      console.warn('RtcClient.unpublish() called but not published yet');
      //Toast.error('当前尚未发布本地流！');
      return;
    }

    try {
      // 停止发布本地流
      await this.client_.unpublish(this.localStream_);
      this.isPublished_ = false;
        //去掉video以及相关标签
      var uid=this.localStream_.getUserId();
      RemovePlayerElement(uid);
      //Toast.info('停止发布本地流成功！');
      RemoveOnLiveUser(uid)
    } catch (error) {
      console.error('failed to unpublish local stream because ' + error);
      //Toast.error('停止发布本地流失败！');
      if (!isLeaving) {
        console.warn('leaving the room because unpublish failure observed');
        //Toast.error('停止发布本地流失败，退出房间！');
        this.leave();
      }
    }
  }

  async createLocalStream(options,video_profile) {
    this.localStream_ = TRTC.createStream({
      audio: options.audio, // 采集麦克风
      video: options.video, // 采集摄像头
      userId: this.userId_
      //cameraId: getCameraId(),
      // microphoneId: getMicrophoneId()
    });
      // 设置视频分辨率帧率和码率
    
    this.localStream_.setVideoProfile(video_profile);

    await this.localStream_.initialize();
  }

  async createLocalScreenStream(options) {
      
    this.localStream_ = TRTC.createStream({ audio: true, screen: true ,
      userId: this.userId_});

    await this.localStream_.initialize();
  }

  handleEvents() {
    var selfRtc = this;
    // 处理 client 错误事件，错误均为不可恢复错误，建议提示用户后刷新页面
    this.client_.on('error', err => {
      console.error(err);
      //alert(err);
      //Toast.error('客户端错误：' + err);
      // location.reload();
    });

    // 处理用户被踢事件，通常是因为房间内有同名用户引起，这种问题一般是应用层逻辑错误引起的
    // 应用层请尽量使用不同用户ID进房
    this.client_.on('client-banned', err => {
      console.error('client has been banned for ' + err);
      //Toast.error('用户被踢出房间！');
      // location.reload();
    });

    // 远端用户进房通知 - 仅限主动推流用户
    this.client_.on('peer-join', evt => {
      const userId = evt.userId;
      console.log('peer-join ' + userId);
      //Toast.notify('远端用户进房 - ' + userId);
    });
    // 远端用户退房通知 - 仅限主动推流用户
    this.client_.on('peer-leave', evt => {
      const userId = evt.userId;
      console.log('peer-leave ' + userId);
        //Toast.notify('远端用户退房 - ' + userId);
      RemoveOnLiveUser(userId);
    });

    // 处理远端流增加事件
    this.client_.on('stream-added', evt => {
      const remoteStream = evt.stream;
      const id = remoteStream.getId();
      const userId = remoteStream.getUserId();
      console.log(`remote stream added: [${userId}] ID: ${id} type: ${remoteStream.getType()}`);
      //Toast.info('远端流增加 - ' + userId);
      console.log('subscribe to this remote stream');
      // 远端流默认已订阅所有音视频，此处可指定只订阅音频或者音视频，不能仅订阅视频。
      // 如果不想观看该路远端流，可调用 this.client_.unsubscribe(remoteStream) 取消订阅
      this.client_.subscribe(remoteStream);
    });

    // 远端流订阅成功事件
    this.client_.on('stream-subscribed', evt => {
      const remoteStream = evt.stream;
      const id = remoteStream.getId();
      this.remoteStreams_.push(remoteStream);

      //添加其他人的播放视频的容器
      var uid = remoteStream.getUserId();
      var width = $("#videos-container").width();
      if (getFocusedStatus()) {
        //主持人状态
        width = width / 3 - 18;
      } else {
        //平铺状态
        width = width / 2 - 14;
      }
      var config = {
          uid:uid,//当前用户的uid
          realName:getTrueNameByUserid(uid),//trtc.js生成的video的id          
          videoid:id,//trtc.js生成的video的id          
          width:width,//宽度
          muteAudio:function(){
              document.getElementById("audio_" + remoteStream.getId()).pause()//静音方法
          },
          unmuteAudio:
              function(){
                  document.getElementById("audio_" + remoteStream.getId()).play()//关闭静音方法
              },
          muteVideo:
              function(){
                  document.getElementById("video_" + remoteStream.getId()).pause()//静音视频方法
              },
          unmuteVideo:
              function(){
                  document.getElementById("video_" + remoteStream.getId()).play()//关闭静音视频方法
              },
          gdVideo:null,//挂断方法
          gd:false,//是否有挂断标签
      };
      CreatePlayerElement(config);

      remoteStream.play(uid);
      console.log('stream-subscribed ID: ', uid);
      //Toast.info('远端流订阅成功 - ' + remoteStream.getUserId());

      setTimeout(function () {
        //添加单击和双击事件
        var videoElement = document.getElementById("video_" + remoteStream.getId())
        if (videoElement) {
          //单击
          videoElement.onclick = function () {
              SetFocus(remoteStream.getUserId());
          };

          //双击全屏
          videoElement.ondblclick = function () {
            launchFullscreen(videoElement);
          };
        }
      }, 3000)
    });

    // 处理远端流被删除事件
    this.client_.on('stream-removed', evt => {
      const remoteStream = evt.stream;
      const id = remoteStream.getId();
      // 关闭远端流内部的音视频播放器
      remoteStream.stop();
      this.remoteStreams_ = this.remoteStreams_.filter(stream => {
        return stream.getId() !== id;
      });
      //removeView(id);
      RemovePlayerElement(remoteStream.getUserId());
      
      console.log(`stream-removed ID: ${id}  type: ${remoteStream.getType()}`);
      //Toast.info('远端流删除 - ' + remoteStream.getUserId());
    });

    // 处理远端流更新事件，在音视频通话过程中，远端流音频或视频可能会有更新
    this.client_.on('stream-updated', evt => {
      const remoteStream = evt.stream;
      console.log(
        'type: ' +
        remoteStream.getType() +
        ' stream-updated hasAudio: ' +
        remoteStream.hasAudio() +
        ' hasVideo: ' +
        remoteStream.hasVideo()
      );
      //Toast.info('远端流更新！');
    });

    // 远端流音频或视频mute状态通知
    this.client_.on('mute-audio', evt => {
      console.log(evt.userId + ' mute audio');
    });
    this.client_.on('unmute-audio', evt => {
      console.log(evt.userId + ' unmute audio');
    });
    this.client_.on('mute-video', evt => {
      console.log(evt.userId + ' mute video');
    });
    this.client_.on('unmute-video', evt => {
      console.log(evt.userId + ' unmute video');
    });

    // 信令通道连接状态通知
    this.client_.on('connection-state-changed', evt => {
      console.log(`RtcClient state changed to ${evt.state} from ${evt.prevState}`);
    });
  }


    //分享屏幕，这个分享没有音频，这个是TRTC的直接分享桌面的方式，目前还不支持音频
    //从分享摄像头，切换到分享屏幕，采用的是替换流的方式，所以是有音频的
  async publishScreen() {
      var selfRtc = this;
      var uid = this.localStream_.getUserId();
    if (!this.isJoined_) {
      //Toast.error('请先加入房间再点击开始推流！');
      console.warn('publish() - please join() firstly');
      return;
    }
    if (this.isPublished_) {
      console.warn('duplicate RtcClient.publish() observed');
      //Toast.error('当前正在推流！');
      return;
    }

    try {
      // 采集桌面
      await this.createLocalScreenStream();
      //Toast.info('摄像头及麦克风采集成功！');
      console.log('createLocalScreenStream with audio/video success');
    } catch (error) {
        console.error('createLocalScreenStream with audio/video failed: ' + error);
      alert(
        //'无法获取麦克风/摄像头/分享桌面权限，请确认已经连接麦克风/摄像头并授予其访问权限!'        
        '请确认已连接摄像头和麦克风，是否被其他设备占用，请刷新浏览器'
      );
    }

    try {

      this.localStream_.on('player-state-changed', event => {
        console.log(`local stream ${event.type} player is ${event.state}`);
        if (event.type === 'video' && event.state === 'PLAYING') {
          // dismiss the remote user UI placeholder
        } else if (event.type === 'video' && event.state === 'STOPPED') {
            // show the remote user UI placeholder
            RemovePlayerElement(uid);
            selfRtc.unpublish();
        }
      });

      //添加自己的播放视频的容器
      var width = $("#videos-container").width();
      if (getFocusedStatus()) {
        //主持人状态
        width = width - 16;
      } else {
        //平铺状态
        width = width / 2 - 14;
      }


      var config = {
          uid:uid,//当前用户的uid
          realName:getTrueNameByUserid(uid),//trtc.js生成的video的id          
          videoid:selfRtc.localStream_.getId(),//trtc.js生成的video的id          
          width:width,//宽度
          muteAudio:null,//静音方法
          unmuteAudio:null,//关闭静音方法
          muteVideo:
              function(){
                  selfRtc.localStream_.muteVideo()
              },//静音视频方法
          unmuteVideo:
              function(){
                  selfRtc.localStream_.unmuteVideo()
              },//关闭静音视频方法
          gdVideo:
              function(){
                  selfRtc.unpublish()
              },//挂断方法
          gd:true,//是否有挂断标签
      };
      

      CreatePlayerElement(config);
      this.localStream_.play(uid);
      // 发布本地流
      await this.client_.publish(this.localStream_);
      //Toast.info('发布本地流成功！');
      this.isPublished_ = true;
      SetOnLiveUser(uid);


      setTimeout(function(){
          //添加单击和双击事件
          var videoElement = document.getElementById("video_" + selfRtc.localStream_.getId())
          if(videoElement){
              //单击
              videoElement.onclick = function () {
                  SetFocus(selfRtc.localStream_.getUserId());
              };

              //双击全屏
              videoElement.ondblclick = function () {
                  launchFullscreen(videoElement);
              };
            }
      },3000)
      

    } catch (error) {
      console.error('failed to publish local stream ' + error);
      //Toast.error('发布本地流失败！');
      this.isPublished_ = false;
      var ele = $("#videos-container div[data-uid='" + this.localStream_.getUserId() + "']");
      if (ele.length > 0) {
          ele.remove();
      }
    }
  }



}


//根据用户id获取用户的真实姓名
function getTrueNameByUserid(uid) {
  var ele = $(".member-ul li[data-uid='" + uid + "']");
  if (ele.length !== 0) {
    return ele.attr("data-title");
  }
  return "";
}



