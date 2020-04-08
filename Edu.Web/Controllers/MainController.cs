using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ChineseConvertPinyin;
using Edu.Entity;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.Service;
using Edu.Service.Service;
using Edu.Service.Service.Message;
using Edu.Tools;
using Edu.Web.Models;
using Edu.Web.Service;
using KNet.AAMS.Foundation.Model;
using Newtonsoft.Json;

namespace Edu.Web.Controllers
{
    public class MainController : BaseControl
    {
        
        /// <summary>
        /// id=0 表示oaokcs.cnki.net 默认值
        /// id=1 表示oa.cnki.net
        /// 这个id用于前端页面连接signalr的时候使用
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ActionResult> Index(int id=0)
        {
            var currentuser = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID);
            var orgid = currentuser.OrgId;
            //异步获取当前机构内所有用户，设置拼音第一个字母，放到缓存中
            Task.Run(() =>
            {
                var memberList = new List<object>();
                memberList = ssoUserOfWork.GetMembersOfUnit(orgid);
                
                List<SUser> list = new List<SUser>();
                JavaScriptSerializer jsonser = new JavaScriptSerializer();
                string myJson = jsonser.Serialize(memberList);
                //到上面步骤已经完成了object转化json，接下来由json转化自定model
                list = JsonConvert.DeserializeObject<List<SUser>>(myJson);
                var listtemp = list.Where(p => p.type == 0 &&p.Enabled==true ).ToList();
                foreach (var sUser in listtemp)
                {
                    sUser.PinYinFull = pinyinConvert.GetFullPinyin(sUser.RealName).ToUpper();
                    sUser.PinYinSimple = pinyinConvert.GetInitialPinyin(sUser.RealName).ToUpper();
                }
                RedisHelper.Hash_Set<List<SUser>>("OrgMembers", orgid, listtemp);
            });

            ViewBag.terminal = "oaokcs";
            if (id == 1)
            {
                ViewBag.terminal = "oa";
            }

            return View();
        }

        /// <summary>
        /// 初始化用户聊天列表
        /// </summary>
        /// <returns></returns>
        public ActionResult InitUserList()
        {
            return PartialView("_InitUserList");
        }
        /// <summary>
        /// 初始化用户聊天群
        /// </summary>
        /// <returns></returns>
        public ActionResult InitGroupList()
        {
            return PartialView("_InitGroupList");
        }

        /// <summary>
        /// 最近联系人 
        /// </summary>
        /// <returns></returns>
        public ActionResult RecentlyChatUser()
        {
            /*
             * 2018年6月14日 先从缓存中取该用户所有的未读消息 然后按照fromuid分组 查看是否够5个 若不够 则再从RecentContacts表中取数据 凑够五个 若大于等于5个 则全部显示到前台界面
             */
            /*
             * 2019年5月13日 取消5个的限制
             */

            var selfuid = LoginUserService.ssoUserID;
            var allUnreadMessages = MsgServices.GetRedisKeyValue<Msg>("IMMsg", selfuid);
            var queryRecentContactUsers =
                    unitOfWork.DRecentContacts.Get(p => (p.uID == selfuid || p.ContactID == selfuid))
                        .OrderByDescending(p => p.ContactDate);
            var resultList=new List<UserViewWeb>();
            if (queryRecentContactUsers.Any())
            {
                foreach (var user in queryRecentContactUsers)
                {
                    var uid = (user.uID == selfuid ? user.ContactID : user.uID);
                    var ssouser = ssoUserOfWork.GetUserByID(uid);
                    if (ssouser == null) { continue; }

                    var u = new UserViewWeb();
                    u.Logo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + uid;
                    u.UserId = uid;
                    u.LatestMsgtime = user.ContactDate.Value;
                    u.UnreadMsgCount = 0;
                    u.Connid = "";
                    u.Department = "";
                    if (allUnreadMessages != null && allUnreadMessages.Any())
                    {
                        var k = allUnreadMessages.Where(p => p.fromuid == uid);
                        if (k.Any())
                        {
                            u.UnreadMsgCount = k.Count();
                        }
                    }
                    u.RealName = ssouser.RealName;
                    u.Department = ssouser.DeptName;
                    var isonline = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
                    if (isonline != null)
                    {
                        u.Connid=(isonline.ConnectionId);
                    }
                    else
                    {
                        var isonlineapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", uid);
                        if (isonlineapp != null)
                        {
                            u.Connid =(isonlineapp.ConnectionId);
                        }
                        else
                        {
                            var isonline_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
                            if (isonline_oa != null)
                            {
                                u.Connid = (isonline_oa.ConnectionId);
                            }
                        }
                    }
                    resultList.Add(u);
                }
            }
            return PartialView("_RecentlyChatUserList", resultList);
        }

        /// <summary>
        /// 常用联系人
        /// </summary>
        /// <returns></returns>
        public ActionResult TopChatUser()
        {
            var selfuid = LoginUserService.ssoUserID;
            var query =
                unitOfWork.DTopContacts.Get(p => p.uID == selfuid || p.ContactID == selfuid)
                    .OrderByDescending(p => p.ContactDate);
            var resultList = new List<UserViewWeb>();
            if (query != null && query.Any())
            {
                foreach (var user in query)
                {
                    var uid = (user.uID == selfuid ? user.ContactID : user.uID);
                    //去重
                    if (resultList.Where(p => p.UserId == uid).ToList().Count == 0)
                    {
                        var ssouser = ssoUserOfWork.GetUserByID(uid);
                        if (ssouser == null) { continue; }

                        var u = new UserViewWeb();
                        u.Logo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + uid;
                        u.UserId = uid;
                        u.LatestMsgtime = user.ContactDate.Value;
                        u.UnreadMsgCount = 0;
                        u.Connid = "";
                        u.Department = "";
                        u.RealName = ssouser.RealName;
                        u.Department = ssouser.DeptName;

                        var isonline = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
                        if (isonline != null)
                        {
                            u.Connid = (isonline.ConnectionId);
                        }
                        else
                        {
                            var isonlineapp = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", uid);
                            if (isonlineapp != null)
                            {
                                u.Connid = (isonlineapp.ConnectionId);
                            }
                            else
                            {
                                var isonline_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
                                if (isonline_oa != null)
                                {
                                    u.Connid = (isonline_oa.ConnectionId);
                                }
                            }
                        }
                        resultList.Add(u);
                    }

                    
                }
                return PartialView("_TopChatUserList", resultList);
            }
            return PartialView("_TopChatUserList", null);

        }

        /// <summary>
        /// 根据关键字查询用户 最新的，可以根据拼音首字母查询 2018年12月28日新增
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult SearchUserNewPinYin(string keyword)
        {
            var list = new List<SUser>();
            var uid=LoginUserService.ssoUserID;
            var currentuser = ssoUserOfWork.GetUserByID(uid);
            var orgid = currentuser.OrgId;
            list = RedisHelper.Hash_Get<List<SUser>>("OrgMembers", orgid);
            if (list != null&& list.Any())
            {
                //排除掉当前自己的用户的常用联系人
                //查找到自己的常用联系人 获取其中的uid 组成一个List
                var queryTopContact = unitOfWork.DTopContacts.Get(p => p.uID == LoginUserService.ssoUserID);
                List<string> uidList = queryTopContact.Select(user => user.ContactID).ToList();
                ViewBag.contactIdList = uidList;
                var listtemp = list.Where(p => p.type == 0).ToList();
                if ((int) keyword[0] > 0x4E00 && (int) keyword[0] < 0x9FA5)
                {
                    //中文汉字开头
                    //var queryresult = listtemp.Where(p => (p.RealName.StartsWith(keyword)) && p.id != uid);
                    //中文汉字包含的
                    var queryresultCotain = listtemp.Where(p => (p.RealName.Contains(keyword)) && p.id != uid);

                    return PartialView("_SearchUser", queryresultCotain);
                    if (queryresultCotain.Any())
                    {
                        //queryresult.AddList(queryresultCotain);
                        //return PartialView("_SearchUser", queryresult);
                    }
                    //return PartialView("_SearchUser", queryresult);
                }
                var keywords = pinyinConvert.GetFullPinyin(keyword).ToUpper();
                var queryresult2 =
                    listtemp.Where(
                        p => (p.PinYinFull == keywords || p.PinYinFull.StartsWith(keywords)||p.PinYinFull.Contains(keywords)|| (p.PinYinSimple == keywords || p.PinYinSimple.StartsWith(keywords) || p.PinYinSimple.Contains(keywords))) && p.id != uid);
                if (!queryresult2.Any())
                {
                    queryresult2 =
                       listtemp.Where(p => ( p.PinYinFull.StartsWith(keywords)) && p.id != uid);
                    if (!queryresult2.Any())
                    {
                        queryresult2 =
                           listtemp.Where(p => (p.PinYinFull.Contains(keywords)) && p.id != uid);
                        if (!queryresult2.Any())
                        {
                            queryresult2 =
                                listtemp.Where(p => (p.PinYinSimple == keywords || p.PinYinSimple.StartsWith(keywords) || p.PinYinSimple.Contains(keywords)) && p.id != uid);
                            if (!queryresult2.Any())
                            {
                                queryresult2 =
                                    listtemp.Where(p => (p.PinYinSimple.StartsWith(keywords)) && p.id != uid);
                                if (!queryresult2.Any())
                                {
                                    queryresult2 =
                                        listtemp.Where(p => (p.PinYinSimple.Contains(keywords)) && p.id != uid);
                                }
                            }
                        }
                    }
                }

                return PartialView("_SearchUser", queryresult2);
            }
            return PartialView("_SearchUser", null);
        }


        /// <summary>
        /// 添加到常用联系人
        /// </summary>
        /// <param name="touid"></param>
        /// <returns></returns>
        public ActionResult AddToContact(string touid)
        {
            var selfuid = LoginUserService.ssoUserID;
            var item = new TopContacts();
            item.uID = selfuid;
            item.ContactID = touid;
            item.ContactDate = DateTime.Now;

            if (selfuid == null || touid == null || selfuid == "" || touid == "")
            {
                return Json(new { r = false, msg = "操作失败！联系人uid为空！\n" }, JsonRequestBehavior.AllowGet);
            }


            unitOfWork.DTopContacts.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, msg = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult AddToContactString(string touid)
        {
            var queryUser = unitOfWork.DUserInfo.Get(p => p.uID == touid).FirstOrDefault();
            var photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + touid;
            var trueName = queryUser.TrueName;
            //todo:个性签名
            var sign = queryUser.UserName;
            var ssouser = ssoUserOfWork.GetUserByID(touid);
            var departmentName = "";
            if (ssouser != null)
            {
                departmentName = ssouser.DeptName;
            }
            return Json(new
            {
                r = true,
                uid = touid,
                connid = "",
                photo = photo,
                trueName = trueName,
                sign = sign,
                departmentName= departmentName
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddToGroupContactString(string gid)
        {
            var gidint = Convert.ToInt32(gid);
            var queryUser = unitOfWork.DImGroup.Get(p => p.ID == gidint).FirstOrDefault();
            var trueName = queryUser.Name;

            return Json(new
            {
                r = true,
                photo = ConfigHelper.GetConfigString("IMWebApiGroupPic") + gid,
                groupName = trueName
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckUserOnline(string uid)
        {
            /*1.oaokcs端在线*/
            var isonline = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
            /*2.oa端在线*/
            var isonline_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
            /*3.app端在线*/
            var isonline_app = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", uid);
            
            if(isonline==null && isonline_oa==null && isonline_app == null)
            {
                return Json(new { result = false }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取当前用户所在的群组 自建群  
        /// 2018年6月27日 新增功能 显示出未读的聊天消息的条数
        /// </summary>
        /// <returns></returns>
        public ActionResult SelfChatGroup()
        {
            var selfuid = LoginUserService.ssoUserID;
            var query = unitOfWork.DImGroupDetail.Get(p => p.UserID == LoginUserService.ssoUserID);
            List<GroupViewNew> list = new List<GroupViewNew>();
            foreach (var item in query)
            {
                var gid = Convert.ToInt32(item.GroupID);
                var g=new GroupViewNew();

                g.Logo= ConfigHelper.GetConfigString("IMWebApiGroupPic") + item.GroupID;
                g.ID = item.GroupID;
                g.CreateUserID = item.Creator;
                g.UnreadMsgCount = 0;
                g.LatestMsgtime= "1990/01/01 01:01:01";
                g.Name = unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault().Name;
                var unreadmsg = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item.GroupID + selfuid);
                if (unreadmsg != null && unreadmsg.Any())
                {
                    var msg = unreadmsg[unreadmsg.Count - 1];
                    g.LatestMsgtime = msg.msgtime;
                    g.UnreadMsgCount = unreadmsg.Count;
                }

                list.Add(g);
            }
            list = list.OrderByDescending(p => Convert.ToDateTime(p.LatestMsgtime)).ToList();
            /*修改未读消息群组的缓存  暂时去掉 在分布视图中 使用这个异步方法会报错*/
            //await Task.Run(() =>
            //{
            //    var result1 = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", selfuid);
            //    if (result1 != null && result1.Any())
            //    {
            //        foreach (var groupid in result1)
            //        {
            //            if (groupid.Length != 36)
            //            {
            //                var gourpidInt = Convert.ToInt32(groupid);
            //                var findgroup = list.Where(p => p.ID == gourpidInt);
            //                if (!findgroup.Any())
            //                {
            //                    result1.Remove(groupid);
            //                }
            //            }
            //        }
            //    //重新存储未读消息群组的Redis变量
            //    RedisHelper.Hash_Remove("IMUnreadGroupMsg", selfuid);
            //    RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", selfuid, result1);
            //    }
            //});
            return PartialView("_SelfChatGroupList", list);
        }

        public JsonResult GetMyChatGroup()
        {
            var uid = LoginUserService.ssoUserID;
            var query = unitOfWork.DImGroupDetail.Get(p => p.UserID == uid).OrderByDescending(p => p.CreateDate);
            List<ImGroup> list = new List<ImGroup>();
            var listCount = new List<int>();
            if (query != null)
            {
                list.AddRange(
                    query.Select(item => Convert.ToInt32(item.GroupID))
                        .Select(id => unitOfWork.DImGroup.Get(p => p.ID == id).FirstOrDefault()));
            }
            return Json(new { code = 0, data = list }, JsonRequestBehavior.AllowGet);

        }

        /// <summary>
        /// 获取当前用户所在的群组 工作群 不用了
        /// </summary>
        /// <returns></returns>
        public ActionResult WorkChatGroupView()
        {
            return PartialView("_WorkChatGroupList", null);
        }

        public ActionResult WorkChatGroup(List<GroupViewNew> list)
        {
            //这个list中包含了该用户所在的工作群组
            var selfuid = LoginUserService.ssoUserID;
            foreach (var item in list)
            {
                var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item.ID  + selfuid);
                //如果all不为空，那么其中最后一条数据的时间是最新发生聊天记录的时间
                if (all != null && all.Any())
                {
                    var msg = all[all.Count - 1];
                    item.LatestMsgtime = msg.msgtime;
                    item.UnreadMsgCount = all.Count;
                }
                else
                {
                    item.LatestMsgtime = "1990/01/01 01:01:01";
                    item.UnreadMsgCount = 0;
                }
            }
            list = list.OrderByDescending(p => Convert.ToDateTime(p.LatestMsgtime)).ToList();

            return PartialView("_WorkChatGroupList", list);
        }
        /// <summary>
        /// 根据关键字查找群组
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public ActionResult SearchGroup(string keyword)
        {
            var selfuid = LoginUserService.ssoUserID;
            var list = new List<GroupViewNew>();
            var queryresult = new List<GroupViewNew>();
            //var query = unitOfWork.DImGroupDetail.Get(p => p.UserID == selfuid);
            //foreach (var group in query)
            //{
            //    var groupid = Convert.ToInt32(group.GroupID);
            //    var q = unitOfWork.DImGroup.Get(p => p.ID == groupid).FirstOrDefault();
            //    if (q != null)
            //    {
            //        var imGroup = new GroupViewNew() { ID = q.ID.ToString(), Name = q.Name, Logo = ConfigHelper.GetConfigString("IMWebApiGroupPic") + q.ID,PinYinFull = pinyinConvert.GetFullPinyin(q.Name).ToUpper(),PinYinSimple = pinyinConvert.GetInitialPinyin(q.Name).ToUpper()};
            //        list.Add(imGroup);
            //    }
            //}
            /*工作群--因为初次加载的时候已经放到缓存中了--所以这里从缓存中查找*/
            var queryWorkGroujp = RedisHelper.Hash_Get<List<GroupViewNew>>("IMGroup", selfuid);
            if (queryWorkGroujp != null)
            {
                queryWorkGroujp = queryWorkGroujp.ToList();
                list.AddRange(queryWorkGroujp);
            }
            if (!list.Any())
            {
                return PartialView("_SearchGroup", null);
            }
            //根据关键字查询
            if ((int)keyword[0] > 0x4E00 && (int)keyword[0] < 0x9FA5)
            {
                //中文汉字开头
                queryresult =
               list.Where(p => (p.Name.StartsWith(keyword))).ToList();
                if (!queryresult.Any())
                {
                    queryresult =
                   list.Where(p => (p.Name.Contains(keyword))).ToList();
                }
                return PartialView("_SearchGroup", queryresult);

            }
            var keywords = pinyinConvert.GetFullPinyin(keyword).ToUpper();
            var queryresult2 =
                list.Where(
                    p => (p.PinYinFull == keywords || p.PinYinFull.StartsWith(keywords) || p.PinYinFull.Contains(keywords)));
            if (!queryresult2.Any())
            {
                queryresult2 =
                   list.Where(
                       p => (p.PinYinFull.StartsWith(keywords)));
                if (!queryresult2.Any())
                {
                    queryresult2 =
                       list.Where(
                           p => (p.PinYinFull.Contains(keywords)));
                    if (!queryresult2.Any())
                    {
                        queryresult2 =
                           list.Where(
                               p => (p.PinYinSimple == keywords || p.PinYinSimple.StartsWith(keywords) || p.PinYinSimple.Contains(keywords)));
                        if (!queryresult2.Any())
                        {
                            queryresult2 =
                               list.Where(
                                   p => (p.PinYinSimple.StartsWith(keywords)));
                            if (!queryresult2.Any())
                            {
                                queryresult2 =
                                   list.Where(
                                       p => (p.PinYinSimple.Contains(keywords)));
                            }
                        }
                    }
                }
            }
            return PartialView("_SearchGroup", queryresult2.ToList());
        }


        /// <summary>
        /// 设置当前用户所在的工作群，存储到redis中
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public ActionResult SetWorkGroup(string obj)
        {
            var selfuid = LoginUserService.ssoUserID;
            var list =new List<GroupViewNew>();
            //到上面步骤已经完成了object转化json，接下来由json转化自定model
            list = JsonConvert.DeserializeObject<List<GroupViewNew>>(obj);
            foreach (var item in list)
            {
                //首字母大写
                item.PinYinSimple = pinyinConvert.GetInitialPinyin(item.Name).ToUpper();
                //全拼
                item.PinYinFull = pinyinConvert.GetFullPinyin(item.Name).ToUpper();
            }
            //更新Redis中用户所在的工作群组
            if (list.Any())
            {
                RedisHelper.Hash_Set<List<GroupViewNew>>("IMGroup", selfuid, list);

                //查询Redis中用户有未读消息的群组
                var result1 = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", selfuid);
                if (result1 != null && result1.Any())
                {
                    var result2 = result1.ToList();
                    foreach (var groupid in result1)
                    {
                        if (groupid.Length == 36)
                        {
                            var findgroup = list.Where(p => p.ID == groupid);
                            //如果没找到，则说明该用户已经被移除该群，那么就在Redis中把该未读消息的群移除掉
                            if (!findgroup.Any())
                            {
                                result2.Remove(groupid);
                            }
                        }
                    }
                    //重新存储未读消息群组的Redis变量
                    RedisHelper.Hash_Remove("IMUnreadGroupMsg", selfuid);
                    RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", selfuid, result2);
                }
            }
            return Json(new { Success = true, Content = obj, Error = list, Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 创建群组
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateChatGroup()
        {
            var user = unitOfWork.DUserInfo.Get(p => p.uID == LoginUserService.ssoUserID).FirstOrDefault();
            user.Photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            user.TrueName = LoginUserService.realName;
            
            return View("CreateChatGroup", user);
        }
        /// <summary>
        /// 修改群组前判断条件 是否可以就进行修改
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        public ActionResult EditChatGroupPre(int gid)
        {
            var query = unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
            if (query == null)
            {
                return Json(new { result = false, msg = "未查询到改群组" }, JsonRequestBehavior.AllowGet);
            }
            if (query.Creator != LoginUserService.ssoUserID)
            {
                return Json(new { result = false, msg = "您不是该群组的创建者，不能修改群组属性" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { result = true, url = "/im/Main/EditChatGroupView?gid=" + gid }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 修改群组 只有群组的创建者 才可以修改群组
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        public ActionResult EditChatGroupView(int gid)
        {
            var query = unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
            var gidstr = Convert.ToString(gid);
            var selfuid = LoginUserService.ssoUserID;
            var queryGroupDetail =
                unitOfWork.DImGroupDetail.Get(p => p.GroupID == gidstr && p.UserID != selfuid);
            ViewBag.glist = "";
            ViewBag.queryGroupDetail = queryGroupDetail;
            foreach (var item in queryGroupDetail)
            {
                ViewBag.glist += item.UserID + ";";
            }
            ViewBag.UserName = unitOfWork.DUserInfo.Get(p => p.uID == LoginUserService.ssoUserID).FirstOrDefault().TrueName;
            query.Des = query.Des.Replace("\n", "");
            return View("EditChatGroup", query);
        }

        public ActionResult EditChatGroup(int gid, string gname, string gdes, string glist,string grouplogo)
        {
            var query = unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
            if (query == null)
            {
                return Json(new { result = false, msg = "该群组已被删除" }, JsonRequestBehavior.AllowGet);
            }
            query.Name = gname;
            query.Des = gdes;
            query.CreateDate = DateTime.Now;
            query.Photo = grouplogo;
            unitOfWork.DImGroup.Update(query);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                var gidstr = gid.ToString();
                var queryMembers = unitOfWork.DImGroupDetail.Get(p => p.GroupID == gidstr && p.UserID != LoginUserService.ssoUserID).Select(p=>p.UserID).ToList();
                var mList = glist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                //通过比较这两个list 来确定要新增的成员和要删除的成员
                var addList=new List<string>();
                var deleteList=new List<string>();
                addList = mList.Except(queryMembers).ToList();
                deleteList = queryMembers.Except(mList).ToList();



                foreach (var memberuid in addList)
                {
                    var ssoUser = ssoUserOfWork.GetUserByID(memberuid);
                    if (ssoUser == null) { continue; }
                    var member = new ImGroupDetail
                    {
                        GroupID = gidstr,
                        UserID = memberuid,
                        NickName = ssoUser.RealName,
                        isAdmin = 0,
                        Creator = LoginUserService.ssoUserID,
                        CreateDate = DateTime.Now,
                        photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + memberuid

                    };
                    unitOfWork.DImGroupDetail.Insert(member);
                }

                foreach (var memberuid in deleteList)
                {
                    unitOfWork.DImGroupDetail.Delete(p => p.UserID == memberuid && p.GroupID == gidstr);

                    //删除未读的群组消息缓存
                    var result1 = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", memberuid);
                    if (result1 != null && result1.Any())
                    {
                        if (result1.Contains(gidstr))
                        {
                            result1.Remove(gidstr);
                            //重新存储未读消息群组的Redis变量
                            RedisHelper.Hash_Remove("IMUnreadGroupMsg", memberuid);
                            RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", memberuid, result1);
                        }
                    }
                }



                result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    ///*新增成员------start------*/
                    //var queryMemberList = unitOfWork.DImGroupDetail.Get(p => p.GroupID == gidstr);
                    //foreach (var memberuid in addList)
                    //{
                    //    var ssoUser = ssoUserOfWork.GetUserByID(memberuid);
                    //    if (ssoUser == null) { continue; }
                    //    //向MQ中发通知，向移动的推送一条消息 新增成员 的消息
                    //    var msg = new SelfGroupNoticeMore
                    //    {
                    //        groupid = gidstr,
                    //        groupname = query.Name,
                    //        membersList = queryMemberList.Select(p => p.UserID).ToList(),
                    //        membername = ssoUser.RealName,
                    //        type = 0
                    //    };
                    //    var queueAddress =
                    //        MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                    //    queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                    //}
                    /*新增成员------end------*/
                    /*移除成员------start------*/
                    //foreach (var memberuid in deleteList)
                    //{
                    //    var ssoUser = ssoUserOfWork.GetUserByID(memberuid);
                    //    if (ssoUser == null) { continue; }
                    //    //向MQ中发通知，向移动的推送一条消息 移除成员 的消息
                    //    //注意需要向自己推送
                    //    var receiveUsers = queryMemberList.Select(p => p.UserID).ToList();
                    //    receiveUsers.Add(memberuid);
                    //    var msg = new SelfGroupNoticeMore
                    //    {
                    //        groupid = gidstr,
                    //        groupname = query.Name,
                    //        membersList = receiveUsers,
                    //        membername = ssoUser.RealName,
                    //        type = 1
                    //    };
                    //    var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                    //    queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                    //}
                    /*移除成员------end------*/

                    

                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                else if (result.ResultType == OperationResultType.NoChanged)
                {
                    //没有成员变化，只有群组信息变化
                    if (addList.Count == 0 && deleteList.Count == 0)
                    {
                        var toList = mList.ToList(); toList.Add(query.Creator);
                        //向MQ中发通知，向移动的推送一条消息 移除成员 的消息
                        var msg = new SelfGroupNoticeMore
                        {
                            groupid = gidstr,
                            groupname = query.Name,
                            membersList = toList,
                            membername = "",
                            type = 4
                        };
                        var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                        queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                    }
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }else
                {
                    return Json(new { result = false, msg =  result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { result = false, msg =  result.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 退出群组 非群组管理员退出群组
        /// </summary>
        /// <param name="gid"></param>
        /// <returns></returns>
        public ActionResult ExitChatGroup(int gid)
        {
            var uid = LoginUserService.ssoUserID;
            var gidstr = Convert.ToString(gid);
            var query =
                unitOfWork.DImGroupDetail.Get(p => p.GroupID == gidstr && p.UserID == LoginUserService.ssoUserID)
                    .FirstOrDefault();
            if (query == null)
            {
                return Json(new { result = false, msg = "未查询到该群组" }, JsonRequestBehavior.AllowGet);
            }
            //var groupid = gid.ToString();
            //var queryMemberList = unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
            //var queryGroup = unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
            //var ssoUser = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID);
            //var msg = new SelfGroupNoticeMore
            //{
            //    groupid = groupid,
            //    groupname = queryGroup.Name,
            //    membersList = queryMemberList.Select(p => p.UserID).ToList(),
            //    membername = ssoUser.RealName,
            //    type = 2,
            //};
            unitOfWork.DImGroupDetail.Delete(query);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                //发送消息队列
                //var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                //queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);

                //删除未读的群组消息缓存
                var result1 = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid);
                if (result1 != null && result1.Any())
                {
                    var groupidString = Convert.ToString(gid);
                    if (result1.Contains(groupidString))
                    {
                        result1.Remove(groupidString);
                        //重新存储未读消息群组的Redis变量
                        RedisHelper.Hash_Remove("IMUnreadGroupMsg", uid);
                        RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", uid, result1);
                    }
                }

                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, msg = result.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        
        /// <summary>
        /// 解散工作群组
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult GiveupChatGroup(string gid)
        {
            var groupid = Convert.ToInt32(gid);
            var queryMemberList = unitOfWork.DImGroupDetail.Get(p => p.GroupID == gid);
            var queryGroup = unitOfWork.DImGroup.Get(p => p.ID == groupid).FirstOrDefault();
            //var ssoUser = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID);
            //var msg = new SelfGroupNoticeMore
            //{
            //    groupid = gid,
            //    groupname = queryGroup.Name,
            //    membersList = queryMemberList.Select(p => p.UserID).ToList(),
            //    membername = ssoUser.RealName,
            //    type=3,
            //};

            unitOfWork.DImGroupDetail.Delete(p => p.GroupID == gid);
            unitOfWork.DImGroup.Delete(p => p.ID == groupid);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                //发送消息队列
                //var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                //queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);

                foreach(var member in queryMemberList)
                {
                    //删除未读的群组消息缓存
                    var result1 = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", member.UserID);
                    if (result1 != null && result1.Any())
                    {
                        if (result1.Contains(gid))
                        {
                            result1.Remove(gid);
                            //重新存储未读消息群组的Redis变量
                            RedisHelper.Hash_Remove("IMUnreadGroupMsg", member.UserID);
                            RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", member.UserID, result1);
                        }
                    }
                }

                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, msg = result.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 创建工作群组
        /// </summary>
        /// <param name="groupname"></param>
        /// <param name="groupdes"></param>
        /// <param name="uid"></param>
        /// <param name="glist"></param>
        /// <param name="grouplogo"></param>
        /// <param name="gtype"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CreateChatGroup(string groupname, string groupdes, string uid, string glist, string grouplogo, int gtype = 0)
        {
            if (string.IsNullOrEmpty(groupname))
            {
                return Json(new { result = false, msg = "群组名称不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { result = false, msg = "创建者uid不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(glist))
            {
                return Json(new { result = false, msg = "群组成员不能为空" }, JsonRequestBehavior.AllowGet);
            }
            if (groupname.Length > 50)
            {
                return Json(new { result = false, msg = "群组名称不能超过50个字符" }, JsonRequestBehavior.AllowGet);
            }
            if (groupdes.Length > 500)
            {
                return Json(new { result = false, msg = "群组说明不能超过500个字符" }, JsonRequestBehavior.AllowGet);
            }

            var group = new ImGroup();
            group.Name = groupname;
            group.Photo = "";//群组照片,选取前几个成员的头像合成的，如果不传值，默认为空，获取的时候通过接口去获取
            if (!string.IsNullOrEmpty(grouplogo))
            {
                group.Photo = grouplogo;
            }
            group.Des = groupdes;
            group.Creator = LoginUserService.ssoUserID;
            group.CreateDate = DateTime.Now;
            group.TypeID = gtype;
            unitOfWork.DImGroup.Insert(group);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                /*先创建管理员*/
                var gid = group.ID;
                var gidstr = Convert.ToString(gid);
                var owner = new ImGroupDetail
                {
                    GroupID = gidstr,
                    UserID = LoginUserService.ssoUserID,
                    NickName = LoginUserService.User.TrueName,
                    isAdmin = 1,
                    Creator = LoginUserService.ssoUserID,
                    CreateDate = DateTime.Now,
                    photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID
                };
                unitOfWork.DImGroupDetail.Insert(owner);
                /*再添加成员*/
                var mList = glist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var memberuid in mList)
                {
                    var ssoUser = ssoUserOfWork.GetUserByID(memberuid);
                    if (ssoUser == null) { continue; }
                    var member = new ImGroupDetail
                    {
                        GroupID = gidstr,
                        UserID = memberuid,
                        NickName = ssoUser.RealName,
                        isAdmin = 0,
                        Creator = LoginUserService.ssoUserID,
                        CreateDate = DateTime.Now,
                        photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + memberuid
                    };
                    unitOfWork.DImGroupDetail.Insert(member);
                }
                result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    //向MQ中发通知，向移动的推送一条消息 创建自建群 的消息
                    //现在修改为直接向SirnalR服务端发送一条消息
                    var msg = new SelfGroupNotice
                    {
                        groupid = gidstr,
                        groupname = groupname,
                        creator = uid,
                        photo = System.Web.HttpUtility.UrlEncode(System.Configuration.ConfigurationManager.AppSettings.Get("IMWebApiGroupPic") + group.ID, System.Text.Encoding.UTF8),
                        createtime = group.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                        membersList = mList.ToList(),
                        describe = groupdes
                    };

                    var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                    queueAddress.Send<Edu.Web.Models.SelfGroupNotice>(msg);

                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { result = false, msg =  result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { result = false, msg =  result.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetMemberInfo(string uids)
        {
            var memberList = new List<DeptUser>();
            var uidList = uids.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var uid in uidList)
            {
                var member = ssoUserOfWork.GetUserByID(uid);
                if (member == null) { continue; }
                memberList.Add(member);
            }
            return PartialView("_MemberList", memberList);
        }
        public ActionResult ChooseGroupMember(int groupid = 0)
        {
            ViewBag.groupid = groupid;
            return View();
        }

        public ActionResult GetAllMembers()
        {
            var currentuser = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID);
            var memberList = ssoUserOfWork.GetMembersOfUnit(currentuser.OrgId);
            return Json(new { members = memberList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGroupMembers(int groupid = 0)
        {
            var list = new List<DeptUser>();

            if (groupid != 0)
            {
                var gidstr = groupid.ToString();
                var query = unitOfWork.DImGroupDetail.Get(p => p.GroupID == gidstr && p.UserID != LoginUserService.ssoUserID);
                if (query == null)
                {
                    return Json(new { count = 0, members = list }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    foreach (var item in query)
                    {
                        if (item == null) { continue; }
                        var user = ssoUserOfWork.GetUserByID(item.UserID);
                        list.Add(user);
                    }
                    return Json(new { count = list.Count, members = list }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { count = 0, members = list }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetSelfGroupMembers(string groupid)
        {
            var selfid = LoginUserService.ssoUserID;
            var list = new List<GroupMember>();
            var query = unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid && p.UserID != selfid);
            foreach (var item in query)
            {
                var member = new GroupMember();
                member.department = "";
                member.logo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.UserID;
                member.pId = "";
                member.realName = item.NickName;
                member.type = "";
                member.userId = item.UserID;
                member.userType = 0;
                list.Add(member);
            }

            return Json(new { members = list }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult QueryNotReadMsg()
        {
            var selfuid = LoginUserService.ssoUserID;
            var result0 = MsgServices.GetRedisKeyValue<Msg>("IMMsg", selfuid);


            if (result0 != null && result0.Any())
            {
                return Json(new { r = true, msg = "有未读的单人聊天消息" }, JsonRequestBehavior.AllowGet);
            }
            var result1 = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", selfuid);
            if (result1 != null && result1.Any())
            {
                //如果这个人被移出去了这个群组，那么就会一直有未读的群组消息
                /*
                 * 为了解决这个bug 修改如下：
                    1.在Task<ActionResult> WorkChatGroup这个方法中，每次关联当前用户所在群组的时候，将已经查询出的群组的ID和这个result1中的ID比较，若不存在该id，则删除该id
                    2.在自建群删除、编辑的时候，修改对应的IMUnreadGroupMsg中的数据
                    
                 */

                return Json(new { r = true, msg = "有未读的群组消息",groupids=result1 }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, msg = "没有未读的消息" }, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 获取用户所在的工作群组 WCF
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult GetUserGroups(string uid)
        {
            //最后修改为调用webapi
            //http://okcs.dev.cnki.net/pmcwebapi/api/Group/GetGroupList?unitID=9995385f-653f-43f4-b26c-607360c65ae4&userID=a395ef03-3ab2-482d-9ee3-62a67cdaaaeb
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { data = "" }, JsonRequestBehavior.AllowGet);
            }
            var client = new ImOkcsClientClient();
            OKCS.PMC.Model.ViewModel.GroupView[] result = null;
            var errorMsg = "";
            try
            {
                result = client.GetGroupInfos(uid);
                client.Close();
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
                client.Abort();
            }
            return Json(new { data = result, ErrorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }





        /*以下是Web页面所用到的一些接口，在app页面内也有相同的函数，但是返回的数据格式不一样（因为app页面所调用的数据接口，都固定了格式）*/

        /// <summary>
        /// 按照时间查询个人（人与人）聊天记录 最近的某一天的聊天记录 这个场景应用于在聊天界面点击【聊天记录】按钮的时候调用（因为如果按照日期查询，当前天并不一定会有聊天记录数据，所以查找最近的那一天的聊天记录）
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="dt">时间（yyyy-MM-dd）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetPersonalChatHistoryLatest(string fromuid, string touid, int pageNo = 1, int pageSize = 20)
        {
            try
            {
                //按照日期从大到小查询 获取到最近的日期
                var query = unitOfWork.DIMMsg.Get(p =>
                          ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.IsDel != 1)
                        .OrderByDescending(p => p.CreateDate);
                if (query.Any())
                {
                    var dt0 = query.ToList()[0].CreateDate.Value.Date;
                    var dt2 = dt0.AddDays(1);
                    var query2 = unitOfWork.DIMMsg.Get(p =>
                              ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) &&
                              (p.CreateDate.Value >= dt0 && p.CreateDate.Value <= dt2) && p.IsDel != 1)
                            .OrderBy(p => p.CreateDate);

                    if(query2!=null && query2.Any())
                    {
                        if(query2.Count() % pageSize == 0)
                        {
                            pageNo = query2.Count() / pageSize;
                        }
                        else
                        {
                            pageNo = query2.Count() / pageSize + 1;
                        }
                    }
                    //查找这一天最后一页数据
                    return GetPersonalChatHistory(fromuid, touid, dt0, pageNo, pageSize);
                }
                return Json(new
                {
                    status = 0,
                    data = query.ToList(),
                    msg = "未查询到到相关数据",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 按照时间查询个人（人与人）聊天记录
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="dt">时间（yyyy-MM-dd）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <param name="msg">查询关键字</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetPersonalChatHistory(string fromuid, string touid, DateTime dt, int pageNo = 1, int pageSize = 20, string msg = "")
        {
            try
            {
                //带关键字的查询
                if (msg != "")
                {
                    return GetPersonalChatHistoryBymsg(fromuid, touid, msg, pageNo, pageSize);
                }
                //不带关键字的查询
                var dt2 = dt.AddDays(1);
                var query = unitOfWork.DIMMsg.Get(p =>
                          ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) &&
                          (p.CreateDate.Value >= dt && p.CreateDate.Value <= dt2) && p.IsDel != 1)
                        .OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var list = new List<ChatHistoryView>();
                var queryNextDay = unitOfWork.DIMMsg.GetCount(p => ((p.FromuID == fromuid && p.TouID == touid) || p.FromuID == touid && p.TouID == fromuid) && p.CreateDate.Value >= dt2);
                var queryBeforeDay = unitOfWork.DIMMsg.GetCount(p => ((p.FromuID == fromuid && p.TouID == touid) || p.FromuID == touid && p.TouID == fromuid) && p.CreateDate.Value <= dt);
                var hasNextDay = queryNextDay != 0;//是否还有下一天的记录
                var hasBeforeDay = queryBeforeDay != 0;//是否还有前一天的记录
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                if (query.Any())
                {
                    foreach (var imMsg in queryPage)
                    {
                        var chatHistory = Immg2ChatHistoryView(imMsg);
                        list.Add(chatHistory);
                    }
                }
                return Json(new
                {
                    status = 0,
                    msg = "查询成功",
                    data = list,
                    currentPage = pageNo,
                    pageCount = Math.Ceiling(((float)totalCount / pageSize)),
                    datetime = dt.ToString("yyyy-MM-dd"),
                    hasNextDay = hasNextDay,
                    hasBeforeDay = hasBeforeDay,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 查询个人（人与人）聊天记录 根据关键字查询聊天记录
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="msg">关键字</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetPersonalChatHistoryBymsg(string fromuid, string touid, string msg, int pageNo = 1, int pageSize = 20)
        {
            try
            {
                var query = unitOfWork.DIMMsg.Get(p =>
                          ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.Msg.Contains(msg) && p.IsDel != 1)
                        .OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var list = new List<ChatHistoryView>();
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                if (query.Any())
                {
                    foreach (var imMsg in queryPage)
                    {
                        var chatHistory = Immg2ChatHistoryView(imMsg);
                        list.Add(chatHistory);
                    }
                }
                return Json(new
                {
                    status = 0,
                    msg = "查询成功",
                    data = list,
                    currentPage = pageNo,
                    pageCount = Math.Ceiling(((float)totalCount / pageSize)),
                    datetime = DateTime.Now.ToString("yyyy-MM-dd"),
                    hasNextDay = false,
                    hasBeforeDay = false,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 按照时间查询群组（群组内）聊天记录 最近的某一天的聊天记录 这个场景应用于在聊天界面点击【聊天记录】按钮的时候调用（因为如果按照日期查询，当前天并不一定会有聊天记录数据，所以查找最近的那一天的聊天记录）
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="dt">时间（yyyy-MM-dd）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>        
        [ValidateInput(false)]
        public JsonResult GetGroupChatHistoryLatest(string fromuid, string touid, int pageNo = 1, int pageSize = 20)
        {
            try
            {
                //按照日期从大到小查询 获取到最近的日期
                var query = unitOfWork.DIMMsg.Get(p => p.TouID == touid &&p.IsDel!=1)
                        .OrderByDescending(p => p.CreateDate);
                if (query.Any())
                {
                    var dt0 = query.ToList()[0].CreateDate.Value.Date;
                    var dt2 = dt0.AddDays(1);
                    var query2 = unitOfWork.DIMMsg.Get(p => (p.TouID == touid) && p.Type != "6" && (p.CreateDate.Value >= dt0 && p.CreateDate.Value <= dt2) && p.IsDel != 1).OrderBy(p => p.CreateDate);

                    if (query2 != null && query2.Any())
                    {
                        if(query2.Count() % pageSize==0)
                        {
                            pageNo = query2.Count() / pageSize;
                        }
                        else
                        {
                            pageNo = query2.Count() / pageSize + 1;
                        }
                    }
                    //查找这一天最后一页数据
                    return GetGroupChatHistory(fromuid, touid, dt0, pageNo, pageSize);
                }
                return Json(new
                {
                    status = 0,
                    data = query.ToList(),
                    msg = "未查询到到相关数据",
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 按照时间查询群组（群组内）聊天记录
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="dt">时间（yyyy-MM-dd）</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <param name="msg">查询关键字</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetGroupChatHistory(string fromuid, string touid, DateTime dt, int pageNo = 1, int pageSize = 20, string msg = "")
        {
            try
            {
                //带关键字的查询
                if (msg != "")
                {
                    return GetGroupChatHistoryBymsg(fromuid, touid, msg, pageNo, pageSize);
                }
                //不带关键字的查询
                var dt2 = dt.AddDays(1);
                var query = unitOfWork.DIMMsg.Get(p => (p.TouID == touid) && p.Type != "6" && (p.CreateDate.Value >= dt && p.CreateDate.Value <= dt2) && p.IsDel != 1).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var list = new List<ChatHistoryView>();
                var queryNextDay = unitOfWork.DIMMsg.GetCount(p => (p.TouID == touid) && p.CreateDate.Value >= dt2);
                var queryBeforeDay = unitOfWork.DIMMsg.GetCount(p => (p.TouID == touid) && p.CreateDate.Value <= dt);
                var hasNextDay = queryNextDay != 0;//是否还有下一天的记录
                var hasBeforeDay = queryBeforeDay != 0;//是否还有前一天的记录
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                if (query.Any())
                {
                    foreach (var imMsg in queryPage)
                    {
                        var chatHistory = Immg2ChatHistoryView(imMsg);
                        list.Add(chatHistory);
                    }
                }
                return Json(new
                {
                    status = 0,
                    msg = "查询成功",
                    data = list,
                    currentPage = pageNo,
                    pageCount = Math.Ceiling(((float)totalCount / pageSize)),
                    datetime = dt.ToString("yyyy-MM-dd"),
                    hasNextDay = hasNextDay,
                    hasBeforeDay = hasBeforeDay,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 查询群组（群组内）聊天记录 根据关键字查询聊天记录
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="msg">关键字</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetGroupChatHistoryBymsg(string fromuid, string touid, string msg, int pageNo = 1, int pageSize = 20)
        {
            try
            {
                var query = unitOfWork.DIMMsg.Get(p => (p.TouID == touid) && p.Type != "6" && p.Msg.Contains(msg) && p.IsDel != 1).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var list = new List<ChatHistoryView>();
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                if (query.Any())
                {
                    foreach (var imMsg in queryPage)
                    {
                        var chatHistory = Immg2ChatHistoryView(imMsg);
                        list.Add(chatHistory);
                    }
                }
                return Json(new
                {
                    status = 0,
                    msg = "查询成功",
                    data = list,
                    currentPage = pageNo,
                    pageCount = Math.Ceiling(((float)totalCount / pageSize)),
                    datetime = DateTime.Now.ToString("yyyy-MM-dd"),
                    hasNextDay = false,
                    hasBeforeDay = false,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 按照时间查询个人（人与人）聊天记录，这个查询用在上一页、下一页 跨天的情况下
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="dt">时间（yyyy-MM-dd）</param>
        /// <param name="addday">只接收1 和 -1 两个参数 1表示下一天，-1表示前一天</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [ValidateInput(false)]
        public JsonResult GetPersonalChatHistoryOverPage(string fromuid, string touid, DateTime dt, int addday = 1, int pageNo = 1, int pageSize = 20)
        {
            try
            { 
                //查询dt之前的有聊天记录的那一天的数据 
                var query = unitOfWork.DIMMsg.Get(p =>
                          ((p.FromuID == fromuid && p.TouID == touid) || p.FromuID == touid && p.TouID == fromuid)&& p.Type!= "6" && p.IsDel != 1).OrderByDescending(p => p.CreateDate).GroupBy(p => p.CreateDate.Value.ToString("yyyy-MM-dd")).ToList();
                var newdt = DateTime.Now;
                for (var i = 0; i < query.Count; i++)
                {
                    var dtenum = Convert.ToDateTime(query[i].Key);
                    if (dtenum == dt)
                    {
                        switch (addday)
                        {
                            case 1:
                                newdt = Convert.ToDateTime(query[i - 1].Key);
                                break;
                            case -1:
                                newdt = Convert.ToDateTime(query[i + 1].Key);
                                break;
                        }
                        break;
                    }
                }
                return GetPersonalChatHistory(fromuid, touid, newdt, pageNo, pageSize);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 按照时间查询群组（群组内）聊天记录，这个查询用在上一页、下一页 跨天的情况下
        /// </summary>
        /// <param name="fromuid">发送人</param>
        /// <param name="touid">接收人</param>
        /// <param name="dt">时间（yyyy-MM-dd）</param>
        /// <param name="addday">只接收1 和 -1 两个参数 1表示下一天，-1表示前一天</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        public JsonResult GetGroupChatHistoryOverPage(string fromuid, string touid, DateTime dt, int addday = 1, int pageNo = 1, int pageSize = 20)
        {
            try
            {
                //查询dt之前的有聊天记录的那一天的数据 去掉【加入群组视频消息】
                var query = unitOfWork.DIMMsg.Get(p => p.TouID == touid && p.Type!= "6" && p.IsDel != 1).OrderByDescending(p => p.CreateDate).GroupBy(p => p.CreateDate.Value.ToString("yyyy-MM-dd")).ToList();
                var newdt = DateTime.Now;
                for (var i = 0; i < query.Count; i++)
                {
                    var dtenum = Convert.ToDateTime(query[i].Key);
                    if (dtenum == dt)
                    {
                        switch (addday)
                        {
                            case 1:
                                newdt = Convert.ToDateTime(query[i - 1].Key);
                                break;
                            case -1:
                                newdt = Convert.ToDateTime(query[i + 1].Key);
                                break;
                        }
                        break;
                    }
                }
                return GetGroupChatHistory(fromuid, touid, newdt, pageNo, pageSize);
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        statue = -1,
                        msg = "操作失败" + ex.ToString()
                    }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 同意/拒绝的申请的时候，将这条信息存放到Redis中
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="groupname"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public JsonResult SaveGroupApplyResult(string uid, string groupname, int status)
        {
            if (status != 1 && status != -1)
            {
                return Json(new { Success = false, Content = "", Error = "【status】参数不正确", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            var item = new GroupApplyResult();
            item.TrueName = "您";
            item.GroupName = groupname;
            item.Msg = "";
            item.PostDate = DateTime.Now.ToLongDateString();
            if (status == 1)
            {
                item.ApplyResult = "已成功加入群";
            }
            else if (status == -1)
            {
                item.ApplyResult = "被拒绝加入群";
            }
            var list = RedisHelper.Hash_Get<List<GroupApplyResult>>("GroupApplyResult", uid) ?? new List<GroupApplyResult>();
            list.Add(item);
            RedisHelper.Hash_Remove("GroupApplyResult", uid);
            RedisHelper.Hash_Set<List<GroupApplyResult>>("GroupApplyResult", uid, list);
            return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 用户从Redis中读取自己的申请入群结果
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult GetGroupApplyResult(string uid)
        {
            var result = RedisHelper.Hash_Get<List<GroupApplyResult>>("GroupApplyResult", uid);
            if (result != null)
            {
                result.Reverse();
                //获取一次就清空了，表示已经读取过了
                RedisHelper.Hash_Remove("GroupApplyResult", uid);
            }
            return Json(new { Success = true, Content = result, Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 获取当前机构所有成员，先查询Redis缓存，如果有则从缓存中获取，如果没有则从sso接口获取
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOrgMembers()
        {
            var currentuser = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID);
            var memberList = RedisHelper.Hash_Get<List<object>>("AllMembers", currentuser.OrgId);

            if (memberList == null)
            {
                memberList = ssoUserOfWork.GetMembersOfUnit(currentuser.OrgId);
                return Json(new { members = memberList }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = new List<object>();
                JavaScriptSerializer jss = new JavaScriptSerializer();
                foreach (var o in memberList)
                {
                    var o2 = jss.Deserialize<Dictionary<string, object>>(o.ToString());
                    list.Add(o2);
                }
                return Json(new { members = list }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 从Redis缓存获取工作群组的群组简介 2019年3月6日新增
        /// </summary>
        /// <returns></returns>
        public ActionResult GetGroupInfo(string groupid)
        {
            var uid = LoginUserService.ssoUserID;
            var queryWorkGroujp = RedisHelper.Hash_Get<List<GroupViewNew>>("IMGroup", uid);
            if (queryWorkGroujp != null && queryWorkGroujp.Any())
            {
                var group = queryWorkGroujp.FirstOrDefault(p => p.ID == groupid);
                if (group != null)
                {
                    return Json(new { Content = group.Summary }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { Content = "" }, JsonRequestBehavior.AllowGet);
        }


        private ChatHistoryView Immg2ChatHistoryView(IMMsg imMsg)
        {
            var chatHistory = new ChatHistoryView
            {
                ID = imMsg.ID.ToString(),
                FromID = imMsg.FromuID,
                ToID = imMsg.TouID,
                Msg = imMsg.Msg,
                CreateDate = imMsg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                FromUserTrueName = imMsg.fromusername,
                ToUserTrueName = imMsg.tousername,
                MsgType = imMsg.Type,
            };
            if (imMsg.Type == "1")
            {
                chatHistory.Msg = "<img class=\"imgA\" src=\"" + imMsg.Msg + "\"/>";

            }
            else if (imMsg.Type == "2")
            {
                chatHistory.Msg = "<a class=\"layui-layim-file\" href=\"" + imMsg.FileUrl + "\" target=\"_blank\"><i class=\"layui-icon\"></i><cite>" + imMsg.Msg + "</cite></a>";
            }
            else if (imMsg.Type == "3")
            {
                var k = 0;
                int index = imMsg.Msg.IndexOf("[$PICTURE$]");
                if (!string.IsNullOrEmpty(imMsg.ImgList))
                {
                    var imglist = imMsg.ImgList.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    while (index > -1)
                    {
                        var imgele = "<img class=\"imgA\" src=\"" + imglist[k] + "\" />";
                        imMsg.Msg = imMsg.Msg.Remove(index, 11).Insert(index, imgele);
                        index = imMsg.Msg.IndexOf("[$PICTURE$]");
                        k++;
                    }
                    chatHistory.Msg = imMsg.Msg;
                }
            }
            else if (imMsg.Type == "4")
            {
                //语音类消息
                chatHistory.Msg = "<div id=\"" + imMsg.ID + "\" class=\"yuyin fl\" style=\"width:2px\" audioSize=\"2\"><span class=\"yuyin_txt yy_txt_l\">" + imMsg.Duration + "''</span><audio class=\"myaudio\" preload=\"auto\" hidden=\"true\"><source src=\"" + imMsg.FileUrl + "\" type=\"audio/mpeg\"></audio><span class=\"yuyin_img yuyin_img_l\"></span></div>";
            }
            else if (imMsg.Type == "5")
            {
                //语音类消息
                //imMsg.Msg中存放的是Json字符串 例如：{"latitude":"116.397428, 39.90923","address":"北京市北京市海淀区华宇信息郭行行"}
                var p = JsonConvert.DeserializeObject<GDPosition>(imMsg.Msg);
                var mapid = "mapinhistory" + imMsg.ID;
                chatHistory.Msg = "<div class=\"map-item\"><div class=\"mapi-top mapi-history\" map-data=\"" + p.latitude + "\" id=\"" + mapid + "\"></div><p class=\"mapi-mid\">位置</p><p class=\"mapi-bot\" title=\"" + p.address + "\">" + p.address + "</p></div>";
            }
            return chatHistory;
        }



        /// <summary>
        /// 服务端日志查看页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ServiceLog()
        {
            var filename = DateTime.Now.ToString("yyyy-MM-dd")+ ".txt";

            ViewBag.filename = filename;

            return View();
        }
    }
}