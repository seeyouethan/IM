using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using log4net;
using Microsoft.AspNet.SignalR.Hubs;
using Edu.Tools;
using Edu.Models.Models;
using Edu.Service.Service;
using Edu.Models.Models.Msg;
using Edu.Service.Service.Message;
using Edu.SignalRServer.Models;
using Edu.SignalRServer.Service;
using Newtonsoft.Json;
using Edu.Models.JobAssignment;
using Edu.SignalRServer.Model;
using Edu.Entity;

namespace Edu.SignalRServer.Hub
{

    /*
        redis中存放的主要有三个表
            IMUserOnLineApp 是移动端在线用户表
            IMUserOnLine 是OAOKCS端在线用户表
            IMUserOnLine_OA 是OA端在线用户表
        这三个的在线状态是互通的
    */


    [HubName("chatHub")]
    [EnableCors("*", "*", "*")]
    public class ChatHub : Microsoft.AspNet.SignalR.Hub
    {
        private DataService _dataService=new DataService();

        public override Task OnConnected()
        {
            return Clients.Caller.receiveMessage("Connect Success");
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            //stopCalled=true时，客户端关闭连接
            //stopCalled=false时，出现链接超时

            UserOffline(Context.ConnectionId);
            var msg=new DisconnetMsg();            
            if (!stopCalled)
            {
                msg.type = 0;
                msg.msg = "出现链接超时断开";
                return Clients.Caller.disconnectMessage(msg);
            }
            else
            {
                msg.type = 1;
                msg.msg = "客户端关闭连接(可能是自己主动断开链接，也可能是该账号被其他设备端登录)";
                return Clients.Caller.disconnectMessage(msg);
            }
        }

        public override Task OnReconnected()
        {
            return Clients.Caller.receiveMessage("User Reconnect:"+Context.ConnectionId);
        }

        /// <summary>
        /// 移动端离线的时候，调用该方法
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="deviceid"></param>
        public void AppOffLine(string uid,string deviceid)
        {
            var onlineapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp",uid);
            if (onlineapp != null)
            {
                if (onlineapp.deviceid == deviceid)
                {
                    LogoutLog(onlineapp);
                    RedisHelper.Hash_Remove("IMUserOnLineApp", uid);
                }
            }
            //查询其他终端是否还有在线的用户，如果有，就不通知下线了
            var online_oaokcs = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
            var online_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
            if (online_oaokcs==null && online_oa == null)
            {
                //通知下线
                Clients.All.bsOffLine(uid);
            }
        }

        private void UserOnline(string uid, bool isApp = false, string deviceid="", string deviceToken = "",string devicetype="oaokcs")
        {
            try
            {
                //如果是移动端，并且devicetype是web，则默认为android设备
                if (isApp && devicetype == "web")
                {
                    devicetype = "android";
                }

                //如果有在线的用户，则先把它踢掉,（更新缓存数据）移动端只允许一个用户在线
                if (isApp)
                {
                    //移动端登陆
                    var onlineapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", uid);
                    if (onlineapp != null && deviceid != onlineapp.deviceid)
                    {
                        //先离线，再上线
                        LogoutLog(onlineapp);
                        RedisHelper.Hash_Remove("IMUserOnLineApp", uid);
                        //调用移动端的方法，让移动端离线
                        //Clients.Client(onlineapp.ConnectionId).disConnect();
                    }
                }
                else
                {
                    //web端登陆
                    //分别判断是OA端还是OAOKCS端
                    if (devicetype == "oaokcs")
                    {
                        var online = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
                        if (online != null)
                        {
                            //先离线，再上线
                            LogoutLog(online);
                            RedisHelper.Hash_Remove("IMUserOnLine", uid);
                        }
                    }
                    else if (devicetype == "oa")
                    {
                        var online = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
                        if (online != null)
                        {
                            //先离线，再上线
                            LogoutLog(online);
                            RedisHelper.Hash_Remove("IMUserOnLine_OA", uid);
                        }
                    }                    
                }

                //然后再上线
                UserOnLine model = new UserOnLine
                {
                    ConnectionId = Context.ConnectionId,
                    uid = uid,
                    CreateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                    deviceid = deviceid,
                    deviceToken = deviceToken,
                    devicetype = devicetype,
                };
                if (isApp)
                {
                    RedisHelper.Hash_Set<UserOnLine>("IMUserOnLineApp", uid, model);
                }else
                {
                    if (devicetype == "oaokcs")
                    {
                        RedisHelper.Hash_Set<UserOnLine>("IMUserOnLine", uid, model);
                    }
                    else if (devicetype == "oa")
                    {
                        RedisHelper.Hash_Set<UserOnLine>("IMUserOnLine_OA", uid, model);
                    }
                }
                Clients.All.bsOnLine(uid, Context.ConnectionId); //通知所有人我上线了
                LoginLog(model);
                //如果是ios设备，那么会更新一下deviceToken和deviceid
                if (isApp && !string.IsNullOrEmpty(deviceToken) && !string.IsNullOrEmpty(deviceid) && devicetype=="ios")
                {
                    var b = _dataService.UpdateUserDevice(uid, deviceid, deviceToken, devicetype);
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("UserOnline Error",ex);
            }
        }

        private void UserOffline(string connid)
        {
            //通知离线功能，只给web端推送，不给移动端推送，因为移动端没有上线下线的指示
            var uid = "";
            var online = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine");
            if (online != null&& online.Any())
            {
                var self = online.Where(p => p.ConnectionId == connid).ToList();
                if (self.Any())
                {
                    foreach (var item in self)
                    {
                        uid = item.uid;
                        LogoutLog(item);
                        RedisHelper.Hash_Remove("IMUserOnLine", item.uid);
                    }
                }
            }

            var online_oa = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine_OA");
            if (online_oa != null && online_oa.Any())
            {
                var self = online_oa.Where(p => p.ConnectionId == connid).ToList();
                if (self.Any())
                {
                    foreach (var item in self)
                    {
                        uid = item.uid;
                        LogoutLog(item);
                        RedisHelper.Hash_Remove("IMUserOnLine_OA", item.uid);
                    }
                }
            }



            var onlineapp = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLineApp");
            if (onlineapp != null && onlineapp.Any())
            {
                var self = onlineapp.Where(p => p.ConnectionId == connid).ToList();
                if (self.Any())
                {
                    foreach (var item in self)
                    {
                        uid = item.uid;
                        LogoutLog(item);
                        RedisHelper.Hash_Remove("IMUserOnLineApp", item.uid);
                    }
                }
            }


            //从redis中移除对应的消息后，再根据uid判断该用户在其他终端是否有登录，如果没有，再发送离线通知
            //查询其他终端是否还有在线的用户，如果有，就不通知下线了
            var online_oaokcs = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
            var online_oa_new = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
            var online_app = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", uid);
            if (online_oaokcs == null && online_oa_new == null && online_app == null)
            {
                //通知其他用户，我已经下线
                Clients.All.bsOffLine(uid);
            }
        }

        /// <summary>
        /// mqtt服务作为一个客户端登录的时候，去调用
        /// 转发服务作为一个客户端登录的时候，去调用
        /// </summary>
        /// <param name="uid">用户的uid</param>
        /// <param name="isApp">是否是移动端登录</param>
        public void Connect(string uid, bool isApp) 
        {
            UserOnline(uid, isApp);
        }

        public void ConnectWithTerminal(string uid,string terminal)
        {
            UserOnline(uid, false, "", "", terminal);
        }

        /// <summary>
        /// 登录连线，在缓存中记录对应的用户uid（移动端采用的最新的版本）
        /// </summary>
        /// <param name="uid">用户的uid</param>
        /// <param name="isApp">是否是移动端登录</param>
        /// <param name="deviceid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="devicetype"></param>
        public void ConnectNew(string uid, bool isApp, string deviceid,string deviceToken,string devicetype)
        {
            UserOnline(uid, isApp, deviceid, deviceToken,devicetype);
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="id"></param>
        public void DeleteMessage(string groupid,int id) { 
            var result= _dataService.DeleteMessage(id);

            if (result == 1)
            {
                var touids = _dataService.GetWorkGroupMembers(groupid);
                foreach (var touid in touids)
                {
                    Task.Run(() =>
                    {
                        var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
                        if (touser != null)
                        {
                            Clients.Client(touser.ConnectionId).deleteMessageInvoke(groupid,id);
                        }
                    });
                }
            }
            Clients.Caller.DeleteMessageResult(id, result);
        }

        /// <summary>
        /// 删除主题
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="id"></param>
        public void DeleteSubject(string groupid,int id)
        {
            var result = _dataService.DeleteSubject(id);
            if (result == 1)
            {
                var touids = _dataService.GetWorkGroupMembers(groupid);
                if (result == 1)
                {
                    foreach (var touid in touids)
                    {
                        Task.Run(() =>
                        {
                            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
                            if (touser != null)
                            {
                                Clients.Client(touser.ConnectionId).deleteSubjectInvoke(groupid,id);
                            }
                        });
                    }
                }
            }

            Clients.Caller.DeleteSubjectResult(id, result);
        }


        public void AddSubject(string json)
        {
            var subject = JsonConvert.DeserializeObject<GroupSubject>(json);

            var touids = _dataService.GetWorkGroupMembers(subject.groupid);
            touids.Remove(subject.creator);
            foreach (var touid in touids)
            {
                Task.Run(() =>
                {
                    var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
                    if (touser != null)
                    {
                        Clients.Client(touser.ConnectionId).addSubjectInvoke(json);
                    }
                });
            }
        }


        /// <summary>
        /// web端发送聊天消息，调用这个方法
        /// </summary>
        /// <param name="json">消息接口的Json字符串</param>
        /// <param name="terminal">终端类型，目前包括oaokcs和oa</param>
        public void SendMessageFromWeb(string json,string terminal)
        {
            try
            {
                var msg = new Msg();//收到的消息
                var msgfeedback = new MsgFeedback();//回执消息
                var jsonFeedback = string.Empty;
                var sendUserConnectionId = Context.ConnectionId;
                msg = JsonConvert.DeserializeObject<Msg>(json);
                //消息合法性验证
                if (string.IsNullOrEmpty(msg.fromuid) || string.IsNullOrEmpty(msg.touid) || string.IsNullOrEmpty(msg.fromrealname) || string.IsNullOrEmpty(msg.torealname) || (msg.isgroup != 0 && msg.isgroup != 1))
                {
                    msg.id0 = "0";
                    //相关字段为空
                    msgfeedback.Success = false;
                    //msgfeedback.Content = json;
                    msgfeedback.Content = JsonConvert.SerializeObject(msg);
                    msgfeedback.Error = "相关字段为空";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                    return;
                }
                //当前发送人的connid和缓存中的connid不同的时候，视为该用户在其他设备登录，
                //最新的connid以缓存中记录为主,如果不同，则发送失败
                var fromuser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", msg.fromuid);
                var fromuser_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", msg.fromuid);
                //这里不做对【是否是移动端发送来的消息】进行判断，因为这个方法的web端调用的,所以注释掉以下这一行
                //var fromuser_app = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", "mqttserver");

                var connid = "";
                var connid_oa = "";
                var connid_app = "";
                if (fromuser != null)
                {
                    connid = fromuser.ConnectionId;
                }
                if (fromuser_oa != null)
                {
                    connid_oa = fromuser_oa.ConnectionId;
                }
                
                if (connid != Context.ConnectionId && connid_oa != Context.ConnectionId)
                {
                    msg.id0 = "0";
                    msgfeedback.Success = false;
                    //msgfeedback.Content = json;
                    msgfeedback.Content = JsonConvert.SerializeObject(msg);
                    msgfeedback.Error = "您已经在其他客户端登录，发送消息失败";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                    return;
                }
                var selfuserapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", msg.fromuid);
                //添加到数据库
                var msgid = _dataService.InsertMsg(msg);
                var groupid = "";
                if (msg.isgroup == 1) groupid = msg.touid;
                if (msgid == 0)
                {
                    msg.id0 = "0";
                    //消息添加到数据库失败，发送消息回执
                    msgfeedback.Success = false;
                    //msgfeedback.Content = json;
                    msgfeedback.Content = JsonConvert.SerializeObject(msg);
                    msgfeedback.Error = "数据存储失败（msgid=0）";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                    return;
                }
                msg.id0 = msgid.ToString();
                var jsonMsg = JsonConvert.SerializeObject(msg);

                //1.发送消息给回执
                Task.Run(() =>
                {
                    //成功的消息回执
                    msgfeedback.Success = true;
                    msgfeedback.Content = jsonMsg;
                    msgfeedback.Error = "";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                });
                //2.异步开始发送消息
                if (msg.isgroup == 1)
                {
                    //发送群组消息
                    var touids = new List<string>();
                    //根据群组ID去获取其对应的群组成员
                    if (groupid.Length == 36)
                    {
                        //工作群组的ID长度的36位的，自建群的群组ID是从1开始自增的int，所以长度不可能达到36位
                        touids = _dataService.GetWorkGroupMembers(groupid);
                    }
                    else
                    {
                        touids = _dataService.GetSelfGroupMembers(groupid);
                    }
                    if (touids.Any() && touids.Contains(msg.fromuid))
                        //如果是群组消息，不再给自己推送
                        touids.Remove(msg.fromuid);
                    SendGroupMessageCommon(touids, msg);
                }
                else
                {
                    SendPersonalMessageCommon(msg);
                }
            }
            catch (Exception ex)
            {

                var msg = JsonConvert.DeserializeObject<Msg>(json);
                msg.id0 = "0";
                var msgfeedback = new MsgFeedback();
                msgfeedback.Success = false;
                //msgfeedback.Content = json;
                msgfeedback.Content = JsonConvert.SerializeObject(msg);
                msgfeedback.Error = ex.ToString();
                var jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                Clients.Client(Context.ConnectionId).msgFeedback(jsonFeedback);
                LoggerHelper.Error(ex.ToString());
                LoggerHelper.Error("Json:" + json);
                LoggerHelper.Error("Terminal:" + terminal);
            }
        }


        /// <summary>
        /// 移动端发送聊天消息，调用这个方法
        /// </summary>
        /// <param name="json">消息接口的Json字符串</param>
        public void SendMessageA(string json)
        {
            try
            {
                var msg = new Msg();//收到的消息
                var msgfeedback = new MsgFeedback();//回执消息
                var jsonFeedback = string.Empty;
                var sendUserConnectionId = Context.ConnectionId;
                msg = JsonConvert.DeserializeObject<Msg>(json);
                //消息合法性验证
                if (string.IsNullOrEmpty(msg.fromuid) || string.IsNullOrEmpty(msg.touid) || string.IsNullOrEmpty(msg.fromrealname) || string.IsNullOrEmpty(msg.torealname) || (msg.isgroup != 0 && msg.isgroup != 1))
                {
                    //验证失败相关字段为空
                    msgfeedback.Success = false;
                    msgfeedback.Content = json;
                    msgfeedback.Error = "相关字段为空";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                    return;
                }
                //当前发送人的connid和缓存中的connid不同的时候，视为该用户在其他设备登录，
                //最新的connid以缓存中记录为主,如果不同，则发送失败

                //这里不做对【是否是web端发送来的消息】进行判断，因为这个方法的移动端调用的
                var fromuser_app = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", "mqttserver");
                
                var connid_app = "";
                
                if (fromuser_app != null)
                {
                    connid_app = fromuser_app.ConnectionId;
                }
                if (connid_app != Context.ConnectionId)
                {
                    //mqtt掉线，无法从移动端发送消息给web端
                    msgfeedback.Success = false;
                    msgfeedback.Content = json;
                    msgfeedback.Error = "您已经在其他客户端登录，发送消息失败";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                    return;
                }
                //添加到数据库
                var msgid = _dataService.InsertMsg(msg);
                var groupid = "";

                if (msgid == -1) return;
                if (msg.isgroup == 1) groupid = msg.touid;
                if (msgid == 0)
                {
                    //失败的消息回执
                    msgfeedback.Success = false;
                    msgfeedback.Content = json;
                    msgfeedback.Error = "数据存储失败（msgid=0）";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                    return;
                }
                msg.id0 = msgid.ToString();
                var jsonMsg = JsonConvert.SerializeObject(msg);

                //1.发送消息给回执
                Task.Run(() =>
                {
                    //成功的消息回执
                    msgfeedback.Success = true;
                    msgfeedback.Content = jsonMsg;
                    msgfeedback.Error = "";
                    jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                    Clients.Client(sendUserConnectionId).msgFeedback(jsonFeedback);
                });
                //2.异步开始发送消息

                if (msg.isgroup == 1)
                {
                    //发送群组消息
                    var touids = new List<string>();
                    //根据群组ID去获取其对应的群组成员
                    if (groupid.Length == 36)
                    {
                        //工作群组的ID长度的36位的，自建群的群组ID是从1开始自增的int，所以长度不可能达到36位
                        touids = _dataService.GetWorkGroupMembers(groupid);
                    }
                    else
                    {
                        touids = _dataService.GetSelfGroupMembers(groupid);
                    }
                    if (touids.Any() && touids.Contains(msg.fromuid))
                        //如果是群组消息，不再给自己推送
                        touids.Remove(msg.fromuid);


                    SendGroupMessageCommon(touids, msg);
                }
                else
                {
                    SendPersonalMessageCommon(msg);
                }                
            }
            catch (Exception ex)
            {
                var msgfeedback = new MsgFeedback();
                msgfeedback.Success = false;
                msgfeedback.Content = json;
                msgfeedback.Error = ex.ToString();
                var jsonFeedback = JsonConvert.SerializeObject(msgfeedback);
                Clients.Client(Context.ConnectionId).msgFeedback(jsonFeedback);
                LoggerHelper.Error("SendMessageA Error",ex);
            }
        }

        /// <summary>
        /// 发送单人消息
        /// </summary>
        /// <param name="msg"></param>
        private void SendPersonalMessageCommon(Msg msg)
        {
            //推送消息
            Task.Run(() =>
            {
                SendMessageToOther(msg.fromuid, msg.touid, msg);
            });
            //各终端同步自己的消息（如果在线）
            SendMessageToSelf(msg); 
        }

        /// <summary>
        /// 发送群组消息
        /// </summary>
        /// <param name="touids"></param>
        /// <param name="msg"></param>
        private void SendGroupMessageCommon(List<string> touids,Msg msg)
        {
            foreach (var touid in touids)
            {
                Task.Run(() =>
                {
                    SendMessageToOther(msg.fromuid, touid, msg);
                });
            }
            //各终端同步自己的消息（如果在线）
            SendMessageToSelf(msg);
        }

        /// <summary>
        /// 整合的方法，发送消息，内部调用  给别人发消息
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="msg"></param>
        private void SendMessageToOther(string fromuid,string touid, Msg msg)
        {
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            var touser_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", touid);
            var touserapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);

            //var selfuser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", fromuid);
            //var selfuser_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", fromuid);
            //var selfuserapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", fromuid);
            //移动消息的发送者/接收者
            var mqttserver = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", "mqttserver");
            //消息字符串
            var json = JsonConvert.SerializeObject(msg);

            if (touser != null)
            {
                Clients.Client(touser.ConnectionId).receiveMessageA(json);
            }           
            if (touser_oa != null)
            {
                Clients.Client(touser_oa.ConnectionId).receiveMessageA(json);
            }
            if (touserapp != null)
            {
                //给移动端发送的消息，都存到缓存中，移动端在进入聊天界面会清空一次，退出聊天界面也会清空一次
                MessageToRedis(touid, msg);
                Clients.Client(mqttserver.ConnectionId).MQTTReceive(touid,json);
            }
            else
            {
                //接收人移动端不在线，推送离线消息
                OffLinePush(touid, msg);
            }
            //接收人web端和移动端都不在线，写到缓存中
            if (touser == null && touser_oa == null && touserapp == null)
            {
                MessageToRedis(touid, msg);
            }            
        }

        /// <summary>
        /// 多个终端互相同步自己推送的消息   （app->web 同步已经更新  web-app还没更新  去掉注释即可）
        /// </summary>
        /// <param name="msg"></param>
        private async Task SendMessageToSelf(Msg msg)
        {
            var selfuser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", msg.fromuid);
            var selfuser_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", msg.fromuid);
            var selfuserapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", msg.fromuid);
            var mqttserver = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", "mqttserver");
            var json = JsonConvert.SerializeObject(msg);

            //消息来源终端
            var terminal = msg.terminal;
            if (string.IsNullOrEmpty(terminal))
            {
                //消息来自于移动端
                if (selfuser != null)
                {
                    Clients.Client(selfuser.ConnectionId).receiveMessageA(json);
                }
                if (selfuser_oa != null)
                {
                    Clients.Client(selfuser_oa.ConnectionId).receiveMessageA(json);
                }
            }
            else if (terminal == "oaokcs")
            {
                //消息来自于oaokcs端
                //if (selfuserapp != null)
                //{
                //    Clients.Client(mqttserver.ConnectionId).MQTTReceive(msg.fromuid,json);
                //}
                if (selfuser_oa != null)
                {
                    Clients.Client(selfuser_oa.ConnectionId).receiveMessageA(json);
                }
            }
            else if (terminal == "oa")
            {
                //消息来自于oa端
                //if (selfuserapp != null)
                //{
                //    Clients.Client(mqttserver.ConnectionId).MQTTReceive(msg.fromuid, json);
                //}
                if (selfuser != null)
                {
                    Clients.Client(selfuser.ConnectionId).receiveMessageA(json);
                }
            }
        }
        private void MessageToRedis(string touid,Msg msg)
        {
            if (msg.isgroup == 0)
            {
                //个人聊天消息
                MsgServices.ResetRedisKeyValue("IMMsg", msg.touid, msg);
            }
            else if(msg.isgroup == 1)
            {
                //群组聊天消息
                MsgServices.ResetRedisKeyValue<Msg>("IMGroupMsg", msg.touid + touid, msg);
                /*添加和该成员相关系的有未读消息的群,用来获取该用户是否有未读消息，页面上右上角的红点*/
                var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", touid) ?? new List<String>();
                if (!unreadgroupmsg.Contains(msg.touid))
                {
                    unreadgroupmsg.Add(msg.touid);
                    RedisHelper.Hash_Remove("IMUnreadGroupMsg", touid);
                    RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", touid, unreadgroupmsg);
                }
            }
        }

        /// <summary>
        /// 聊天消息，离线推送消息
        /// </summary>
        /// <param name="msg"></param>
        private async Task OffLinePush(string touid,Msg msg)
        {
            try
            {
                //查询是否对该用户进行离线推送
                var isPush = _dataService.GetNoticeSwitch(touid, 1);

                if (isPush)
                {
                    //var json = JsonConvert.SerializeObject(msg);
                    //手机不在进程中,查询数据库中是否有记录，如果有对应的devicetoken那么就推送消息
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;//这个字段好像没用了 2019年9月26日
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (!string.IsNullOrEmpty(devicetoken) && devicetype == "ios")
                    {
                        if (msg.msgtype != 6)
                        {                            
                            SmartPushService.PushMessageToSingle(touid,devicetoken,msgcount,msg);
                        }
                    }
                    
                    if (devicetype == "android" && !string.IsNullOrEmpty(devicetoken))
                    {
                        if (msg.msgtype != 6)
                        {
                            AndroidPushService.PushMessageToSingle(touid, devicetoken, msg);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"OffLinePush Exception!ex:  "+ ex);
            }
                
        }

        

        //发起视频聊天 
        public void VideoChat(string fromuid, string fromusername, string touid,string roomid)
        {
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            var toconnid = "";
            var fromconnid = "";
            if (touser != null)
            {
                toconnid = touser.ConnectionId;
                fromconnid = Context.ConnectionId;
                Clients.Client(toconnid).requestVideoChat(fromuid, fromusername, touid, fromconnid,roomid);
            }
        }
        //拒绝视频聊天
        public void RefuseVideoChat(string fromuid, string touid)
        {
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            if (touser != null)
            {
                var toconnid = touser.ConnectionId;
                Clients.Client(toconnid).videoChatRefused(fromuid);
            }
        }

        /// <summary>
        /// 发起者主动关闭视频窗口（场景：发起者发起视频聊天 接受者还没有点击任何按钮 发起者点击关闭视频聊天窗口）
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        public void CloseVideoChatByFrom(string fromuid, string touid)
        {
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            if (touser != null)
            {
                var toconnid = touser.ConnectionId;
                Clients.Client(toconnid).videoChatClosedByFrom(fromuid);
            }
        }

        /// <summary>
        /// 群组动态，主要是工作群组的 新建、加人、减人、解散操作等等，web端也需要加上对应的通知了
        /// </summary>
        /// <param name="uid">发送消息的人</param>
        /// <param name="touids">接受消息的人</param>
        /// <param name="msgcontent">消息内容(Json)</param>
        public void SendMsg(string uid, List<string> touids, string msgcontent)
        {
            /*
             * 针对Web页面：新增了如下逻辑：
             *  1.新建：通知所有人
                2.加人：通知所有人
                3.减人：通知所有人
                4.解散：通知所有人
                5.更新：通知所有人
             */

            var msgModel = JsonConvert.DeserializeObject<AppGroupDynamic>(msgcontent);
            
            if (!touids.Contains(msgModel.CreatorId))
            {
                touids.Add(msgModel.CreatorId);
            }

            foreach (var touid in touids)
            { 
                var touserOAOKCS=RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
                if (touserOAOKCS != null)
                {
                    //OAOKCS 端在线
                    Clients.Client(touserOAOKCS.ConnectionId).receiveMessageA(msgcontent);
                }
                var touserOA = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", touid);
                if (touserOA != null)
                {
                    //OA 端在线
                    Clients.Client(touserOA.ConnectionId).receiveMessageA(msgcontent);
                }
            }



            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {
                    //判断是否进行离线推送
                    var isPush = _dataService.GetNoticeSwitch(touid, 2);
                    if (isPush)
                    {
                        var msgcount = 0;
                        var devicetype = "";
                        var isonline = false;//这个字段好像没用了 2019年9月26日
                        var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                        if (devicetype == "android")
                        {
                            if (!string.IsNullOrEmpty(devicetoken))
                            {
                                //AndroidPushService.PushGroupDynamic(devicetoken, touid, msgcontent);
                            }

                        }
                        else if (devicetype == "ios")
                        {
                            //SmartPushService.PushGroupDynamic(touid, devicetoken, msgcount, msgcontent);
                        }
                    }                  
                }
            }
        }

        /// <summary>
        /// 群组公告（移动端使用)离线推送
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="touids"></param>
        /// <param name="msgcontent"></param>
        public void SendGroupNotice(string uid, List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {

                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushGroupNotice(devicetoken, touid, msgcontent);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            SmartPushService.PushGroupNotice(touid, devicetoken, msgcount,msgcontent);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 工作台公告（移动端使用),发送给所有在线用户
        /// </summary>
        /// <param name="msgcontent"></param>
        public void SendAppNotice(string msgcontent)
        {
            Clients.Others.receiveAppNotice(msgcontent);
            //暂时没有离线推送

        }

        /// <summary>
        /// 待办任务（移动端使用)  这些只是作为一个日志通知查看，并不会发送具体的内容
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="touid"></param>
        /// <param name="msgcontent"></param>
        public void SendTodoTask(string uid, string touid, string msgcontent)
        {
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
            if (touser != null)
            {
            }
            else
            {
                var msgcount = 0;
                var devicetype = "";
                var isonline = false;
                var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                if (devicetype == "android")
                {
                    if (!string.IsNullOrEmpty(devicetoken))
                    {
                        AndroidPushService.PushTodoTask(devicetoken, touid, msgcontent);
                    }

                }
                else if (devicetype == "ios")
                {
                    
                }
            }
        }

        /// <summary>
        /// 工作交办(待办任务、日程)推送
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="msgcontent"></param>
        public void SendSchedule( string touid, string msgcontent)
        {
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
            if (touser != null)
            {
            }
            else
            {
                var msgcount = 0;
                var devicetype = "";
                var isonline = false;//这个字段好像没用了 2019年9月26日
                var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                if (devicetype == "android")
                {                    
                    if (!string.IsNullOrEmpty(devicetoken))
                    {
                        var model= JsonConvert.DeserializeObject<ScheduleModel>(msgcontent);
                        AndroidPushService.PushToDoMessage(devicetoken,touid,model);
                    }

                }
                else if (devicetype == "ios")
                {
                    
                }



            }
            
            var touserweb=RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            if (touserweb != null)
            {
                var connid = touserweb.ConnectionId;
                Clients.Client(connid).receiveSchedule(msgcontent);
            }
        }

        public void SendDeleteCreation(string id, bool isDelete)
        {
            if (isDelete)
            {
                var b = _dataService.DeletePlanDiscuss(id);
            }
            else
            {
                var b = _dataService.RevertPlanDiscuss(id);
            }
        }

        public void SendAddDiscuss(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {

                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;//这个字段好像没用了 2019年9月26日
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken, 6, msgcontent,touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        
                    }
                }
            }
        }

        public void SendDeleteDiscuss(List<string> touids, string msgcontent, string discussid)
        {
            //在我的数据库中删除这个协同研讨，把和工作任务相关的这条协同研讨删除了
            _dataService.DeletePlanDiscuss(discussid);

            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                    var connid = touser.ConnectionId;
                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;//这个字段好像没用了 2019年9月26日
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken, 7, msgcontent,touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        
                    }
                }
            }
        }
        public void SendModifyDiscuss(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {

                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken, 8, msgcontent, touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        
                    }
                }
            }
        }

        public void SendFinishDiscuss(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {

                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken, 9, msgcontent, touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        
                    }
                }
            }
        }

        public void SendActivateDiscuss(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken, 10, msgcontent, touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                       
                    }
                }
            }
        }

        public void SendRemindDiscuss(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken, 11, msgcontent, touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        
                    }
                }
            }
        }

        public void SendNote(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {
                    var msgcount = 0;
                    var devicetype = "";
                    var isonline = false;
                    var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);
                    if (devicetype == "android")
                    {
                        if (!string.IsNullOrEmpty(devicetoken))
                        {
                            AndroidPushService.PushDiscussMessage(devicetoken,12,msgcontent, touid);
                        }

                    }
                    else if (devicetype == "ios")
                    {
                        
                    }
                }
            }
        }

        public void SendSelfGroupNotice(List<string> touids, string msgcontent)
        {
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {
                    //判断是否进行离线推送
                    var isPush = _dataService.GetNoticeSwitch(touid, 2);
                    if (isPush)
                    {
                        var msgcount = 0;
                        var devicetype = "";
                        var isonline = false;
                        var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                        if (devicetype == "android")
                        {
                            if (!string.IsNullOrEmpty(devicetoken))
                            {
                                AndroidPushService.PushSelfGroupNotice(devicetoken, touid, msgcontent);
                            }

                        }
                        else if (devicetype == "ios")
                        {
                            
                        }
                    }
                }
            }
        }

        public void SendSelfGroupNoticeMore(List<string> touids, string msgcontent)
        {

            var model = JsonConvert.DeserializeObject<SelfGroupNoticeMore>(msgcontent);
            foreach (var touid in touids)
            {
                var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                if (touser != null)
                {
                }
                else
                {
                    //判断是否进行离线推送
                    var isPush = _dataService.GetNoticeSwitch(touid, 2);
                    if (isPush)
                    {
                        var msgcount = 0;
                        var devicetype = "";
                        var isonline = false;
                        var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                        if (devicetype == "android")
                        {
                            if (!string.IsNullOrEmpty(devicetoken))
                            {
                                if (model.type == 3)
                                {
                                    //添加成员的操作，不进行个推离线推送(只有解散)
                                    //AndroidPushService.PushSelfGroupNoticeMore(devicetoken, touid, model);
                                }
                            }

                        }
                        else if (devicetype == "ios")
                        {
                            if (model.type == 3)
                            {
                                //添加成员的操作，不进行个推离线推送(只有解散)
                                //SmartPushService.PushSelfGroupNoticeMore(touid, devicetoken,msgcount, model);
                            }                            
                        }
                    }
                }
            }
        }

        public void SendLiveAPPInfoNotice(List<string> touids, string msgcontent, int isPublic)
        {
            if (isPublic == 1)
            {
                Clients.All.receiveLiveAppInfoNotice(msgcontent);
            }
            else
            {
                foreach (var touid in touids)
                {
                    var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                    if (touser != null)
                    {
                    }
                    else
                    {
                        var msgcount = 0;
                        var devicetype = "";
                        var isonline = false;
                        var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                        if (devicetype == "android")
                        {
                            if (!string.IsNullOrEmpty(devicetoken))
                            {
                                AndroidPushService.PushLiveAPPInfoNotice(devicetoken, touid, msgcontent);
                            }

                        }
                        else if (devicetype == "ios")
                        {
                            //OffLinePushOA(touid, msgcontent);
                        }
                    }
                }
            }


        }

        public void SendOANotice(List<string> touids, string msgcontent)
        {
            //如果这里收不到，可能是转发服务连接的signalr客户端那边掉线了
            try {                
                var guid = Guid.NewGuid().ToString();
                OALog(msgcontent);
                var model = JsonConvert.DeserializeObject<MessageModel>(msgcontent);
                //statuscode为0的时候，不需要给离线设备推送
                if (model.StatusCode == 0)
                {

                }
                else
                {
                    foreach (var touid in touids)
                    {
                        var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", touid);
                        if (touser != null)
                        {
                        }
                        else
                        {
                            //离线设备推送
                            //判断是否进行离线推送
                            var isPush = _dataService.GetNoticeSwitch(touid, 3);
                            
                            if (isPush)
                            {
                                var msgcount = 0;
                                var devicetype = "";
                                var isonline = false;
                                var devicetoken = _dataService.GetUserDeviceToken(touid, out msgcount, out devicetype, out isonline);

                                if (devicetype == "android")
                                {
                                    if (!string.IsNullOrEmpty(devicetoken))
                                    {
                                        AndroidPushService.PushOANotice(devicetoken, touid, model);
                                    }

                                }
                                else if (devicetype == "ios")
                                {
                                    SmartPushService.PushOANotice(touid, devicetoken, msgcount, model);
                                }
                            }
                        }
                    }
                }
            } catch (Exception ex)
            {
                LoggerHelper.Error(ex.ToString());
                LoggerHelper.Error(msgcontent);
            }
            

            
        }

        /// <summary>
        /// 发送成功的消息日志
        /// </summary>
        /// <param name="msg"></param>
        private void MsgLog(string device0 ,string device1, Msg msg,string touid)
        {
            //发送消息的日志
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", "01logger");
            if (touser != null)
            {
                var connid = touser.ConnectionId;
                if (msg.isgroup==1)
                {
                    //群组
                    Clients.Client(connid).weblog(msg.msgtime, "("+device0 + ")" + msg.fromrealname, "(" + device1 + ")" + touid, msg);
                }
                else
                {
                    //个人
                    Clients.Client(connid).weblog(msg.msgtime, "(" + device0 + ")" + msg.fromrealname, "(" + device1 + ")" + msg.torealname, msg);
                }
            }
        }

        /// <summary>
        /// 记录消息到oa日志显示界面
        /// </summary>
        /// <param name="content"></param>
        private void OALog(string content)
        {
            var dt = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", "oalogger");
            if (touser != null)
            {
                var connid = touser.ConnectionId;
                Clients.Client(connid).oalog(dt,content);
            }
        }

        private void SelfMsgLog(string device0, string device1, Msg msg)
        {
            //发送消息的日志
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", "01logger");
            if (touser != null)
            {
                var connid = touser.ConnectionId;

                Clients.Client(connid).selfweblog(msg.msgtime, "(" + device0 + ")" + msg.fromrealname, "(" + device1 + ")" + msg.fromrealname, msg);
            }
        }
        /// <summary>
        /// 写入到缓存中的消息日志
        /// </summary>
        /// <param name="msg"></param>
        private void RedisLog(string device0,Msg msg, string touid)
        {
            //记录缓存的日志
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", "01logger");
            if (touser != null)
            {
                var connid = touser.ConnectionId;
                if (msg.isgroup == 1)
                {
                    //群组
                    Clients.Client(connid).redislog(msg.msgtime, "(" + device0 + ")" + msg.fromrealname, "(缓存)" + touid, msg);
                }
                else
                {
                    //个人
                    Clients.Client(connid).redislog(msg.msgtime, "(" + device0 + ")" + msg.fromrealname, "(缓存)" + msg.torealname, msg);
                }
            }
        }

        /// <summary>
        /// 登陆记录
        /// </summary>
        /// <param name="user"></param>
        private void LoginLog(UserOnLine user)
        {
            //当前在线用户日志
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", "03logger");
            if (touser != null) { Clients.Client(touser.ConnectionId).loginlog(user); }
            

        }

        /// <summary>
        /// 离线记录
        /// </summary>
        /// <param name="user"></param>
        private void LogoutLog(UserOnLine user)
        {
            //当前在线用户日志
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", "03logger");
            if (touser != null)
            {
                Clients.Client(touser.ConnectionId).logoutlog(user);
            }

        }    
    }
}