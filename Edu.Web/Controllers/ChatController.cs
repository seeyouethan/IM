using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Edu.Entity;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.Tools;
using Exceptionless.Json;
using Edu.Service.Service.Message;

namespace Edu.Web.Controllers
{
    public class ChatController : BaseControl
    {
        // GET: Chat
        public  ActionResult Index(string touid, string connid, string tousername, string roomid = "", bool isgroup = false)
        {
            /*
             * 打开聊天窗口 touid表示是要聊天的对象的uid  connid如果为空则表示该用户离线
             */
            /*添加/修改最近联系人
              2018年6月14日  最近联系人 应该是互相的关系 两个人共用一条数据 即两个人只要有聊天行为 那么这两个人的最近联系人   互相都为对方*/
            var selfuid = LoginUserService.ssoUserID;
            ViewBag.isGroup = 0;
           
            if (isgroup)
            {
                ViewBag.isGroup = 1;
                /*移除在Redis中存储的未读的群组消息 */
                /* 不移除这个groupid了，留在缓存中，可以作为最近发生聊天记录的群id --2018年12月14日 修改*/
                /*2018年12月27日 修改 移除这个redis否则页面上一直会有显示未读的群聊信息*/
                var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", selfuid) ?? new List<String>();
                if (unreadgroupmsg.Contains(touid))
                {
                    unreadgroupmsg.Remove(touid);
                    RedisHelper.Hash_Remove("IMUnreadGroupMsg", selfuid);
                    RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", selfuid, unreadgroupmsg);
                }
            }
            ViewBag.isOnLine = 0;
            if (connid != "")
            {
                ViewBag.isOnLine = 1;
            }
            ViewBag.roomid = "";
            if (!string.IsNullOrEmpty(roomid))
            {
                ViewBag.roomid = roomid;
            }
            /*再从redis中查找一下connid*/
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            if (touser != null)
            {
                connid = touser.ConnectionId;
                ViewBag.isOnLine = 1;
            }
            ViewBag.touid = touid;
            ViewBag.connid = connid;
            ViewBag.sefconnid = "";
            var self = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", selfuid);
            if (self != null)
            {
                ViewBag.sefconnid = self.ConnectionId;
            }
            ViewBag.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            ViewBag.touserphoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + touid;
            if (isgroup)
                ViewBag.touserphoto = "/im/Content/Images/group.png";
            ViewBag.fromusername = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.tousername = tousername;
            return View();
        }


        public ActionResult UserChatContent(string touid, string connid, string tousername, string roomid = "", bool isgroup = false)
        {

            var selfuid = LoginUserService.ssoUserID;
            ViewBag.unitId = ssoUserOfWork.GetUserByID(selfuid).OrgId;
            ViewBag.isGroup = 0;
            if (isgroup) ViewBag.isGroup = 1;
            //默认为离线状态
            ViewBag.isOnLine = 0;
            if (connid != "")
            {
                ViewBag.isOnLine = 1;
            }
            ViewBag.roomid = "";
            if (!string.IsNullOrEmpty(roomid))
            {
                ViewBag.roomid = roomid;
            }
            /*再从redis中查找一下connid*/
            var touser = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", touid);
            if (touser != null)
            {
                connid = touser.ConnectionId;
                ViewBag.isOnLine = 1;
            }
            ViewBag.touid = touid;
            //ViewBag.connid = connid;
            ViewBag.sefconnid = "";
            //var self = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", selfuid);
            //if (self != null)
            //{
            //    ViewBag.sefconnid = self.ConnectionId;
            //}
            ViewBag.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            ViewBag.touserphoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + touid;
            //if (isgroup)
            //    ViewBag.touserphoto = "/im/Content/Images/group.png";
            ViewBag.fromusername = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.tousername = tousername;
            return View();


            /*新版-未完成
            var selfuid = LoginUserService.ssoUserID;
            ViewBag.unitId = ssoUserOfWork.GetUserByID(selfuid).OrgId;
            ViewBag.isGroup = 0;

            if (isgroup)
            {
                ViewBag.isGroup = 1;

            }
            //默认为离线状态
            ViewBag.isOnLine = 0;
            if (connid != "")
            {
                ViewBag.isOnLine = 1;
            }
            ViewBag.roomid = "";
            if (!string.IsNullOrEmpty(roomid))
            {
                ViewBag.roomid = roomid;
            }
            
            
            ViewBag.touid = touid;
            
            ViewBag.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            ViewBag.touserphoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + touid;
            
            ViewBag.fromusername = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.tousername = tousername;
            return View();*/
        }


        /// <summary>
        /// 取最近聊天记录 5条 显示在聊天窗口中
        /// 6月13日补充需求：如果有未读的聊天消息 则取所有未读的聊天消息 如果没有未读的聊天消息 则取最近5条聊天消息
        /// 未读的聊天消息从缓存中取  取到之后将其删除
        /// </summary>
        /// <param name="touid"></param>
        /// <returns></returns>
        public ActionResult RecentlyChat(string touid)
        {
            //先从缓存中取未读的消息，如果缓存中没有未读的消息，那么再从数据库中取最近的5条聊天记录
            var selfuid = LoginUserService.ssoUserID;
            var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", selfuid);
            if (all != null && all.Any())
            {
                var queryUnreadMsg =
                all.Where(p => p.fromuid == touid);
                var unreadMsg = queryUnreadMsg as Msg[] ?? queryUnreadMsg.ToArray();
                if (unreadMsg.Any())
                {
                    /*从缓存中删除这些消息*/
                    foreach (var msg in unreadMsg)
                    {
                        all.Remove(msg);
                    }
                    RedisHelper.Hash_Remove("IMMsg", selfuid);
                    MsgServices.ResetRedisKeyValue<Msg>("IMMsg", selfuid, all);
                    return PartialView("_RecentlyChat", unreadMsg);
                }
            }
            var listResult = new List<Msg>();
            var query =
                unitOfWork.DIMMsg.Get(
                    p => ((p.FromuID == selfuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == selfuid)) && p.IsDel != 1).OrderByDescending(p => p.CreateDate).Take(5).OrderBy(p => p.CreateDate);

            foreach (var msg in query)
            {
                var singleMsg = MsgServices.ImMsg2Msg(msg);
                listResult.Add(singleMsg);
            }

            return PartialView("_RecentlyChat", listResult);
        }

        public JsonResult HistoryChatNew(string touid, DateTime dateTime, int pageNumber = 1, string msg = "")
        {
            return null;
        }


        /// <summary>
        /// 历史聊天记录(根据传入参数的不同 分为两种查询方式 一种是按照时间来查询 一种是按照聊天内容来查询)
        /// </summary>
        /// <returns></returns>
        public ActionResult HistoryChat(string touid, DateTime dateTime, int pageNumber = 1, string msg = "")
        {
            List<HistoryChatEntity> list = new List<HistoryChatEntity>();
            if (msg == "")
            {
                /*聊天记录 根据当前日期 选取该日期的聊天就 然后执行翻页*/
                dateTime = dateTime.Date;
                var fromuid = LoginUserService.ssoUserID;
                var dateTime2 = dateTime.AddDays(1);
                var query =
                    unitOfWork.DIMMsg.Get(
                        p =>
                            ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) &&
                            (p.CreateDate.Value >= dateTime && p.CreateDate.Value <= dateTime2) && p.IsDel != 1)
                        .OrderByDescending(p => p.CreateDate);

                Paging paging = new Paging();

                paging.PageNumber = pageNumber;
                paging.PageSiz = 20;
                paging.Amount = query.Count();

                ViewBag.pageNo = pageNumber; //当前页码 1
                ViewBag.pageCount = paging.PageCount;

                var pageQuery2 =
                    query.Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).OrderBy(p => p.CreateDate).ToList();
                foreach (var imMsg in pageQuery2)
                {
                    var entity = new HistoryChatEntity();
                    entity.TrueName = imMsg.fromusername;
                    entity.Msg = imMsg.Msg;
                    entity.ChatTime = imMsg.CreateDate.Value.ToString("G");
                    list.Add(entity);
                }
                if (list.Any())
                {
                    return Json(new { r = true, list = list, pageCount = paging.PageCount, pageNo = pageNumber },
                        JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //根据内容查询
                var fromuid = LoginUserService.ssoUserID;
                var query =
                    unitOfWork.DIMMsg.Get(
                        p => ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.Msg.Contains(msg) && p.IsDel != 1).OrderByDescending(p => p.CreateDate);
                Paging paging = new Paging();
                paging.PageNumber = pageNumber;
                paging.PageSiz = 20;
                paging.Amount = query.Count();

                ViewBag.pageNo = pageNumber;//当前页码 1
                ViewBag.pageCount = paging.PageCount;

                var pageQuery2 = query.Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).OrderBy(p => p.CreateDate);

                var fromuser = unitOfWork.DUserInfo.Get(p => p.uID == fromuid).FirstOrDefault();
                var touser = unitOfWork.DUserInfo.Get(p => p.uID == touid).FirstOrDefault();
                foreach (var imMsg in pageQuery2)
                {
                    var entity = new HistoryChatEntity();
                    entity.TrueName = imMsg.FromuID == LoginUserService.ssoUserID ? fromuser.TrueName : touser.TrueName;
                    entity.Msg = imMsg.Msg;
                    entity.ChatTime = imMsg.CreateDate.Value.ToString("G");
                    list.Add(entity);
                }
                if (list.Any())
                {
                    return Json(new { r = true, list = list, pageCount = paging.PageCount, pageNo = pageNumber }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 关键字为空 日期为空
        /// </summary>
        /// <returns></returns>
        public List<HistoryChatEntity> Query00(string fromuid, string touid, int pageSize, int pageNumber, int queryCount)
        {
            var query =
                    unitOfWork.DIMMsg.Get(
                        p => ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.IsDel != 1)
                        .OrderByDescending(p => p.CreateDate).Skip(pageSize * (pageNumber - 1)).Take(pageSize).OrderBy(p => p.CreateDate).ToList();
            List<HistoryChatEntity> list = new List<HistoryChatEntity>();
            foreach (var imMsg in query)
            {
                var entity = new HistoryChatEntity();
                entity.TrueName = imMsg.fromusername;
                entity.Msg = imMsg.Msg;
                entity.ChatTime = imMsg.CreateDate.Value.ToString("G");
                list.Add(entity);
            }
            return list;
        }
        /// <summary>
        /// 关键字不为空 日期为空
        /// </summary>
        /// <returns></returns>
        public List<HistoryChatEntity> Query01()
        {
            return null;
        }
        /// <summary>
        /// 关键字为空 日期不为空
        /// </summary>
        /// <returns></returns>
        public List<HistoryChatEntity> Query02()
        {
            return null;
        }
        /// <summary>
        /// 关键字不为空 日期不为空
        /// </summary>
        /// <returns></returns>
        public List<HistoryChatEntity> Query03()
        {
            return null;
        }

        /// <summary>
        /// 发送聊天消息  拆解聊天消息
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SendMsg(string touid, string msg, string fromusername, string tousername, int isgroup = 0)
        {
            var fromuid = LoginUserService.ssoUserID;
            var item = new IMMsg
            {
                FromuID = fromuid,
                CreateDate = DateTime.Now,
                Msg = msg,
                TouID = touid,
                CreateUser = fromuid,
               
                fromusername = fromusername,
                tousername = tousername,
                isgroup = isgroup //是否是群组消息标记
            };
            var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            unitOfWork.DIMMsg.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true, msgtime = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msgcontent = item.Msg, photo = photo, msgid = item.ID }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        

        [ValidateInput(false)]
        public ActionResult AddMsgToRedisNew(string model)
        {
            try
            {
                var msg = new Msg(); //收到的消息
                var selfuid = LoginUserService.ssoUserID;
                msg = JsonConvert.DeserializeObject<Msg>(model);
                if (msg.isgroup == 0)
                {
                    //单人消息
                    MsgServices.ResetRedisKeyValue<Msg>("IMMsg", selfuid, msg);
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }else if (msg.isgroup == 1)
                {
                    //群组消息
                    MsgServices.ResetRedisKeyValue<Msg>("IMGroupMsg", msg.touid + selfuid, msg);
                    var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", selfuid) ?? new List<String>();
                    if (!unreadgroupmsg.Contains(msg.touid))
                    {
                        unreadgroupmsg.Add(msg.touid);
                        RedisHelper.Hash_Remove("IMUnreadGroupMsg", selfuid);
                        RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", selfuid, unreadgroupmsg);
                    }
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false, error = "isgroup数据异常"+msg.isgroup }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { r = false,error=ex.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }





        /// <summary>
        /// 邀请视频 右下角弹出窗口
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="fromusername"></param>
        /// <param name="connid"></param>
        /// <param name="roomid"></param>
        /// <returns></returns>
        public ActionResult VideoChatRequest(string fromuid, string fromusername, string connid,string roomid)
        {
            ViewBag.fromuid = fromuid;
            ViewBag.fromusername = fromusername;
            ViewBag.connid = connid;
            ViewBag.roomid = roomid;
            return View();
        }

        /// <summary>
        /// 视频聊天窗口
        /// </summary>
        /// <returns></returns>
        public ActionResult VideoChatWindow(string roomid,string touid)
        {
            ViewBag.fromuid = LoginUserService.ssoUserID;
            ViewBag.touid = touid;
            return PartialView("VideoChatWindow", roomid);
        }

        public ActionResult VideoTest()
        {
            return View();
        }



        public ActionResult GroupMsgNotice()
        {
            return View();
        }

        /// <summary>
        /// 查询历史聊天记录中的文件类型的消息，这个方法返回的仅仅有文件类型消息
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="isgroup"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public JsonResult GetFileMessage(string fromuid, string touid, bool isgroup, int pageNo,int pageSize=20)
        {
            var list = new List<Msg>();
            try
            {
                var dt = DateTime.Now;
                var totalCount = 0;
                if (isgroup)
                {
                    var query =
                        unitOfWork.DIMMsg.Get(
                            p => p.Type == "2" && p.TouID == touid && p.CreateDate <= dt && p.IsDel != 1).OrderByDescending(p => p.CreateDate);
                    if (query.Any())
                    {
                        var queryResult = query.Skip(pageNo - 1).Take(pageSize);
                        list = MsgServices.ImMsg2Msg(queryResult.ToList());
                        totalCount = query.Count();
                    }
                }
                else
                {
                    var query =
                           unitOfWork.DIMMsg.Get(
                               p => p.Type == "2" && ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.CreateDate <= dt && p.IsDel != 1).OrderByDescending(p => p.CreateDate);
                    if (query.Any())
                    {
                        var queryResult = query.Skip(pageNo - 1).Take(pageSize);
                        list = MsgServices.ImMsg2Msg(queryResult.ToList());
                        totalCount = query.Count();
                    }
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = totalCount
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
               new
               {
                   Success = false,
                   Content = "",
                   Error = ex.ToString(),
                   Message = "查询失败",
                   Count = 0,
                   Total = 0
               },
               JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 查询历史聊天记录中的图片文件，这个方法返回的仅仅有图片文件
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="isgroup"></param>
        /// <param name="pageNo">从1开始</param>
        /// <param name="pageSize">每页默认20个</param>
        /// <returns></returns>
        public JsonResult GetImageMessage(string fromuid, string touid, bool isgroup, int pageNo,int pageSize=20)
        {
            var list = new List<Msg>();
            try
            {
                var dt = DateTime.Now;
                var totalCount = 0;
                if (isgroup)
                {
                    var query =
                        unitOfWork.DIMMsg.Get(
                            p => (p.Type == "1"||p.Type == "3") && p.TouID == touid && p.CreateDate <= dt && p.IsDel != 1).OrderByDescending(p => p.CreateDate);
                    if (query.Any())
                    {
                        var queryResult = query.Skip(pageNo - 1).Take(pageSize);
                        list = MsgServices.ImMsg2Msg(queryResult.ToList());
                        totalCount = query.Count();
                    }
                }
                else
                {
                    var query =
                           unitOfWork.DIMMsg.Get(
                               p => (p.Type == "1" || p.Type == "3") && ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.CreateDate <= dt && p.IsDel != 1).OrderByDescending(p => p.CreateDate);
                    if (query.Any())
                    {
                        var queryResult = query.Skip(pageNo - 1).Take(pageSize);
                        list = MsgServices.ImMsg2Msg(queryResult.ToList());
                        totalCount = query.Count();
                    }
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = totalCount
                    },
                    JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
               new
               {
                   Success = false,
                   Content = "",
                   Error = ex.ToString(),
                   Message = "查询失败",
                   Count = 0,
                   Total = 0
               },
               JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// web页面聊天输入窗口输入文本 ，通过这个方法，返回对应的类型
        /// 0 纯文本 1单张图片 3 图文混合
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetMessageType(string message)
        {
            try
            {

                var msgContent = string.Empty;
                var msgType = 0;
                var imgList = new List<string>();
                MsgServices.GetMessageType(message, out msgContent, out msgType, out imgList);

                //去掉html标签
                string strText = System.Text.RegularExpressions.Regex.Replace(msgContent, @"<[^>]*>", "");
                strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");


                return Json(
                new
                {
                    Success = true,
                    Content = strText,
                    msgType = msgType,
                    imgList = imgList,
                },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
               new
               {
                   Success = true,
                   Content=ex.ToString()
               },
               JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult Meeting(string groupid,string groupname)
        {
            var uid = LoginUserService.ssoUserID;
            //roomid即群组id
            ViewBag.roomid = groupid;
            ViewBag.uid = uid;
            ViewBag.trueName = LoginUserService.User.TrueName;
            ViewBag.groupName = groupname+"群组视频会议";

            var query = unitOfWork.DConferenceDiscuss.Get(p => p.groupid == groupid).FirstOrDefault();
            var discussid = "";
            if (query != null)
            {
                discussid = query.discussid;
            }
            ViewBag.discussid = discussid;
            return View("VideoConference");
        }

        public ActionResult TRTCMeeting(string groupid, string groupname)
        {

            return JanusMeeting(groupid, groupname);
        }

        public ActionResult TRTCMeetingNew(string groupid, string groupname)
        {
            var uid = LoginUserService.ssoUserID;
            //roomid即群组id
            ViewBag.roomid = groupid;
            ViewBag.uid = uid;
            ViewBag.trueName = LoginUserService.User.TrueName;
            ViewBag.groupName = System.Web.HttpUtility.UrlDecode(groupname) + "群组视频会议";


            //随机的房间号

            if (groupid.Length == 36)
            {
                byte[] buffer = Guid.Parse(groupid).ToByteArray();
                ViewBag.meetingid = BitConverter.ToInt32(buffer, 0);
            }
            else
            {
                ViewBag.meetingid = Convert.ToInt32(groupid);
            }


            var query = unitOfWork.DConferenceDiscuss.Get(p => p.groupid == groupid).FirstOrDefault();
            var discussid = "";
            if (query != null)
            {
                discussid = query.discussid;
            }
            ViewBag.discussid = discussid;
            return View("VideoConferenceTRTC");
        }


        public ActionResult JanusMeeting(string groupid, string groupname)
        {
            var isJanus = ConfigHelper.GetConfigString("VideoConference");
            if(string.IsNullOrEmpty(isJanus) || isJanus != "Janus")
            {
                return TRTCMeetingNew(groupid, groupname);
            }

            var uid = LoginUserService.ssoUserID;
            //roomid即群组id
            ViewBag.roomid = groupid;
            ViewBag.uid = uid;
            ViewBag.trueName = LoginUserService.User.TrueName;
            ViewBag.groupName = System.Web.HttpUtility.UrlDecode(groupname) + "群组视频会议";

            

            if (groupid.Length == 36)
            {
                byte[] buffer = Guid.Parse(groupid).ToByteArray();
                ViewBag.meetingid = BitConverter.ToInt32(buffer, 0);//roomid只能用int类型字符串
            }
            else
            {
                ViewBag.meetingid = Convert.ToInt32(groupid);
            }

            if (ViewBag.meetingid < 0)
            {
                ViewBag.meetingid = -ViewBag.meetingid;
            }


            var query = unitOfWork.DConferenceDiscuss.Get(p => p.groupid == groupid).FirstOrDefault();
            var discussid = "";
            if (query != null)
            {
                discussid = query.discussid;
            }
            ViewBag.discussid = discussid;
            return View("VideoConferenceJanus");
        }

        public ActionResult GetGroupMembers(string groupid)
        {
            var queryMembers = unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
            if (queryMembers != null && queryMembers.Any())
            {
                if (queryMembers.ToList().Select(p => p.UserID).Contains(LoginUserService.ssoUserID))
                {
                    return Json(new
                    {
                        Success = true,
                        Content = queryMembers.ToList(),
                        Error = "",
                        Message = "查询成功",
                        Count = queryMembers.ToList().Count,
                        Total = queryMembers.ToList().Count
                    },
                JsonRequestBehavior.AllowGet);

                }
                return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "您不属于该群组成员，无法加入视频会议",
                        Message = "查询失败",
                        Count = "",
                        Total = ""
                    },
                    JsonRequestBehavior.AllowGet);
            }


            return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "群组成员查询结果为空",
                    Message = "查询失败",
                    Count = "",
                    Total = ""
                },
                JsonRequestBehavior.AllowGet);
        }
    }

}