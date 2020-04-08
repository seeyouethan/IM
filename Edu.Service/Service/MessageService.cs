using Edu.Models.Models.Msg;
using Exceptionless.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Entity;
using Edu.Tools;

namespace Edu.Service.Service
{
    public static class MessageService
    {
        /// <summary>
        /// 获取要发送的消息List 通过这个方法将要发送的消息都处理成一个个SingleMsg对象，将图片和文字两种类型分开
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="fromuid">发送者ID</param>
        /// <param name="msgtime">发送时间</param>
        /// <param name="fromusername">发送者姓名</param>
        /// <param name="photo">发送者头像</param>
        /// <param name="groupid">所在群组ID</param>
        /// <param name="msgid">消息数据库中的msgid</param>
        /// <param name="type">type=0表示是存文本，type=2表示是单独的文件类型，type=3表是文本类型和图片类型的整合,type=4表示语音消息</param>
        /// <param name="fileurl">如果是文件/语音类型，则该字段存放的是文件的下载地址URL</param>
        /// <param name="duration">如果是语音消息，则记录语音消息的长度</param>
        /// <returns></returns>
        public static List<SingleMsg> GetMessageList(string message,string fromuid,string msgtime,string fromusername,string photo,string groupid, string msgid,int type,string fileurl,int duration)
        {
            var list = new List<SingleMsg>();
            // 0 1 3分别表示 文字、图片、图文混合，这三种情况一样
            if (type != 2&& type!=4)
            {
                /*利用正则表达式去判断，将消息分为文本、图片*/
                var msgQueue = RegexHelper.GetMessageDictionaryQueue(message);
                foreach (var msg in msgQueue)
                {
                    if (msg.Key.Contains("text"))
                    {
                        var item = new SingleMsg
                        {
                            uid = fromuid,
                            truename = fromusername,
                            photo = photo,
                            createtime = msgtime,
                            type = 0,
                            msg = msg.Value,
                            groupid = groupid,
                            msgid = msgid,
                            fileurl="",
                            duration = 0,
                        };
                        list.Add(item);
                    }
                    else if (msg.Key.Contains("img"))
                    {
                        var item = new SingleMsg
                        {
                            uid = fromuid,
                            truename = fromusername,
                            photo = photo,
                            createtime = msgtime,
                            type = 1,
                            msg = msg.Value,
                            groupid = groupid,
                            msgid = msgid,
                            fileurl = "",
                            duration = 0,
                        };
                        list.Add(item);
                    }
                }
            }
            else
            {
                var itemfile = new SingleMsg
                {
                    uid = fromuid,
                    truename = fromusername,
                    photo = photo,
                    createtime = msgtime,
                    type = type,
                    msg = message,
                    groupid = groupid,
                    msgid = msgid,
                    fileurl = fileurl,
                    duration= duration,
                };
                list.Add(itemfile);
            }
            return list;
        }
        /// <summary>
        /// 获取整合后的单条信息  将图片也作为字符串发送出去，图片用占位符代替 2018年12月20日 添加了文件消息，type=2
        /// </summary>
        /// <param name="message"></param>
        /// <param name="fromuid"></param>
        /// <param name="msgtime"></param>
        /// <param name="fromusername"></param>
        /// <param name="photo"></param>
        /// <param name="groupid"></param>
        /// <param name="msgid"></param>
        /// <param name="type">type=0表示是存文本，type=2表示是单独的文件类型，type=3表是文本类型和图片类型的整合,type=4表示语音消息</param>
        /// <param name="fileurl">如果是文件/语音类型，则该字段存放的是文件的下载地址URL</param>
        /// <param name="duration">如果是语音消息，则记录语音消息的长度</param>
        /// <returns></returns>
        public static SingleMsg GetSingleMessage(string message, string fromuid, string msgtime, string fromusername, string photo, string groupid,string msgid,int type,string fileurl,int duration)
        {
            //var linkurl = RegexHelper.GetLinkString(message);
            if (type!=2&& type!=4)
            {
                /*利用正则表达式去判断，将消息分为文本、图片*/
                var msgQueue = RegexHelper.GetMessageDictionaryQueue(message);
                var item = new SingleMsg
                {
                    msgid = msgid,
                    uid = fromuid,
                    truename = fromusername,
                    photo = photo,
                    createtime = msgtime,
                    type = type, //需要重新赋值
                    msg = "",//需要重新赋值
                    groupid = groupid,
                    fileurl = "",
                    duration=0,
                };
                foreach (var msg in msgQueue)
                {
                    if (msg.Key.Contains("text"))
                    {
                        item.msg += msg.Value;
                    }
                    else if (msg.Key.Contains("img"))
                    {
                        item.msg += "[$PICTURE$]";
                        item.imgs.Add(msg.Value);
                        item.type = 3;
                    }
                }
                return item;
            }
            else
            {
                var item = new SingleMsg
                {
                    msgid = msgid,
                    uid = fromuid,
                    truename = fromusername,
                    photo = photo,
                    createtime = msgtime,
                    type = type, 
                    msg = message,
                    groupid = groupid,
                    fileurl = fileurl,
                    duration = duration,
                };
                return item;
            }
        }

        public static List<SingleMsg> GetMessageListGroup(List<IMMsg> list)
        {
            var msgList = new List<SingleMsg>();
            foreach (var msg in list)
            {
                var photo= ConfigHelper.GetConfigString("sso_host_name") + "pic/" + msg.FromuID;
                var groupid = "";
                var type = 0;
                if (!string.IsNullOrEmpty(msg.Type)) type = Convert.ToInt32(msg.Type);
                if (msg.isgroup == 1) groupid = msg.TouID;
                var list0=GetMessageList(msg.Msg,msg.FromuID,msg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),msg.fromusername, photo, groupid,msg.ID.ToString(), type, msg.FileUrl,msg.Duration);
                if(list0.Any()) msgList.AddRange(list0);
            }
            if (msgList.Any()) msgList.Reverse();
            return msgList;
        }

        public static List<SingleMsg> GetMessageListFromList(List<IMMsg> list)
        {
            var msgList = new List<SingleMsg>();
            foreach (var msg in list)
            {
                var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + msg.FromuID;
                var groupid = "";
                if (msg.isgroup == 1) groupid = msg.TouID;
                var list0 = GetMessageList(msg.Msg, msg.FromuID, msg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msg.fromusername, photo, groupid, msg.ID.ToString(), Convert.ToInt32(msg.Type), msg.FileUrl, msg.Duration);
                if (list0.Any()) msgList.AddRange(list0);
            }
            return msgList;
        }

        /// <summary>
        /// 这个方法返回的List中的SingleMsg是整合后的单条信息，将图片也作为字符串发送出去，用占位符代替，2019年2月18日
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<SingleMsg> GetMessageListFromListNew(List<IMMsg> list)
        {
            var msgList = new List<SingleMsg>();
            foreach (var msg in list)
            {
                var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + msg.FromuID;
                var groupid = "";
                if (msg.isgroup == 1) groupid = msg.TouID;
                var amsg = GetSingleMessage(msg.Msg, msg.FromuID, msg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msg.fromusername, photo, groupid, msg.ID.ToString(), Convert.ToInt32(msg.Type), msg.FileUrl, msg.Duration);
                msgList.Add(amsg);
            }
            return msgList;
        }

        public static List<SingleMsg> GetFileMessageList(List<IMMsg> list)
        {
            var msgList = new List<SingleMsg>();
            foreach (var msg in list)
            {
                var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + msg.FromuID;
                var groupid = "";
                if (msg.isgroup == 1) groupid = msg.TouID;
                var list0 = GetMessageList(msg.Msg, msg.FromuID, msg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msg.fromusername, photo, groupid, msg.ID.ToString(), Convert.ToInt32(msg.Type), msg.FileUrl, msg.Duration);
                if (list0.Any()) msgList.AddRange(list0);
            }
            //if (msgList.Any()) msgList.Reverse();
            return msgList;
        }

        public static List<SingleMsg> GetImageMessageList(List<IMMsg> list)
        {
            var msgList = new List<SingleMsg>();
            foreach (var msg in list)
            {
                var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + msg.FromuID;
                var groupid = "";
                if (msg.isgroup == 1) groupid = msg.TouID;
                var list0 = GetImageMessages(msg.Msg, msg.FromuID, msg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msg.fromusername, photo, groupid, msg.ID.ToString(), Convert.ToInt32(msg.Type), msg.FileUrl, msg.Duration);
                if (list0.Any()) msgList.AddRange(list0);
            }
            return msgList;
        }

        public static List<SingleMsg> GetImageMessages(string message, string fromuid, string msgtime, string fromusername, string photo, string groupid, string msgid, int type, string fileurl, int duration)
        {
            var list = new List<SingleMsg>();
            // 1 3分别表示 图片、图文混合，这三种情况一样
            if (type == 1 || type == 3)
            {
                /*利用正则表达式去判断，将消息分为文本、图片*/
                var msgQueue = RegexHelper.GetMessageDictionaryQueue(message);
                list.AddRange(from msg in msgQueue
                    where msg.Key.Contains("img")
                    select new SingleMsg
                    {
                        uid = fromuid, truename = fromusername, photo = photo, createtime = msgtime, type = 1, msg = msg.Value, groupid = groupid, msgid = msgid, fileurl = "", duration = 0,
                    });
            }
            return list;
        }

    }
}
