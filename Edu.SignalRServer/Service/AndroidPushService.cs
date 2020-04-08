using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.gexin.rp.sdk.dto;
using com.igetui.api.openservice;
using com.igetui.api.openservice.igetui;
using com.igetui.api.openservice.igetui.template;
using com.igetui.api.openservice.igetui.template.notify;
using Edu.Models.Models.Msg;
using Edu.SignalRServer.Models;
using Edu.Tools;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using log4net;
using Newtonsoft.Json;
using Edu.Models.JobAssignment;
using Edu.SignalRServer.Model;

namespace Edu.SignalRServer.Service
{
    public static class AndroidPushService
    {

        private static DataService _dataService = new DataService();
        //参数设置 <-----参数需要重新设置----->
        //http的域名
        private static String HOST = "http://sdk.open.api.igexin.com/apiex.htm";

        //https的域名
        //private static String HOST = "https://api.getui.com/apiex.htm";
        
        private static String APPID = ConfigHelper.GetConfigString("AndroidPushAppid");
        private static String APPKEY = ConfigHelper.GetConfigString("AndroidPushAppKey");
        private static String MASTERSECRET = ConfigHelper.GetConfigString("AndroidPushMasterSecret");
        private static String Intent = ConfigHelper.GetConfigString("AndroidPushIntent");
        //移动端的clientid
        //private static String CLIENTID = "";

        public static TransmissionTemplate TransmissionTemplate(string title,string content,string intent,string msgJson)
        {
            TransmissionTemplate template = new TransmissionTemplate();
            template.AppId = APPID;
            template.AppKey = APPKEY;
            //应用启动类型，1：强制应用启动 2：等待应用启动
            template.TransmissionType = 2;
            //透传内容  
            template.TransmissionContent = msgJson;
            Notify noti = new Notify();
            noti.Title = title;
            noti.Content = content;
            noti.Type = NotifyInfo.Types.Type._intent;
            //移动端跳转的页面
            noti.Intent = intent;
            template.set3rdNotifyInfo(noti);
            return template;
        }



        /// <summary>
        /// 1.个推-聊天消息
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushMessageToSingle(string touid,string deviceid,Msg msg)
        {
            var currentMethod = "PushMessageToSingle";
            //LoggerHelper.AndroidPush($"0.开始给[{touid}]推送Android消息{Environment.NewLine},cliendid={deviceid}");
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);

            //消息模版：TransmissionTemplate:透传模板
            //var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.IndexActivity;S.friendId={msg.fromuid};S.friendName={msg.fromrealname};S.isGroup=0;S.levelNoticeTpye=1;end";
            var title = msg.fromrealname;
            var content = "";
            var fromPersonName = "";
            if(msg.isgroup == 1)
            {
                title = msg.torealname;
                fromPersonName = msg.fromrealname+":";
            }
            content = fromPersonName + msg.msg;
            if (msg.msgtype == 1)
            {
                content = fromPersonName+"[图片]";
            }
            else if (msg.msgtype == 2)
            {
                content = fromPersonName + "[文件]";
            }
            else if (msg.msgtype == 4)
            {
                content = fromPersonName + "[语音]";
            }
            else if (msg.msgtype == 5)
            {
                content = fromPersonName + "[地图签到]";
            }
            else if (msg.msgtype == 3)
            {
                content = fromPersonName + "[图片]";
            }

            var getui = new GeTuiMessage
            {
                mtype = 1,
                mcontent = msg,
            };
            if (msg.isgroup == 1)
            {
                intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.IndexActivity;S.friendId={msg.touid};S.friendName={msg.torealname};S.isGroup=1;S.levelNoticeTpye=1;end";
                //template = TransmissionTemplate(msg.torealname, msg.msg, intent, JsonConvert.SerializeObject(msg));
            }
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));



            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;
            //判断是否客户端是否wifi环境下推送，2为4G/3G/2G，1为在WIFI环境下，0为不限制环境
            //message.PushNetWorkType = 1;  

            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            //target.alias = ALIAS;

            var messageJson = JsonConvert.SerializeObject(message);
            //LoggerHelper.AndroidPush($"1.给[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");

            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                //LoggerHelper.AndroidPush($"2.给[{touid}]推送的消息返回结果为{Environment.NewLine}{pushResult}");

                _dataService.InsertGeguiLog(msg.fromuid, touid, msg.msgtime, msg.id0, "android", deviceid, messageJson);


            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.给[{touid}]推送发生了异常，重发后，服务端返回结果{Environment.NewLine}{pushResult}");
                
            }
        }

        /// <summary>
        /// 18.个推-工作交办 todolist（群组日程、待办）消息通知（刘静杰）
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="touid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushToDoMessage(string deviceid,string touid, ScheduleModel msg)
        {
            var currentMethod = "PushToDoMessage";
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var title = "待办任务";
            var content = msg.title;

            var getui = new GeTuiMessage
            {
                mtype = 18,
                mcontent = msg,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));   

            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;

            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;

            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.(个推-待办任务)给[{touid}]推送的内容：" + messageJson);

            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.(个推-待办任务)给[{touid}]返回结果：" + pushResult);

            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.(个推-待办任务)给[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }


        /// <summary>
        /// 6 - 12 研讨类消息
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msgtype"></param>
        /// <param name="msg"></param>
        /// <param name="touid"></param>
        /// <returns></returns>
        public static async Task PushDiscussMessage(string deviceid, int msgtype,string msg,string touid)
        {
            var currentMethod = "PushDiscussMessage";
            object model = null;
            var content = "新的研讨消息";

            if (msgtype == 6)
            {
                var m = JsonConvert.DeserializeObject<AddDiscuss>(msg);
                content = m.Name + "有新的研讨消息";
                model = m;
            }
            else if (msgtype == 7)
            {
                var m = JsonConvert.DeserializeObject<DeleteDiscuss>(msg);
                content = m.Name + "有新的研讨消息";
                model = m;
            }
            else if (msgtype == 8)
            {
                var m = JsonConvert.DeserializeObject<ModifyDiscuss>(msg);
                content = m.Name + "有新的研讨消息";
                model = m;
            }
            else if (msgtype == 9)
            {
                var m = JsonConvert.DeserializeObject<FinishDiscuss>(msg);
                content = m.Name + "有新的研讨消息";
                model = m;
            }
            else if (msgtype == 10)
            {
                var m = JsonConvert.DeserializeObject<ActivateDiscuss>(msg);
                content = m.Name + "有新的研讨消息";
                model = m;
            }
            else if (msgtype == 11)
            {
                var m = JsonConvert.DeserializeObject<RemindDiscuss>(msg);
                content = m.Name + "有新的研讨消息";
                model = m;
            }
            else if (msgtype == 12)
            {
                var m = JsonConvert.DeserializeObject<Note>(msg);
                content = m.Title + "有新的研讨消息";
                model = m;
            }



            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);

            //消息模版：TransmissionTemplate:透传模板
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var title = "协同研讨";

            var getui = new GeTuiMessage
            {
                mtype = msgtype,
                mcontent = model,
            };

            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            


            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;
            //判断是否客户端是否wifi环境下推送，2为4G/3G/2G，1为在WIFI环境下，0为不限制环境
            //message.PushNetWorkType = 1;  

            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            //target.alias = ALIAS;

            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-研讨类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");

            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.(个推-研讨类消息)给[{touid}]返回结果：" + pushResult);

            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.(个推-研讨类消息)给[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);

            }
        }



        /// <summary>
        /// 2 群组动态
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushGroupDynamic(string deviceid, string touid,string msg)
        {
            var currentMethod = "PushGroupDynamic";


            var title = "群组动态通知";
            var content = "新的群组动态";

            var model = JsonConvert.DeserializeObject<AppGroupDynamic>(msg);
            title = model.GroupName;
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);

            //消息模版：TransmissionTemplate:透传模板
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            

            var getui = new GeTuiMessage
            {
                mtype = 2,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;                         // 用户当前不在线时，是否离线存储,可选
            message.OfflineExpireTime = 1000 * 3600 * 12;            // 离线有效时间，单位为毫秒，可选
            message.Data = template;

            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;

            var messageJson = JsonConvert.SerializeObject(message);
            
            LoggerHelper.AndroidPush($"1.个推-群组动态类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");

            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-群组动态类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");

            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-群组动态类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);

            }
        }


        /// <summary>
        /// 3 群组公告
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushGroupNotice(string deviceid, string touid, string msg)
        {
            var currentMethod = "PushGroupNotice";
            var title = "群组公告";
            var content = "群组公告";
            var model = JsonConvert.DeserializeObject<AppGroupNotice>(msg);
            content = model.Title;
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var getui = new GeTuiMessage
            {
                mtype = 3,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;
            message.OfflineExpireTime = 1000 * 3600 * 12;
            message.Data = template;
            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-群组公告类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");
            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-群组公告类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");
            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-群组公告类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }


        /// <summary>
        /// 5 待办任务（吴雪飞负责发送）
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushTodoTask(string deviceid, string touid, string msg)
        {
            var currentMethod = "PushTodoTask";
            var title = "待办任务";
            var content = "";
            var model = JsonConvert.DeserializeObject<TodoTask>(msg);
            content = model.Name;
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var getui = new GeTuiMessage
            {
                mtype = 5,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;
            message.OfflineExpireTime = 1000 * 3600 * 12;
            message.Data = template;
            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-待办任务2类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");
            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-待办任务2类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");
            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-待办任务2类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }


        /// <summary>
        /// 13 自建群创建通知（刘静杰）
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushSelfGroupNotice(string deviceid, string touid, string msg)
        {
            var currentMethod = "PushSelfGroupNotice";
            var title = "新建群组通知";
            var content = "新建群组";
            var model = JsonConvert.DeserializeObject<SelfGroupNotice>(msg);
            title = model.groupname;
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var getui = new GeTuiMessage
            {
                mtype = 13,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;
            message.OfflineExpireTime = 1000 * 3600 * 12;
            message.Data = template;
            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-新建聊天群组类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");
            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-新建聊天群组类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");
            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-新建聊天群组类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }


        /// <summary>
        /// 14  -->>   新建直播的通知（申志云）
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushLiveAPPInfoNotice(string deviceid, string touid, string msg)
        {
            var currentMethod = "PushLiveAPPInfoNotice";
            var title = "新建直播通知";
            var content = "新建直播";
            var model = JsonConvert.DeserializeObject<LiveAPPInfo>(msg);
            title = model.Name;
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var getui = new GeTuiMessage
            {
                mtype = 14,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;
            message.OfflineExpireTime = 1000 * 3600 * 12;
            message.Data = template;
            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-新建直播类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");
            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-新建直播类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");
            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-新建直播类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }


        /// <summary>
        /// 15  -->>   自建群编辑通知（刘静杰）
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushSelfGroupNoticeMore(string deviceid, string touid, SelfGroupNoticeMore model)
        {
            var currentMethod = "PushSelfGroupNoticeMore";
            var title = "群组动态通知";
            var content = "群组动态";
            title = model.groupname;
            if (model.type == 0)
            {
                content = model.membername + "新加入群组";
            }
            else if(model.type == 1)
            {
                content = model.membername+"已被移出群组";
            }
            else if (model.type == 2)
            {
                content = model.membername+"退出群组";
            }
            else if (model.type == 3)
            {
                content = "群组已经被解散";
            }
            else if (model.type == 4)
            {
                content = "修改群组信息动态";
            }

            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var getui = new GeTuiMessage
            {
                mtype = 15,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;
            message.OfflineExpireTime = 1000 * 3600 * 12;
            message.Data = template;
            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-自建群编辑通知类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");
            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-自建群编辑通知类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");
            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-自建群编辑通知类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }


        /// <summary>
        /// 16  -->>   公司OA平台发出的消息（杨立旭）
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushOANotice(string deviceid, string touid, MessageModel model)
        {
            var currentMethod = "PushOANotice";

            var title = "待办任务";// model.SchemeTypeName;
            var content = model.Content;
            IGtPush push = new IGtPush(HOST, APPKEY, MASTERSECRET);
            //var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.BootActivity;end";
            var intent = $"intent:#Intent;component=net.cnki.okms/.pages.base.IndexActivity;S.title={model.Content};S.url={model.DetailUrl};S.levelNoticeTpye=2;end";
            var getui = new GeTuiMessage
            {
                mtype = 16,
                mcontent = model,
            };
            TransmissionTemplate template = TransmissionTemplate(title, content, intent, JsonConvert.SerializeObject(getui));
            // 单推消息模型
            SingleMessage message = new SingleMessage();
            message.IsOffline = true;
            message.OfflineExpireTime = 1000 * 3600 * 12;
            message.Data = template;
            com.igetui.api.openservice.igetui.Target target = new com.igetui.api.openservice.igetui.Target();
            target.appId = APPID;
            target.clientId = deviceid;
            var messageJson = JsonConvert.SerializeObject(message);
            LoggerHelper.AndroidPush($"1.个推-OA平台类消息[{touid}]推送的消息内容为：{Environment.NewLine}{messageJson}");
            try
            {
                String pushResult = push.pushMessageToSingle(message, target);
                LoggerHelper.AndroidPush($"2.个推-OA平台类消息[{touid}]推送的返回结果：{Environment.NewLine}{pushResult}");
            }
            catch (RequestException e)
            {
                String requestId = e.RequestId;
                //发送失败后的重发
                String pushResult = push.pushMessageToSingle(message, target, requestId);
                LoggerHelper.AndroidPush($"3.个推-OA平台类消息[{touid}]发生了异常，重发后，服务端返回结果：----------------" + pushResult);
            }
        }        
    }
}