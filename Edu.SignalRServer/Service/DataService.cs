using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Edu.Entity;
using Edu.Models.Models.Msg;
using Edu.Service;
using Edu.Service.Service;
using Edu.SignalRServer.Models;
using Edu.Tools;
using log4net;
using Newtonsoft.Json;

namespace Edu.SignalRServer.Service
{
    public class DataService
    {
        /// <summary>
        /// 将消息添加到数据库中 若添加失败，则返回0
        ///                    若是重复消息，则返回-1
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int InsertMsg(Msg msg)
        {
            try
            {
                var unitOfWork = new UnitOfWork();
                //查询是不是重发的（主要是关于移动端的）添加前先查询是否已经存在对应的数据
                if (!string.IsNullOrEmpty(msg.id1))
                {
                    var query = unitOfWork.DIMMsg.Get(p => p.FromuID == msg.fromuid && p.id1 == msg.id1).FirstOrDefault();
                    if (query != null)
                    {
                        LoggerHelper.Error("尝试添加一条重复的消息:"+JsonConvert.SerializeObject(msg));
                        return -1;
                    }
                }
                var immsg = new IMMsg
                {
                    FromuID = msg.fromuid,
                    CreateDate = DateTime.Now,
                    TouID = msg.touid,
                    CreateUser = msg.fromuid,
                    QuoteId = msg.quoteId,
                    QuoteContent = msg.quoteContent,
                    fromusername = msg.fromrealname,
                    tousername = msg.torealname,
                    isgroup = msg.isgroup,
                    Type = msg.msgtype.ToString(),
                    id1 = string.Empty,//默认为空
                    IsDel = 0,
                    NewFileName = string.Empty,
                    Msg = msg.msg,
                    IsNotice=false,
                    
                };
                if (!string.IsNullOrEmpty(msg.id1))
                {
                    immsg.id1 = msg.id1;
                }
                if (msg.msgtype == 0)
                {
                    //纯文本消息
                    immsg.Msg = msg.msg;
                }
                else if (msg.msgtype == 1)
                {
                    //单张图片，存放的是图片的url
                    //immsg.Msg = msg.msg;
                    immsg.FileUrl = msg.filename;
                }
                else if (msg.msgtype == 2)
                {
                    //文件消息
                    immsg.Msg = msg.filename;
                    immsg.FileUrl = msg.msg;
                    //暂时用Duration这个字段来存放文件的大小，以字节为单位
                    immsg.Duration = msg.duration;
                    /*
                     * 1.取出filecode
                     */
                    if (msg.msg.Contains("&fileCode="))
                    {
                        var index = msg.msg.LastIndexOf("&fileCode=");
                        var fileCode = msg.msg.Substring(index + 10);
                        immsg.NewFileName = fileCode;
                    }

                }
                else if (msg.msgtype == 3)
                {
                    //图文混合消息，
                    immsg.Msg = msg.msg;
                    immsg.ImgList = "";
                    foreach (var img in msg.imglist)
                    {
                        immsg.ImgList += img + ";";
                    }
                }
                else if (msg.msgtype == 4)
                {
                    //语音消息
                    immsg.Msg = "语音消息";
                    immsg.FileUrl = msg.msg;
                    immsg.Duration = msg.duration;
                }
                else if (msg.msgtype == 5)
                {
                    //地图类消息 用这个字段存放json
                    var p = new GDPosition();
                    p.latitude = msg.filename;
                    p.address = msg.msg;
                    immsg.Msg = JsonConvert.SerializeObject(p);
                }

                immsg.SubjectId = "";
                if (!string.IsNullOrEmpty(msg.subjectId))
                {
                    immsg.SubjectId = msg.subjectId;
                }



                msg.msgtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                unitOfWork.DIMMsg.Insert(immsg);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    msg.id0 = immsg.ID.ToString();

                    Task.Run(() =>
                    {
                        /*2019年5月13日 新增，更新最近聊天发送聊天记录的数据，这里只更新单人聊天*/
                        if (msg.isgroup == 0)
                        {
                            var query =
                                 unitOfWork.DRecentContacts.Get(
                                     p => (p.uID == msg.fromuid && p.ContactID == msg.touid) || (p.uID == msg.touid && p.ContactID == msg.fromuid))
                                     .FirstOrDefault();
                            if (query != null)
                            {
                                query.ContactDate = Convert.ToDateTime(msg.msgtime);
                                unitOfWork.DRecentContacts.Update(query);
                            }
                            else
                            {
                                var item = new RecentContacts();
                                item.uID = msg.fromuid;
                                item.ContactID = msg.touid;
                                item.ContactDate = DateTime.Now;
                                unitOfWork.DRecentContacts.Insert(item);
                            }
                            unitOfWork.Save();
                        }
                    });
                    return immsg.ID;
                }
                return 0;
            }
            catch (Exception ex)
            {
                //记录下异常
                LoggerHelper.Error(ex.ToString());
                return 0;
            }
            
        }

        public void InsertGeguiLog(string fromuid,string touid,string msgtime,string msgid,string devicetype,string deviceid,string content)
        {
            try
            {
                var unitOfWork = new UnitOfWork();
                //查询是不是重发的（主要是关于移动端的）添加前先查询是否已经存在对应的数据

                var model = new GetuiLog
                {
                    fromuid = fromuid,
                    touid = touid,
                    msgtime = msgtime,
                    resulttime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
                    msgid=msgid,
                    type = 0,
                    content = content,
                    remark = string.Empty,
                    devicetype=devicetype,
                    deviceid = deviceid,
                };
                unitOfWork.DGetuiLog.Insert(model);
                var result = unitOfWork.Save();
            }
            catch (Exception ex)
            {
                //记录下异常
                LoggerHelper.Error(ex.ToString());
            }
        }

        /// <summary>
        /// 向数据库中添加一条会议研讨数据
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool InsertConferenceMsg(ConferenceChatModel model)
        {
            try
            {
                var msg = new ConferenceMsg()
                {
                    conferenceid = model.conferenceId,
                    uid = model.uid,
                    truename = model.trueName,
                    photo = model.photo,
                    msgContent = model.msgContent,
                    msgtype = model.msgType,
                    datetime = DateTime.Now,
                    ext = "",
                };
                var unitOfWork = new UnitOfWork();
                unitOfWork.DConferenceMsg.Insert(msg);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return true;
                }
            }
            catch (Exception dbEx)
            {
                //记录下异常
                LoggerHelper.Error(dbEx.ToString());               
            }
            return false;
        }




        public List<string> GetWorkGroupMembers(string groupid)
        {
            var url = ConfigHelper.GetConfigString("GetWorkGroupMembers") + groupid;
            var resp = HttpWebHelper.HttpGet(url);
            var list = new List<string>();
            resp = resp.Replace("[", "").Replace("]", "");
            var arry = resp.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in arry)
            {
                list.Add(s.Replace("\"", ""));
            }
            return list;
        }

        public List<string> GetSelfGroupMembers(string groupid)
        {
            var url = ConfigHelper.GetConfigString("GetSelfGroupMembers") + groupid;
            var resp = HttpWebHelper.HttpGet(url);
            var list = new List<string>();
            resp = resp.Replace("[", "").Replace("]", "");
            var arry = resp.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in arry)
            {
                list.Add(s.Replace("\"", ""));
            }
            return list;
        }

        /// <summary>
        /// 根据id将对应的协同研讨/协作文档删除
        /// </summary>
        /// <param name="id"></param>
        public bool DeletePlanDiscuss(string id)
        {
            try
            {
                var unitOfWork = new UnitOfWork();
                var query = unitOfWork.DPlanDiscuss.Get(p => p.DiscussID == id).FirstOrDefault();
                if (query != null)
                {
                    query.IsDel = 1;
                    unitOfWork.DPlanDiscuss.Update(query);
                    var result = unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public bool RevertPlanDiscuss(string id)
        {
            try
            {
                var unitOfWork = new UnitOfWork();
                var query = unitOfWork.DPlanDiscuss.Get(p => p.PlanID == id && p.IsDel == 1).FirstOrDefault();
                if (query != null)
                {
                    query.IsDel = 0;
                    unitOfWork.DPlanDiscuss.Update(query);
                    var result = unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 更新或新建一条关于移动端设备的信息(同一个用户，在数据库中只能有一个设备记录) 
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="deviceid"></param>
        /// <param name="devicetoken"></param>
        /// <param name="devicetype"></param>
        /// <returns></returns>
        public bool UpdateUserDevice(string uid, string deviceid, string devicetoken,string devicetype)
        {
            var unitOfWork = new UnitOfWork();
            var query = unitOfWork.DUserDevice.Get(p => p.uid == uid).FirstOrDefault();
            if (query != null)
            {
                query.createdate = DateTime.Now;
                query.devicetoken = devicetoken;
                query.devicetype = devicetype;
                query.deviceid = deviceid;
                query.msgcount = 1;
                query.isonline = 1;
                unitOfWork.DUserDevice.Update(query);
            }
            else
            {
                var userDevice = new UserDevice
                {
                    createdate = DateTime.Now,
                    uid = uid,
                    devicetoken = devicetoken,
                    deviceid = deviceid,
                    msgcount = 1,
                    isonline = 1,
                    devicetype = devicetype,
                };
                unitOfWork.DUserDevice.Insert(userDevice);
            }
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取对应移动端的devicetoken和已经发送推送消息的个数、设备类型
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="msgcount"></param>
        /// <param name="devicetype"></param>
        /// <param name="isonline"></param>
        /// <returns></returns>
        public string GetUserDeviceToken(string uid,out int msgcount,out string devicetype,out bool isonline)
        {
            var unitOfWork = new UnitOfWork();
            var query = unitOfWork.DUserDevice.Get(p => p.uid == uid).FirstOrDefault();
            if (query != null)
            {
                msgcount = query.msgcount;
                devicetype = query.devicetype;
                isonline = query.isonline == 1;
                return query.devicetoken;
            }
            devicetype = "";
            msgcount = 0;
            isonline = false;
            return string.Empty;
        }

        /// <summary>
        /// 将对应的devicetoken的消息个数加1
        /// </summary>
        /// <param name="devicetoken"></param>
        /// <returns></returns>
        public bool UpdateUserDeviceTokenMsgcount(string devicetoken)
        {
            var unitOfWork = new UnitOfWork();
            var query = unitOfWork.DUserDevice.Get(p => p.devicetoken == devicetoken).FirstOrDefault();
            if (query != null)
            {
                query.msgcount = query.msgcount+1;
                query.createdate = DateTime.Now;
                unitOfWork.DUserDevice.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return true;
                }
            }
            return false;

        }

        /// <summary>
        /// 获取移动端离线消息推送开关设置
        /// </summary>
        /// <param name="uid">当前用户id</param>
        /// <param name="type">1 —>> IM新消息通知
        ///                    2 —>> 通知公告消息（群组类）
        ///                    3 —>> OA待办消息通知
        /// </param>
        /// <returns></returns>
        public bool GetNoticeSwitch(string uid,int type=1)
        {
            var unitOfWork = new UnitOfWork();
            var query = unitOfWork.DUserNoticeSwitch.Get(p => p.uid == uid).FirstOrDefault();
            if (query == null)
            {
                return true;
            }
            if (type == 1) {
                return query.type1 == 1;
            }else if (type == 2)
            {
                return query.type2 == 1;
            }else if (type == 3)
            {
                return query.type3 == 1;
            }
            return false;
        }

    }
}