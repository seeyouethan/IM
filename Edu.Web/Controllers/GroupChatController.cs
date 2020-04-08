using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Edu.Entity;
using Edu.Models;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.Service.Service;
using Edu.Service.Service.Message;
using Edu.Tools;
using KNet.AAMS.Utility.Extensions;

namespace Edu.Web.Controllers
{
    public class GroupChatController : BaseControl
    {
        // GET: GroupChat
        public async Task<ActionResult> Index(string groupid, string groupname)
        {
            var selfuid = LoginUserService.ssoUserID;

            /*移除在Redis中存储的未读的群组消息 */
            await Task.Run(() =>
            {
                var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", selfuid) ?? new List<String>();
                if (unreadgroupmsg.Contains(groupid))
                {
                    unreadgroupmsg.Remove(groupid);
                    RedisHelper.Hash_Remove("IMUnreadGroupMsg", selfuid);
                    RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", selfuid, unreadgroupmsg);
                }
            });

            /*再从redis中查找一下connid*/
            ViewBag.sefconnid = "";
            var self = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", selfuid);
            if (self != null)
            {
                ViewBag.sefconnid = self.ConnectionId;
            }
            //发送给群组的消息的touid存的为群组的groupid
            ViewBag.touid = groupid;
            ViewBag.fromusername = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.tousername = groupname;
            ViewBag.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            return View();
        }

        /// <summary>
        /// 取最近聊天记录 这是群聊的最近聊天记录 2018年12月21日 修改
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public ActionResult RecentlyChat(string groupid)
        { //先从缓存中取未读的消息，如果缓存中没有未读的消息，那么再从数据库中取最近的5条聊天记录
            var selfuid = LoginUserService.ssoUserID;
            //var all =  RedisHelper.Hash_Get<List<SingleMsg>>("IMGroupMsg",groupid+selfuid);
            var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", groupid + selfuid);
            if (all != null && all.Any())
            {
                /*从缓存中删除这些消息*/
                RedisHelper.Hash_Remove("IMGroupMsg", groupid+selfuid);
                return PartialView("_RecentlyChat", all);
            }
            var listResult = new List<Msg>();
            var query =
                     unitOfWork.DIMMsg.Get(
                         p => (p.TouID == groupid && p.IsDel!=1)).OrderByDescending(p => p.CreateDate).Take(5).OrderBy(p => p.CreateDate);
            foreach (var msg in query)
            {
                var singleMsg = MsgServices.ImMsg2Msg(msg);
                listResult.Add(singleMsg);
            }

            return PartialView("_RecentlyChat", listResult);
        }

        /// <summary>
        /// 历史聊天记录(根据传入参数的不同 分为两种查询方式 一种是按照时间来查询 一种是按照聊天内容来查询)
        /// </summary>
        /// <returns></returns>
        public ActionResult HistoryChat(string touid, DateTime dateTime, int pageNumber = 1,string msg="")
        {
            List<HistoryChatEntity> list = new List<HistoryChatEntity>();
            if (msg == "")
            {
                //根据日期查询
                /*聊天记录 根据当前日期 选取该日期的聊天就 然后执行翻页*/
                dateTime = dateTime.Date;
                var fromuid = LoginUserService.ssoUserID;
                var dateTime2 = dateTime.AddDays(1);
                var query =
                    unitOfWork.DIMMsg.Get(
                        p =>
                            p.TouID == touid &&
                            (p.CreateDate.Value >= dateTime && p.CreateDate.Value <= dateTime2) && p.IsDel!=1)
                        .OrderByDescending(p => p.CreateDate);

                Paging paging = new Paging();

                paging.PageNumber = pageNumber;
                paging.PageSiz = 20;
                paging.Amount = query.Count();

                ViewBag.pageNo = pageNumber; //当前页码 1
                ViewBag.pageCount = paging.PageCount;

                var pageQuery2 =
                    query.Skip(paging.PageSiz*paging.PageNumber).Take(paging.PageSiz).OrderBy(p => p.CreateDate).ToList();

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
                    return Json(new {r = true, list = list, pageCount = paging.PageCount, pageNo = pageNumber},
                        JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false}, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //根据内容查询
                var query =
                    unitOfWork.DIMMsg.Get(
                        p => p.TouID == touid&& p.Msg.Contains(msg) && p.IsDel != 1).OrderByDescending(p => p.CreateDate);

                Paging paging = new Paging();

                paging.PageNumber = pageNumber;
                paging.PageSiz = 20;
                paging.Amount = query.Count();

                ViewBag.pageNo = pageNumber;//当前页码 1
                ViewBag.pageCount = paging.PageCount;

                var pageQuery2 = query.Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).OrderBy(p => p.CreateDate).ToList();
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
                    return Json(new { r = true, list = list, pageCount = paging.PageCount, pageNo = pageNumber }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 发送聊天消息
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        public ActionResult SendMsg(string touid, string msg,string fromusername,string tousername,int isgroup=1)
        {
            var fromuid = LoginUserService.ssoUserID;
            var item = new IMMsg
            {
                FromuID = fromuid,
                CreateDate = DateTime.Now,
                Msg = msg,
                TouID = touid,
                CreateUser = fromuid,
                Result = 0,
                fromusername = fromusername,
                tousername = tousername,
                isgroup= isgroup
            };
            var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            unitOfWork.DIMMsg.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                //此群组包含的用户 即群聊要通知的用户
                List<string> touids =
                    unitOfWork.DImGroupDetail.Get(p => p.GroupID == touid&&p.UserID!=fromuid).Select(p => p.UserID).ToList();



                return Json(new { r = true,  msgtime = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msgcontent = item.Msg, photo = photo ,msgid=item.ID,touids= touids }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

       
        

        public ActionResult SendImageMsg(string touid,string msg, string fromusername, string tousername)
        {
            var fromuid = LoginUserService.ssoUserID;
            var item = new IMMsg
            {
                FromuID = fromuid,
                CreateDate = DateTime.Now,
                Msg = "<img src=\""+msg+"\">",
                TouID = touid,
                CreateUser = fromuid,
                Result = 0,
                fromusername = fromusername,
                tousername = tousername
            };
            var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            unitOfWork.DIMMsg.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                // 此群组包含的用户 即群聊要通知的用户
                List<string> touids =
                    unitOfWork.DImGroupDetail.Get(p => p.GroupID == touid && p.UserID != fromuid).Select(p => p.UserID).ToList();
                return Json(new { r = true, msgtime = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msgcontent = item.Msg, photo = photo, msgid = item.ID, touids = touids }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 这个方法已经弃用了，最新的存储文件聊天消息到数据库的方法在chatHub中 2018年12月20日
        /// </summary>
        /// <param name="touid"></param>
        /// <param name="fileurl"></param>
        /// <param name="filename"></param>
        /// <param name="fromusername"></param>
        /// <param name="tousername"></param>
        /// <returns></returns>
        public ActionResult SendFileMsg(string touid, string fileurl,string filename, string fromusername, string tousername)
        {
            var fromuid = LoginUserService.ssoUserID;
            var item = new IMMsg
            {
                FromuID = fromuid,
                CreateDate = DateTime.Now,
                Msg = "<a class=\"layui-layim-file\" href=\""+fileurl+"\" target=\"_blank\"><i class=\"layui-icon\"></i><cite>"+ filename + "</cite></a>",
                TouID = touid,
                CreateUser = fromuid,
                Result = 0,
                fromusername = fromusername,
                tousername = tousername
            };
            var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            unitOfWork.DIMMsg.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                // 此群组包含的用户 即群聊要通知的用户
                List<string> touids =
                    unitOfWork.DImGroupDetail.Get(p => p.GroupID == touid && p.UserID != fromuid).Select(p => p.UserID).ToList();
                return Json(new { r = true, msgtime = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"), msgcontent = item.Msg, photo = photo, msgid = item.ID, touids = touids }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ShowSelfGroupMembers(string groupid)
        {
            ViewBag.groupid = groupid;
            var gid = Convert.ToInt32(groupid);
            var group = unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
            var selfuid = LoginUserService.ssoUserID;
            ViewBag.isadmin = 0;
            ViewBag.groupDesc = "";
            if (group != null)
            {
                ViewBag.groupDesc = group.Des;
                if (group.Creator == selfuid)
                {
                    ViewBag.isadmin = 1;
                }
            }
            var query = unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);

            ViewBag.announcementTitle = "";
            ViewBag.announcement = "";//第一条群组公告
            var queryAnnouncement = unitOfWork.DGroupAnnouncement.Get(p => p.groupid == gid).FirstOrDefault();
            if (queryAnnouncement != null)
            {
                ViewBag.announcementTitle = queryAnnouncement.title;
                ViewBag.announcement = queryAnnouncement.content;
            }

            return PartialView("_GroupMembersList", query);
        }

    }
}