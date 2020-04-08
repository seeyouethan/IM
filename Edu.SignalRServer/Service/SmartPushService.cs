using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Edu.SignalRServer.Models;
using Edu.Tools;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;
using log4net;
using Edu.Models.Models.Msg;
using Newtonsoft.Json;
using Edu.SignalRServer.Model;

namespace Edu.SignalRServer.Service
{
    public static class SmartPushService
    {
        public static async Task ApnsPushOne(DeviceMsg device)
        {
            var dataService =new DataService();
            var apnsServer = ConfigHelper.GetConfigString("ApnsServer");
            var certificateFile= ConfigHelper.GetConfigString("ApnspCertificateFile");
            var certificateFilePwd = ConfigHelper.GetConfigString("ApnsCertificateFilePwd");
            var server = ApnsConfiguration.ApnsServerEnvironment.Sandbox;//0
            if (apnsServer == "1")
            {
                server = ApnsConfiguration.ApnsServerEnvironment.Production;
            }
            var config = new ApnsConfiguration(server, certificateFile, certificateFilePwd);
            // Create a new broker
            var apnsBroker = new ApnsServiceBroker(config);
            // Wire up events
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = ex as ApnsNotificationException;
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;
                        LoggerHelper.IOSPush($"3.Apple Notification Failed!{Environment.NewLine}ID={apnsNotification.Identifier}{Environment.NewLine} Code={statusCode}{Environment.NewLine}Uid={device.uid}{Environment.NewLine}DeviceToken={notification.DeviceToken}{Environment.NewLine}deviceMsgId={device.id}");
                        //Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                    }
                    else
                    {
                        LoggerHelper.IOSPush($"4.Apple Notification Failed for some unknown reason : {ex.InnerException}{Environment.NewLine}deviceMsgId={device.id}");
                    }
                    return true;
                });
            };

            apnsBroker.OnNotificationSucceeded += (notification) => {
                //LoggerHelper.IOSPush($"Apple Notification Sent!, Uid={device.uid},DeviceToken=" + notification.DeviceToken);
                //发送成功后，需要将msgcount 加1
                var b = dataService.UpdateUserDeviceTokenMsgcount(notification.DeviceToken);
                
                LoggerHelper.IOSPush($"2.给【{device.uid}】推送的【{device.id}】消息返回结果为:{b}");
            };
            // Start the broker
            apnsBroker.Start();
            apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = device.devicetoken,
                //Payload = JObject.Parse("{\"aps\":{\"alert\":\"" + device.title + "\",\"badge\":" + device.msgcount + ",\"sound\":\"default\"},\"type\":" + device.msgtype + ",\"content\":" + device.msgcontent + ",\"createdate\":\"" + device.msgtime + "\"}"),
                //将alert分为title和body字段
                Payload = JObject.Parse("{\"aps\":{\"alert\":{\"title\":\""+device.title+ "\",\"body\" : \"" + device.body + "\"},\"badge\":" + device.msgcount + ",\"sound\":\"default\"},\"type\":" + device.msgtype + ",\"content\":" + device.msgcontent + ",\"createdate\":\"" + device.msgtime + "\"}")
            });
            apnsBroker.Stop();
        }

        /// <summary>
        /// 1.聊天消息 IOS离线推送 
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="count"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushMessageToSingle(string touid,string deviceToken,int count, Msg msg)
        {
            LoggerHelper.IOSPush($"0.聊天消息 IOS离线推送，开始给[{touid}]推送消息{Environment.NewLine},devicetoken={deviceToken}");


            var messageJson = JsonConvert.SerializeObject(msg);

            var deviceMsg = new DeviceMsg();
            deviceMsg.id = Guid.NewGuid().ToString();
            deviceMsg.msgtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            deviceMsg.msgtype = 1;
            deviceMsg.msgcount = count;
            deviceMsg.devicetoken = deviceToken;
            deviceMsg.uid = touid;
            deviceMsg.msgcontent= messageJson;

            deviceMsg.title = msg.fromrealname;

            var fromPersonName = "";
            var content = "";
            
            //群组消息
            if (msg.isgroup == 1)
            {
                deviceMsg.title = msg.torealname;
                fromPersonName = msg.fromrealname + ":";
            }
            content = fromPersonName + msg.msg;
            if (msg.msgtype == 1)
            {
                content = fromPersonName + "[图片]";
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

            deviceMsg.body = content;
            var deviceMsgJson = JsonConvert.SerializeObject(deviceMsg);
            LoggerHelper.IOSPush($"1.聊天消息 IOS离线推送，给[{touid}]推送的消息内容为：{Environment.NewLine}{deviceMsgJson}");
            ApnsPushOne(deviceMsg);
        }

        /// <summary>
        /// 2 群组动态(工作群)
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="count"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushGroupDynamic(string touid, string deviceToken, int count, string msg)
        {
            LoggerHelper.IOSPush($"0.群组动态，IOS离线推送，开始给[{touid}]推送消息{Environment.NewLine},devicetoken={deviceToken}");           

            var deviceMsg = new DeviceMsg();
            deviceMsg.id = Guid.NewGuid().ToString();
            deviceMsg.msgtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            deviceMsg.msgtype = 2;
            deviceMsg.msgcount = count;
            deviceMsg.devicetoken = deviceToken;
            deviceMsg.uid = touid;
            deviceMsg.msgcontent = msg;

            deviceMsg.title = "群组动态通知";
            deviceMsg.body = "新的群组动态";



            var deviceMsgJson = JsonConvert.SerializeObject(deviceMsg);
            LoggerHelper.IOSPush($"1.群组动态, IOS离线推送，给[{touid}]推送的消息内容为：{Environment.NewLine}{deviceMsgJson}");
            ApnsPushOne(deviceMsg);
        }

        /// <summary>
        /// 3 群组公告
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="count"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static async Task PushGroupNotice(string touid, string deviceToken, int count, string msg)
        {

            LoggerHelper.IOSPush($"0.群组公告，IOS离线推送，开始给[{touid}]推送消息{Environment.NewLine},devicetoken={deviceToken}");


            var model = JsonConvert.DeserializeObject<AppGroupNotice>(msg);

            var deviceMsg = new DeviceMsg();
            deviceMsg.id = Guid.NewGuid().ToString();
            deviceMsg.msgtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            deviceMsg.msgtype = 3;
            deviceMsg.msgcount = count;
            deviceMsg.devicetoken = deviceToken;
            deviceMsg.uid = touid;
            deviceMsg.msgcontent = msg;

            deviceMsg.title = "群组公告";
            deviceMsg.body = model.Title;



            var deviceMsgJson = JsonConvert.SerializeObject(deviceMsg);
            LoggerHelper.IOSPush($"1.群组公告, IOS离线推送，给[{touid}]推送的消息内容为：{Environment.NewLine}{deviceMsgJson}");
            ApnsPushOne(deviceMsg);
            
        }

        /// <summary>
        /// 15  -->>   自建群编辑通知（刘静杰）
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="count"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static async Task PushSelfGroupNoticeMore(string touid, string deviceToken, int count, SelfGroupNoticeMore model)
        {
            var messageJson = JsonConvert.SerializeObject(model);
            LoggerHelper.IOSPush($"0.自建群编辑通知，IOS离线推送，开始给[{touid}]推送消息{Environment.NewLine},devicetoken={deviceToken}");
            var deviceMsg = new DeviceMsg();
            deviceMsg.id = Guid.NewGuid().ToString();
            deviceMsg.msgtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            deviceMsg.msgtype = 2;
            deviceMsg.msgcount = count;
            deviceMsg.devicetoken = deviceToken;
            deviceMsg.uid = touid;
            deviceMsg.msgcontent = messageJson;

            deviceMsg.title = model.groupname;
            deviceMsg.body = "新的群组动态";
            if (model.type == 0)
            {
                deviceMsg.body = model.membername + "新加入群组";
            }
            else if (model.type == 1)
            {
                deviceMsg.body = model.membername + "已被移出群组";
            }
            else if (model.type == 2)
            {
                deviceMsg.body = model.membername + "退出群组";
            }
            else if (model.type == 3)
            {
                deviceMsg.body = "群组已经被解散";
            }
            else if (model.type == 4)
            {
                deviceMsg.body = "修改群组信息动态";
            }
            var deviceMsgJson = JsonConvert.SerializeObject(deviceMsg);
            LoggerHelper.IOSPush($"1.自建群编辑通知, IOS离线推送，给[{touid}]推送的消息内容为：{Environment.NewLine}{deviceMsgJson}");
            ApnsPushOne(deviceMsg);            
        }

        /// <summary>
        /// 16.公司OA平台发出的消息（杨立旭）
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="deviceToken"></param>
        /// <param name="count"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static async Task PushOANotice(string touid, string deviceToken, int count, MessageModel model)
        {
            LoggerHelper.IOSPush($"0.OA通知消息 IOS离线推送，开始给[{touid}]推送消息{Environment.NewLine},devicetoken={deviceToken}");


            var messageJson = JsonConvert.SerializeObject(model);

            var deviceMsg = new DeviceMsg();
            deviceMsg.id = Guid.NewGuid().ToString();
            deviceMsg.msgtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            deviceMsg.msgtype = 16;
            deviceMsg.msgcount = count;
            deviceMsg.devicetoken = deviceToken;
            deviceMsg.uid = touid;
            deviceMsg.msgcontent = messageJson;

            deviceMsg.title = "待办任务";//model.SchemeTypeName;
            deviceMsg.body = model.Content;



            var deviceMsgJson = JsonConvert.SerializeObject(deviceMsg);
            LoggerHelper.IOSPush($"1.OA通知消息 IOS离线推送，给[{touid}]推送的消息内容为：{Environment.NewLine}{deviceMsgJson}");
            ApnsPushOne(deviceMsg);
        }

    }
}