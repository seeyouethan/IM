using Edu.Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Edu.Entity;
using Edu.Models.Models.Msg;
using Edu.Tools;
using Exceptionless.Json;

namespace Edu.Service.Service.Message
{
    public static class MsgServices
    {
        /// <summary>
        /// 根据web端页面输入消息的内容，来判断是文本消息，还是图文组合消息，将类型和结果都返回到引用参数中
        /// </summary>
        /// <param name="message"></param>
        /// <param name="msgContent"></param>
        /// <param name="msgtype"></param>
        /// <param name="imgList"></param>
        public static void GetMessageType(string message,out string msgContent,out int msgtype,out List<string> imgList)
        {
            msgContent = string.Empty;
            imgList =new List<string>();
            msgtype = 0;
            bool hasText = false;
            bool hasImg = false;
            var msgQueue = RegexHelper.GetMessageDictionaryQueue(message);
            foreach (var msg in msgQueue)
            {
                if (msg.Key.Contains("text"))
                {
                    msgContent += msg.Value;
                    hasText = true;
                }
                else if (msg.Key.Contains("img"))
                {
                    msgContent += "[$PICTURE$]";
                    imgList.Add(msg.Value);
                    hasImg = true;
                }
            }
            if (hasText && !hasImg)
            {
                //只有文本
                msgtype = 0;
            }
            if (!hasText && hasImg && msgQueue.Count==1)
            {
                //只有一张图片
                //这时候，msgContent中 存放的也是图片的地址
                msgtype = 1;
                msgContent = imgList[0];
            }
            if (hasImg && msgQueue.Count > 1)
            {
                //两张以上图片，就是图文混排
                //图文混排、多张图片同时发送
                msgtype = 3;
            }
        }
        

        public static Msg ImMsg2Msg(IMMsg item)
        {
            var msg=new Msg();
            msg.fromuid = item.FromuID;
            msg.touid = item.TouID;
            msg.isgroup = item.isgroup;
            msg.fromrealname = item.fromusername;
            msg.torealname = item.tousername;
            msg.id0 = item.ID.ToString();
            msg.id1 = item.id1;
            msg.msgtime=item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            msg.msgtype = Convert.ToInt32(item.Type);
            msg.filename = "";
            msg.duration = item.Duration;//默认为0  如果是文件类型，则存放文件的大小 单位是KB
            msg.imglist=new List<string>();
            msg.msg = item.Msg;
            if (item.QuoteId != null)
            {
                msg.quoteId = item.QuoteId;
            }else
            {

                msg.quoteId = 0;
            }
            msg.quoteContent = item.QuoteContent;
            if (msg.msgtype == 0)
            {
                //纯文本
                msg.msg = item.Msg;
            }
            else if (msg.msgtype == 1)
            {
                //单个图片
                msg.msg = item.Msg;
                msg.filename = item.FileUrl;
            }
            else if (msg.msgtype == 2)
            {
                //文件
                msg.msg = item.FileUrl;
                msg.filename = item.Msg;
                msg.duration = item.Duration;
            }
            else if (msg.msgtype == 3)
            {
                //图文混合
                msg.msg = item.Msg;
                if (!string.IsNullOrEmpty(item.ImgList))
                {
                    msg.imglist = item.ImgList.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                }
            }
            else if (msg.msgtype == 4)
            {
                //语音
                msg.msg = item.FileUrl;
                msg.duration = item.Duration;
            }
            else if (msg.msgtype == 5)
            {
                //地图签到类消息
                var p = JsonConvert.DeserializeObject<GDPosition>(item.Msg);
                msg.msg = p.address;
                msg.filename = p.latitude;
            }
            else if (msg.msgtype == 6)
            {
                //群组视频聊天消息 纯文本，忽略
            }
            else if (msg.msgtype == 7)
            {
                //视频类消息 已经存放了视频地址的url 忽略
            }
            else if (msg.msgtype == 8)
            {
                //回执类的消息 忽略
            }else if(msg.msgtype == 9 || msg.msgtype==10)
            {
                //协同文档、协同研讨类消息
                msg.filename = item.FileUrl;
            }

            msg.subjectId = "";
            msg.subjectTitle = "";
            if (!string.IsNullOrEmpty(item.SubjectId))
            {
                msg.subjectId = item.SubjectId;
            }

            return msg;
        }

        public static List<Msg> ImMsg2Msg(List<IMMsg> itemlist)
        {
            var list=new List<Msg>();
            foreach (var item in itemlist)
            {
                var msg = ImMsg2Msg(item);
                list.Add(msg);
            }
            return list;
        }

        /// <summary>
        /// 新增（重置）Redis中的缓存内容，添加一个Model
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="model"></param>
        public static void ResetRedisKeyValue<T>(string name,string key,T model)
        {
            var str0 = RedisHelper.Hash_Get<string>(name, key) ?? "";
            var msgList = JsonConvert.DeserializeObject<List<T>>(str0);
            if (msgList == null)
            {
                msgList = new List<T>();
            }
            msgList.Add(model);
            var strf = Newtonsoft.Json.JsonConvert.SerializeObject(msgList);
            RedisHelper.Hash_Remove(name, key);
            RedisHelper.Hash_Set<string>(name, key,strf);
        }
        /// <summary>
        /// 新增（重置）Redis中的缓存内容，添加一个Model的集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="model"></param>
        public static void ResetRedisKeyValue<T>(string name, string key, List<T> model)
        {
            var str0 = RedisHelper.Hash_Get<string>(name, key) ?? "";
            var msgList = JsonConvert.DeserializeObject<List<T>>(str0);
            if (msgList == null)
            {
                msgList = new List<T>();
            }
            msgList.AddRange(model);
            var strf = Newtonsoft.Json.JsonConvert.SerializeObject(msgList);
            RedisHelper.Hash_Remove(name, key);
            RedisHelper.Hash_Set<string>(name, key, strf);
        }

        /// <summary>
        /// 获取Redis中的缓存内容
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<T> GetRedisKeyValue<T>(string name, string key)
        {
            var str0 = RedisHelper.Hash_Get<string>(name, key) ?? "";
            var msgList = JsonConvert.DeserializeObject<List<T>>(str0);
            if (msgList == null)
            {
                msgList = new List<T>();
            }
            return msgList;
        }

        /// <summary>
        /// 移除未读聊天消息中的某一条消息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="key"></param>
        /// <param name="model"></param>
        public static void RemoveRedisKeyValue(string name, string key, IMMsg model)
        {
            var str0 = RedisHelper.Hash_Get<string>(name, key) ?? "";
            var msgList = JsonConvert.DeserializeObject<List<IMMsg>>(str0);
            if (msgList != null && msgList.Any())
            {
                var index = -1;
                for (int i = 0; i < msgList.Count; i++)
                {
                    if (msgList[i].ID == model.ID)
                    {
                        index = i;
                    }
                }
                msgList.RemoveAt(index);
                var strf = JsonConvert.SerializeObject(msgList);
                RedisHelper.Hash_Remove(name, key);
                RedisHelper.Hash_Set<string>(name, key, strf);
            }
        }

    }
}