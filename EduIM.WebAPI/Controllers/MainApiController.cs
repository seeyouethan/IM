using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Edu.Entity;
using Edu.Models.IM;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.Service;
using Edu.Service.Service;
using Edu.Tools;
using EduIM.WebAPI.Filters;
using MassTransitConfig = EduIM.WebAPI.Service.MassTransitConfig;
using Edu.Web.Models;
using System.Web.Script.Serialization;
using Edu.Service.Service.Message;
using EduIM.WebAPI.Models;
using EduIM.WebAPI.Service;
using log4net;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Text;

namespace EduIM.WebAPI.Controllers
{

    //[WebApiTracker]
    public class MainApiController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private readonly ssoUser _ssoUserOfWork = new ssoUser();



        /// <summary>
        /// 根据关键字查询用户
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="keyword">查询关键字</param>
        /// <param name="pageNo">页码（默认为1，从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult SearchUser(string userId, string keyword, int pageNo = 1, int pageSize = 10)
        {
            var currentuser = _ssoUserOfWork.GetUserByID(userId);
            var list = new List<UserView>();
            try
            {
                var count = 0;
                var query =
                    _ssoUserOfWork.GetDeptUsersByRealName(currentuser.OrgId, keyword, pageNo, pageSize, out count)
                        .Where(p => p.UserId != userId);
                list.AddRange(query.Select(deptUser => new UserView
                {
                    UserId = deptUser.UserId,
                    RealName = deptUser.RealName,
                    UserName = deptUser.UserName,
                    IsFriend = false,
                    Department = deptUser.DeptName,
                    Photo = System.Web.HttpUtility.UrlEncode(
                        ConfigHelper.GetConfigString("sso_host_name") + "pic/" + deptUser.UserId, System.Text.Encoding.UTF8),
                    Phone = deptUser.Mobile,
                }));
                //查找到自己的常用联系人 获取其中的uid 组成一个List
                var queryTopContact = _unitOfWork.DTopContacts.Get(p => ((p.uID == userId)||(p.ContactID == userId)));
                if (queryTopContact != null)
                {
                    foreach (var userview in list)
                    {
                        var k = queryTopContact.Where(p => p.ContactID == userview.UserId || p.uID == userview.UserId);
                        userview.IsFriend = k.Any();
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
                        Total = count,
                        //currentPage = pageNo,
                        //pageCount = Math.Ceiling(((float)count / pageSize))
                    });
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
                    });
            }
        }

        /// <summary>
        /// 根据用户id获取该机构内所有成员
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetAllOrgMembers(string uid,string orgid)
        {
            var memberList = _ssoUserOfWork.GetMembersOfUnit(orgid);
            List<SUser> list = new List<SUser>();
            JavaScriptSerializer jsonser = new JavaScriptSerializer();
            string myJson = jsonser.Serialize(memberList);
            list = JsonConvert.DeserializeObject<List<SUser>>(myJson);
            var listtemp = list.Where(p => p.type == 0 && p.id!=uid).ToList();

            //随机取100个成员  2019年8月12日 新增功能 2019年8月14日去掉了
            //var k = listtemp.OrderBy(x => Guid.NewGuid()).Take(100).ToList();
            //查询好友
            var queryFriend = _unitOfWork.DTopContacts.Get(p => (p.uID == uid||p.ContactID==uid)).ToList();
            var listResult= new List<UserView>();
            foreach (var item in listtemp)
            {
                var newItem=new UserView();
                newItem.Department = item.department;
                newItem.UserId = item.id;
                newItem.IsFriend = false;
                //是否是好友
                var isfrient = queryFriend.Where(p => p.ContactID == item.id || p.uID == item.id);
                if (isfrient != null && isfrient.Any())
                {
                    newItem.IsFriend = true;
                }

                newItem.Photo = System.Web.HttpUtility.UrlEncode(
                    ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.id, System.Text.Encoding.UTF8);
                newItem.RealName = item.RealName;
                newItem.UserName = item.userName;
                newItem.Phone = item.Mobile;
                listResult.Add(newItem);
            }
            try
            {
                return Json(
                    new
                    {
                        Success = true,
                        Content = listResult,
                        Error = "",
                        Message = "查询成功",
                        Count = listResult.Count,
                        Total = listResult.Count
                });
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
                    });
            }
           
        }

        /// <summary>
        /// 获取机构内所有的用户，包括部门以及对应的层级关系 为直播页面提供的接口  在直播页面的CreateMeeting/index调用
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetOrgMembersForLive(string uid,string orgid)
        {
            var memberList = _ssoUserOfWork.GetMembersOfUnit(orgid);
            List<SUser> list = new List<SUser>();
            JavaScriptSerializer jsonser = new JavaScriptSerializer();
            string myJson = jsonser.Serialize(memberList);
            list = JsonConvert.DeserializeObject<List<SUser>>(myJson);
            var listtemp = list.Where(p => p.id != uid).ToList();

            var listResult = new List<SsoMembers>();
            foreach (var item in listtemp)
            {
                var newItem = new SsoMembers();
                newItem.userId = item.id;
                newItem.pId = item.pid;
                newItem.department = item.department;
                newItem.realName = item.RealName;
                newItem.logo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.id;
                newItem.type = item.type;
                listResult.Add(newItem);
            }

            return Json(
                    new
                    {
                        Success = true,
                        Content = listResult,
                        Error = "",
                        Message = "查询成功",
                        Count = listResult.Count,
                        Total = listResult.Count
                    });

        }

        /// <summary>
        /// 获取机构内所有的用户，包括部门以及对应的层级关系
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="orgid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetOrgMembers(string uid, string orgid)
        {
            var memberList = _ssoUserOfWork.GetMembersOfUnit(orgid);
            List<SUser> list = new List<SUser>();
            JavaScriptSerializer jsonser = new JavaScriptSerializer();
            string myJson = jsonser.Serialize(memberList);
            list = JsonConvert.DeserializeObject<List<SUser>>(myJson);
            var listtemp = list.Where(p => p.id != uid).ToList();

            var listResult = new List<UserViewNew>();
            foreach (var item in listtemp)
            {
                var newItem = new UserViewNew();
                newItem.Id = item.id;
                newItem.ParentId = item.pid;
                newItem.Department = item.department;
                newItem.RealName = item.RealName;
                newItem.UserId = "";
                newItem.Phone = "";
                newItem.UserName = "";
                if (item.type == 0)
                {
                    newItem.UserId = item.id;
                    newItem.UserName = item.userName;
                    newItem.Phone = item.Mobile;
                }
                listResult.Add(newItem);
            }

            return Json(
                    new
                    {
                        Success = true,
                        Content = listResult,
                        Error = "",
                        Message = "查询成功",
                        Count = listResult.Count,
                        Total = listResult.Count
                    });

        }


        [HttpGet]
        public IHttpActionResult GetAllOrgMembersWithDepartments(string uid, string orgid)
        {
            var memberList = _ssoUserOfWork.GetMembersOfUnit(orgid);
            List<SUser> list = new List<SUser>();
            JavaScriptSerializer jsonser = new JavaScriptSerializer();
            string myJson = jsonser.Serialize(memberList);
            list = JsonConvert.DeserializeObject<List<SUser>>(myJson);
            var listtemp = list.Where(p => p.id != uid).ToList();
            //查询好友
            var queryFriend = _unitOfWork.DTopContacts.Get(p => (p.uID == uid || p.ContactID == uid)).ToList();
            var listResult = new List<UserView>();
            foreach (var item in listtemp)
            {
                var newItem = new UserView();
                newItem.Department = item.department;
                newItem.UserId = item.id;
                newItem.IsFriend = false;
                //是否是好友
                var isfrient = queryFriend.Where(p => p.ContactID == item.id || p.uID == item.id);
                if (isfrient != null && isfrient.Any())
                {
                    newItem.IsFriend = true;
                }

                newItem.Photo = System.Web.HttpUtility.UrlEncode(
                    ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.id, System.Text.Encoding.UTF8);
                newItem.RealName = item.RealName;
                newItem.UserName = item.userName;
                newItem.Phone = item.Mobile;
                listResult.Add(newItem);
            }
            try
            {
                return Json(
                    new
                    {
                        Success = true,
                        Content = listResult,
                        Error = "",
                        Message = "查询成功",
                        Count = listResult.Count,
                        Total = listResult.Count
                    });
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
                    });
            }

        }

        /// <summary>
        /// 获取当前用户所在的自建群组
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="pageNo">页码（默认为1，从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetSelfGroups(string userId, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var selfuid = userId;
                var query =
                    _unitOfWork.DImGroupDetail.Get(p => p.UserID == userId)
                        .OrderByDescending(p => p.ID);
                var totalCount = 0;
                totalCount = query.Count();
                List<ImGroup> list = new List<ImGroup>();
                List<ImGroupView> listview = new List<ImGroupView>();
                if (pageNo == 0 && pageSize == 0)
                {
                    //获取所有
                    totalCount = query.Count();
                    list.AddRange(
                        query.Select(item => Convert.ToInt32(item.GroupID))
                            .Select(id => _unitOfWork.DImGroup.Get(p => p.ID == id).FirstOrDefault()));
                    foreach (var item in list)
                    {
                        var imgroupview = new ImGroupView
                        {
                            ID = item.ID.ToString(),
                            CreateDate = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                            Name = item.Name,
                            Photo =
                                System.Web.HttpUtility.UrlEncode(
                                    System.Configuration.ConfigurationManager.AppSettings.Get("IMWebApiGroupPic") +
                                    item.ID, System.Text.Encoding.UTF8), //这里作为一个接口url返还回去，目的是将base64字符串作为一个图片文件返还回去
                            Des = item.Des,
                            Creator = item.Creator,
                            IsAdmin = item.Creator == selfuid
                        };
                        //未读消息个数
                        var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item.ID + selfuid);
                        imgroupview.UnreadMsgCount = (all == null ? 0 : all.Count);
                        listview.Add(imgroupview);
                    }
                    return Json(
                        new
                        {
                            Success = true,
                            Content = listview,
                            Error = "",
                            Message = "查询成功",
                            Count = listview.Count,
                            Total = totalCount,
                        });
                }
                else
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    list = new List<ImGroup>();
                    listview = new List<ImGroupView>();
                    list.AddRange(queryPage.Select(item => Convert.ToInt32(item.GroupID)).Select(id => _unitOfWork.DImGroup.Get(p => p.ID == id).FirstOrDefault()));
                    foreach (var item in list)
                    {
                        var imgroupview = new ImGroupView
                        {
                            ID = item.ID.ToString(),
                            CreateDate = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                            Name = item.Name,
                            Photo = System.Web.HttpUtility.UrlEncode(System.Configuration.ConfigurationManager.AppSettings.Get("IMWebApiGroupPic") + item.ID, System.Text.Encoding.UTF8),//这里作为一个接口url返还回去，目的是将base64字符串作为一个图片文件返还回去
                            Des = item.Des,
                            Creator = item.Creator,
                            IsAdmin = item.Creator == selfuid
                        };
                        //未读消息个数
                        var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item.ID + selfuid);
                        imgroupview.UnreadMsgCount = (all == null ? 0 : all.Count);
                        listview.Add(imgroupview);
                    }
                    return Json(
                        new
                        {
                            Success = true,
                            Content = listview,
                            Error = "",
                            Message = "查询成功",
                            Count = listview.Count,
                            Total = totalCount,
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("GetSelfGroups",ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 获取当前用户所在的群组，包括工作群和自建群
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="pageNo">页码（默认为1，从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetAllGroups(string userId, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var chatGroupList=new List<MyGroup>();
                var list=new List<MyGroup>();
                /*
                 * 1.发送请求，获取工作群
                 */
                //var unitid = _ssoUserOfWork.GetUserByID(userId).OrgId;
                var unitid = "";
                var url = ConfigHelper.GetConfigString("GetMyGroups") + "?unitID=" + unitid + "&userID=" + userId + "&pageSize=999";
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultJsonContent result = JsonConvert.DeserializeObject<PmcJsonResultJsonContent>(resp);
                if (result != null && result.Content!=null)
                {
                    list.AddRange(result.Content);
                }
                /*
                 * 2查询数据库，获取聊天群工作群  已经去掉 2019年12月30日19:48:02
                 */
                //var query =
                //    _unitOfWork.DImGroupDetail.Get(p => p.UserID == userId)
                //        .OrderByDescending(p => p.CreateDate);
                //if (query != null && query.Any())
                //{
                //    foreach (var chatGroup in query)
                //    {
                //        var intId = Convert.ToInt32(chatGroup.GroupID);
                //        var queryChatGroup = _unitOfWork.DImGroup.Get(p => p.ID == intId).FirstOrDefault();
                //        var tempGroup = ImGroup2MyGroup(queryChatGroup);
                //        tempGroup.CanManage = (chatGroup.isAdmin == 1);
                //        chatGroupList.Add(tempGroup);
                //    }
                //}
                //list.AddRange(chatGroupList);
                //排序
                list = list.OrderByDescending(p => Convert.ToDateTime(p.PostTime)).ToList();
                if (pageNo == 0 && pageSize == 0)
                {
                    //获取所有
                    return Json(
                        new
                        {
                            Success = true,
                            Content = list,
                            Error = "",
                            Message = "查询成功",
                            Count = list.Count,
                            Total = list.Count,
                        });
                }
                else
                {
                    var lisgPage = list.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                    return Json(
                        new
                        {
                            Success = true,
                            Content = lisgPage,
                            Error = "",
                            Message = "查询成功",
                            Count = lisgPage.Count,
                            Total = list.Count,
                        });
                }
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
                    });
            }
        }

        /// <summary>
        /// 根据自建群ID获取自建群群组的信息
        /// </summary>
        /// <param name="groupid">自建群ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetSelfGroupInfo(string groupid)
        {
            try
            {
                var gid = Convert.ToInt32(groupid);
                var group = _unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
                if (group != null)
                {
                    var item = new SelfGroup();
                    var creator = _ssoUserOfWork.GetUserByID(group.Creator);
                    if (creator != null)
                    {
                        item.creatorrealname = creator.RealName;
                    }
                    item.id = group.ID.ToString();
                    item.createtime= group.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                    item.des = group.Des;
                    item.name = group.Name;
                    item.photo =ConfigHelper.GetConfigString("IMWebApiGroupPic") + groupid;
                    item.creatorid = group.Creator;
                    //item.creatorrealname = creator.RealName;
                    item.members=new List<UserView>();
                    var queryMember = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid && p.isAdmin!=1).ToList();
                    if (queryMember.Any())
                    {
                        foreach (var member in queryMember)
                        {
                            var ssoUser = _ssoUserOfWork.GetUserByID(member.UserID);
                            if (ssoUser == null)
                            {
                                continue;
                            }
                            var info = new UserView()
                            {
                                UserId= member.UserID,
                                RealName = ssoUser.RealName,
                                Photo = ConfigHelper.GetConfigString("SsoPic") + member.UserID,
                                IsFriend=false,
                                Department = ssoUser.DeptName,
                                UserName = ssoUser.UserName,Phone=ssoUser.Mobile,
                            };
                            item.members.Add(info);
                        }
                    }

                    return Json(
                        new
                        {
                            Success = true,
                            Content = item,
                            Error = "",
                            Message = "查询成功",
                            Count = 1,
                            Total = 1
                        });
                }
                
                return Json(
                    new
                    {
                        Success = true,
                        Content = "未找到相关群组",
                        Error = "",
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
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
                    });
            }
        }

        /// <summary>
        /// 根据自建群ID获取自建群组群组成员基本信息
        /// </summary>
        /// <param name="groupid">自建群ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetSelfGroupMembers(string groupid)
        {
            try
            {
                var gid = Convert.ToInt32(groupid);
                //var group = _unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
                var query = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
                List<UserView> list = new List<UserView>();
                foreach (var item in query)
                {
                    var ssoUser = _ssoUserOfWork.GetUserByID(item.UserID);
                    if (ssoUser == null)
                    {
                        continue;
                    }
                    var member = new UserView();
                    member.UserId = item.UserID;
                    //member.CreateDate = item.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                    member.RealName = item.NickName;
                    member.Photo = System.Web.HttpUtility.UrlEncode(ConfigHelper.GetConfigString("sso_host_name") + "pic/" + member.UserId, System.Text.Encoding.UTF8);
                    member.Department = ssoUser.DeptName;
                    member.IsFriend = false;
                    member.UserName = ssoUser.UserName;
                    member.Phone = ssoUser.Mobile;
                    list.Add(member);
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = 0
                    });
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
                    });
            }
        }

        /// <summary>
        /// 获取当前用户的好友列表
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="pageNo">页码（从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetMyFriends(string userId, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                //查询常用联系人
                var selfuid = userId;
                var query = _unitOfWork.DTopContacts.Get(p => p.uID == selfuid || p.ContactID==selfuid).OrderByDescending(p => p.ContactDate);
                var totalCount = query.Count();
                var list = new List<UserView>();

                if (pageNo == 0 && pageSize == 0)
                {
                    //查询所有好友，不分页
                    foreach (var item in query)
                    {
                        var frienduid = item.uID == selfuid ? item.ContactID : item.uID;

                        var ssoUser = _ssoUserOfWork.GetUserByID(frienduid);
                        if (ssoUser == null)
                        {
                            continue;
                        }
                        var user = new UserView()
                        {
                            UserId = frienduid,
                            RealName = ssoUser.RealName,
                            UserName = ssoUser.UserName,
                            IsFriend = true,
                            Photo = System.Web.HttpUtility.UrlEncode(
                                ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.ContactID, System.Text.Encoding.UTF8),
                            Department = ssoUser.DeptName,
                            Phone = ssoUser.Mobile,
                        };
                        if (list.Where(p => p.UserId == user.UserId).ToList().Count == 0)
                        {
                            list.Add(user);
                        }
                    }
                }
                else
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var item in queryPage)
                    {
                        var frienduid = item.uID == selfuid ? item.ContactID : item.uID;
                        var ssoUser = _ssoUserOfWork.GetUserByID(frienduid);
                        if (ssoUser == null)
                        {
                            continue;
                        }
                    
                        var user = new UserView()
                        {
                            UserId = frienduid,
                            RealName = ssoUser.RealName,
                            UserName = ssoUser.UserName,
                            IsFriend = true,
                            Photo = System.Web.HttpUtility.UrlEncode(
                                ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.ContactID, System.Text.Encoding.UTF8),
                            Department = ssoUser.DeptName,
                            Phone = ssoUser.Mobile,
                        };
                        if (list.Where(p => p.UserId == user.UserId).ToList().Count == 0)
                        {
                            list.Add(user);
                        }
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
                    });
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
                    });
            }
        }

        /// <summary>
        /// 新建自建群组
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupname">群组名称</param>
        /// <param name="des">群组描述</param>
        /// <param name="glist">群组成员uid,用分号隔开</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CreateSelfGroup()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["createUserID"];
                string groupname = HttpContext.Current.Request.Form["name"];
                string des = HttpContext.Current.Request.Form["summary"];
                string glist = HttpContext.Current.Request.Form["memberIdList"];
                string unitid = HttpContext.Current.Request.Form["unitID"];//暂时没用到
                var uid = userId;
                var currentuser = _ssoUserOfWork.GetUserByID(userId);
                if (currentuser == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "sso未查询到对用的用户",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(groupname))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组名称不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(glist))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组成员不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (groupname.Length > 50)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组名称不能超过50个字符",
                        Count = 0,
                        Total = 0
                    });
                }
                if (des.Length > 500)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组描述不能超过500个字符",
                        Count = 0,
                        Total = 0
                    });
                }
                var group = new ImGroup();
                group.Name = groupname;
                group.Photo = "";//群组图片，默认为空，后期会增加上传群组头像功能
                //group.Photo = GetGroupPhoto(uid,glist);//群组图片
                group.Des = des;
                group.Creator = uid;
                group.CreateDate = DateTime.Now;
                group.TypeID = 2;//等于2 表示从App端创建
                _unitOfWork.DImGroup.Insert(group);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    /*先创建管理员*/
                    var gid = group.ID;
                    var gidstr = Convert.ToString(gid);
                    var owner = new ImGroupDetail
                    {
                        GroupID = gidstr,
                        UserID = uid,
                        NickName = currentuser.RealName,
                        isAdmin = 1,
                        Creator = uid,
                        CreateDate = DateTime.Now,
                        photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + uid,
                    };
                    _unitOfWork.DImGroupDetail.Insert(owner);
                    /*再添加成员*/
                    var mList = glist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var memberuid in mList)
                    {
                        var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                        if (ssoUser == null)
                        {
                            continue;
                        }
                        var member = new ImGroupDetail
                        {
                            GroupID = gidstr,
                            UserID = memberuid,
                            NickName = ssoUser.RealName,
                            isAdmin = 0,
                            Creator = uid,
                            CreateDate = DateTime.Now,
                            photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + memberuid
                        };
                        _unitOfWork.DImGroupDetail.Insert(member);
                    }
                    result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        //向MQ中发通知，向移动的推送一条消息 创建自建群 的消息
                        var msg = new SelfGroupNotice
                        {
                            groupid = gidstr,
                            groupname = groupname,
                            creator = uid,
                            photo = System.Web.HttpUtility.UrlEncode(System.Configuration.ConfigurationManager.AppSettings.Get("IMWebApiGroupPic") + group.ID, System.Text.Encoding.UTF8),
                            createtime = group.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                            membersList = mList.ToList(),
                            describe = des
                        };
                        var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                        queueAddress.Send<Edu.Web.Models.SelfGroupNotice>(msg);
                        group.Photo = ConfigHelper.GetConfigString("IMWebApiGroupPic") + group.ID;
                        var workgroup=ImGroup2MyGroup(group);
                        return Json(new
                        {
                            Success = true,
                            Content = workgroup,
                            Error = "",
                            Message = "创建成功",
                            Count = 0,
                            Total = 0
                        });
                    }
                    else
                    {
                        LoggerHelper.Error(result.ToString());
                        return Json(new
                        {
                            Success = false,
                            Content = "",
                            Error = result.ToString(),
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });
                    }
                }
                else
                {
                    LoggerHelper.Error(result.ToString());
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = result.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 编辑自建群
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupid">群组ID</param>
        /// <param name="groupname">群组名称</param>
        /// <param name="des">群组描述</param>
        /// <param name="glist">群组成员uid,用分号隔开</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditSelfGroup()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string groupid = HttpContext.Current.Request.Form["groupid"];
                string groupname = HttpContext.Current.Request.Form["groupname"];
                string des = HttpContext.Current.Request.Form["des"];
                string addlist = HttpContext.Current.Request.Form["addlist"];
                string removelist = HttpContext.Current.Request.Form["removelist"];
                var uid = userId;

                var isEdit = false;
                var isAdd = false;
                var isDelete = false;

                if (string.IsNullOrEmpty(groupid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (groupname.Length > 50)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组名称不能超过50个字符",
                        Count = 0,
                        Total = 0
                    });
                }
                if (des.Length > 500)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组描述不能超过500个字符",
                        Count = 0,
                        Total = 0
                    });
                }

                var gid = Convert.ToInt32(groupid);
                var query = _unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到相关的群组信息",
                        Count = 0,
                        Total = 0
                    });
                }
                if (!string.IsNullOrEmpty(groupname))
                {
                    if (groupname != query.Name)
                    {
                        isEdit = true;
                    }
                    query.Name = groupname;
                }
                if (!string.IsNullOrEmpty(des))
                {
                    if (des != query.Des)
                    {
                        isEdit = true;
                    }
                    query.Des = des;
                }
                query.CreateDate = DateTime.Now;
                _unitOfWork.DImGroup.Update(query);


                /*先移除要删除成员*/
                if (!string.IsNullOrEmpty(removelist))
                {
                    var mListRemove = removelist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    var queryMembers =
                        _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid && mListRemove.Contains(p.UserID));
                    _unitOfWork.DImGroupDetail.Delete(queryMembers);
                    isDelete = true;
                }
                /*再添加要新增的成员*/
                if (!string.IsNullOrEmpty(addlist))
                {
                    var mList = addlist.Split(new Char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var memberuid in mList)
                    {
                        var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                        if (ssoUser == null)
                        {
                            continue;
                        }
                        var member = new ImGroupDetail
                        {
                            GroupID = groupid,
                            UserID = memberuid,
                            NickName = ssoUser.RealName,
                            isAdmin = 0,
                            Creator = userId,
                            CreateDate = DateTime.Now,
                            photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + memberuid
                        };
                        _unitOfWork.DImGroupDetail.Insert(member);
                    }
                    isAdd = true;
                }
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    //接收通知的成员
                    var toList = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid).Select(p => p.UserID).ToList();
                    if (isEdit)
                    {
                        Task.Run(()=>
                        {
                            //发送通知 修改了群组信息
                            var msg = new SelfGroupNoticeMore
                            {
                                groupid = groupid,
                                groupname = query.Name,
                                membersList = toList,
                                membername = "",
                                type = 4
                            };
                            var queueAddress =
                                MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                            queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                        });                        
                    }

                    if (isAdd)
                    {
                        Task.Run(() =>
                        {
                            //发送通知 xxx加入了群组
                            var mListAdd = addlist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var memberuid in mListAdd)
                            {
                                var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                                if (ssoUser == null) continue;
                                //向MQ中发通知，向移动的推送一条消息新增成员 的消息
                                var msg = new SelfGroupNoticeMore
                                {
                                    groupid = groupid,
                                    groupname = query.Name,
                                    membersList = toList,
                                    membername = ssoUser.RealName,
                                    type = 0
                                };
                                var queueAddress =
                                    MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                                queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                            }
                        });
                    }
                    if (isDelete)
                    {
                        Task.Run(() =>
                        {
                            //发送通知 xxx退出了群组
                            var mListRemove = removelist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            toList.AddRange(mListRemove);
                            foreach (var memberuid in mListRemove)
                            {
                                var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                                if (ssoUser == null) continue;
                                //向MQ中发通知，向移动的推送一条消息 1成员 的消息
                                var msg = new SelfGroupNoticeMore
                                {
                                    groupid = groupid,
                                    groupname = query.Name,
                                    membersList = toList,
                                    membername = ssoUser.RealName,
                                    type = 1
                                };
                                var queueAddress =
                                    MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                                queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                            }
                        });
                        
                    }

                    
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "编辑成功",
                        Count = 0,
                        Total = 0
                    });
                }
                else
                {
                    LoggerHelper.Error(result.ToString());
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = result.Message,
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 更新自建群头像
        /// </summary>
        /// <param name="groupid">群组ID</param>
        /// <param name="photo">群组名称</param>
        /// <param name="fileList">群组描述</param>
        /// <param name="glist">群组成员uid,用分号隔开</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditSelfGroupPhoto()
        {
            try
            {
                string groupid = HttpContext.Current.Request.Form["groupid"];
                var photo = System.Web.HttpContext.Current.Request.Files;

                if (string.IsNullOrEmpty(groupid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                if (photo.Count<=0)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组头像不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                

                var gid = Convert.ToInt32(groupid);
                var query = _unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到相关的群组信息",
                        Count = 0,
                        Total = 0
                    });
                }
                

                byte[] thePictureAsBytes;
                using (BinaryReader theReader = new BinaryReader(photo[0].InputStream))
                {
                    thePictureAsBytes = theReader.ReadBytes(photo[0].ContentLength);
                }
                string thePictureDataAsString = Convert.ToBase64String(thePictureAsBytes);

                query.Photo = "data:image/png;base64," + thePictureDataAsString;
                _unitOfWork.DImGroup.Update(query);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = System.Configuration.ConfigurationManager.AppSettings.Get("IMWebApiGroupPic") + groupid,
                        Error = "",
                        Message = "编辑成功",
                        Count = 0,
                        Total = 0
                    });
                }
                else
                {
                    LoggerHelper.Error(result.ToString());
                    return Json(new
                    {
                        Success = false,
                        Content = thePictureDataAsString,
                        Error = result.Message,
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 向自建群中添加群成员
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupid">自建群群组ID</param>
        /// <param name="glist">要添加的群组成员uid,用分号隔开</param>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult AddMembersToSelfGroup()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string groupid = HttpContext.Current.Request.Form["groupid"];
                string glist = HttpContext.Current.Request.Form["glist"];
                var uid = userId;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(groupid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(glist))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "要新增的群组成员不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var gid = Convert.ToInt32(groupid);
                var query = _unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到相关的群组信息",
                        Count = 0,
                        Total = 0
                    });
                }
                /*再添加成员*/
                var mList = glist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var memberuid in mList)
                {
                    var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                    if (ssoUser == null)
                    {
                        continue;
                    }
                    var member = new ImGroupDetail
                    {
                        GroupID = groupid,
                        UserID = memberuid,
                        NickName = ssoUser.RealName,
                        isAdmin = 0,
                        Creator = uid,
                        CreateDate = DateTime.Now,
                        photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + memberuid
                    };
                    _unitOfWork.DImGroupDetail.Insert(member);
                }
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    /*写消息队列------start------*/
                    var queryMemberList = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
                    foreach (var memberuid in mList)
                    {
                        var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                        if (ssoUser == null)
                        {
                            continue;
                        }
                        //向MQ中发通知，向移动的推送一条消息 新增成员 的消息
                        var msg = new SelfGroupNoticeMore
                        {
                            groupid = groupid,
                            groupname = query.Name,
                            membersList = queryMemberList.Select(p=>p.UserID).ToList(),
                            membername = ssoUser.RealName,
                            type = 0
                        };
                        var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                        queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                    }
                    /*写消息队列------end------*/
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "编辑成功",
                        Count = 0,
                        Total = 0
                    });
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }
        /// <summary>
        /// 从自建群中移除成员
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteMembersFromSelfGroup()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string groupid = HttpContext.Current.Request.Form["groupid"];
                string removeids = HttpContext.Current.Request.Form["removeids"];
                var uid = userId;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(groupid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(removeids))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "要移除的群组成员ID不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var gid = Convert.ToInt32(groupid);
                var query = _unitOfWork.DImGroup.Get(p => p.ID == gid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到相关的群组信息",
                        Count = 0,
                        Total = 0
                    });
                }

                var mList = removeids.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var removeid in mList)
                {
                    var queryDelMember=
                       _unitOfWork.DImGroupDetail.Get(p => p.isAdmin == 0 && p.UserID == removeid && p.GroupID == groupid).FirstOrDefault();
                    if (queryDelMember == null)
                    {
                        return Json(new
                        {
                            Success = false,
                            Content = "",
                            Error = "",
                            Message = "成员removeid不属于该群组，无法执行删除操作",
                            Count = 0,
                            Total = 0
                        });
                    }
                    else
                    {
                        _unitOfWork.DImGroupDetail.Delete(queryDelMember);
                    }
                }
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    /*写消息队列------start------*/
                    var queryMemberList = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
                    foreach (var memberuid in mList)
                    {
                        var ssoUser = _ssoUserOfWork.GetUserByID(memberuid);
                        if (ssoUser == null)
                        {
                            continue;
                        }
                        //向MQ中发通知，向移动的推送一条消息 移除成员 的消息
                        var msg = new SelfGroupNoticeMore
                        {
                            groupid = groupid,
                            groupname = query.Name,
                            membersList = queryMemberList.Select(p => p.UserID).ToList(),
                            membername = ssoUser.RealName,
                            type = 1
                        };
                        var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                        queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);
                    }
                    /*写消息队列------end------*/

                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "编辑成功",
                        Count = 0,
                        Total = 0
                    });
                }
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
                
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 添加好友
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="touid">要添加好友的id</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddToContact()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string touid = HttpContext.Current.Request.Form["touid"];
                var selfuid = userId;
                if (selfuid == null || touid == null || selfuid == "" || touid == "")
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "操作失败！联系人uid为空！",
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
                //检查这个touid是否是系统内的用户ID
                var user = _ssoUserOfWork.GetUserByID(touid);
                if (user == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "操作失败！联系人uid不合法（未查找到对应的用户）！",
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
                //首先查询，是否有对应的联系人
                var query = _unitOfWork.DTopContacts.Get(p => ((p.uID == userId && p.ContactID == touid)||(p.uID == touid && p.ContactID == userId)));
                if (query != null && query.Any())
                {
                    //这里默认添加成功
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "",
                        Count = 0,
                        Total = 0
                    });
                }


                var item = new TopContacts();
                item.uID = selfuid;
                item.ContactID = touid;
                item.ContactDate = DateTime.Now;
                _unitOfWork.DTopContacts.Insert(item);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                return Json(
                new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
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
                });
            }
        }

        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="touid">要删除好友的id</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteToContact()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string touid = HttpContext.Current.Request.Form["touid"];
                var selfuid = userId;
                if (selfuid == null || touid == null || selfuid == "" || touid == "")
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "操作失败！联系人uid为空！",
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
                //检查这个touid是否是系统内的用户ID
                var user = _ssoUserOfWork.GetUserByID(touid);
                if (user == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "操作失败！联系人uid不合法（未查找到对应的用户）！",
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }

                //删除好友的时候，将双方都删除了
                var query =
                    _unitOfWork.DTopContacts.Get(
                        p => ((p.uID == selfuid && p.ContactID == touid) || (p.uID == touid && p.ContactID == selfuid))).FirstOrDefault();
                if (query==null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "未查找到对应的好友关系",
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
                _unitOfWork.DTopContacts.Delete(query);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                return Json(
                new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
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
                });
            }
        }


        /// <summary>
        /// 解散/退出群组（如果是群组创建人则是解散群组操作，如果是普通成员则是退出群组操作）
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupid">群组ID</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ExitSelfGroup()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                int groupid = Convert.ToInt32(HttpContext.Current.Request.Form["groupid"]);
                var strgroupid = HttpContext.Current.Request.Form["groupid"];
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！userId参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                var queryGroup = _unitOfWork.DImGroup.Get(p => p.ID == groupid).FirstOrDefault();
                if (queryGroup == null)
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！未查询到对应的群组！", Message = "操作失败", Count = 0, Total = 0 });
                }
                var queryMemberList = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == strgroupid);
                if (queryGroup.Creator == userId)
                {
                    _unitOfWork.DImGroup.Delete(queryGroup);
                    _unitOfWork.DImGroupDetail.Delete(p => p.GroupID == strgroupid);
                }
                else
                {
                    _unitOfWork.DImGroupDetail.Delete(p => p.GroupID == strgroupid && p.UserID == userId);
                }
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    var ssoUser = _ssoUserOfWork.GetUserByID(userId);
                    if (ssoUser == null)
                    {
                        return Json(new { Success = false, Content = "", Error = "操作失败！_ssoUserOfWork.GetUserByID出错！", Message = "操作失败", Count = 0, Total = 0 });
                    }
                                    
                    var msg = new SelfGroupNoticeMore
                    {
                        groupid = strgroupid,
                        groupname = queryGroup.Name,
                        membersList = queryMemberList.Select(p => p.UserID).ToList(),
                        membername = ssoUser.RealName,
                    };
                    if (queryGroup.Creator == userId)
                    {
                        //xxx解散群组 
                        msg.type = 3;
                    }
                    else
                    {
                        //xxx退出群组
                        msg.type = 2;
                    }
                    var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                    queueAddress.Send<Edu.Web.Models.SelfGroupNoticeMore>(msg);

                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 移动端获取最近联系人(最近发送聊天记录的用户) 2019年1月24日 新增
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecentlyChatUsersNew(string uid,string datetime)
        {
            /*
             * 先从缓存中取该用户所有的未读消息 然后按照fromuid分组 查看是否够5个 若不够 则再从RecentContacts表中取数据 凑够五个 若大于等于5个 则全部显示到前台界面
             */

            var userList = new List<RecentlyChatUserNew>(); //这个是最后返回的List
            try
            {
                var selfuid = uid;
                var uidList = new List<string>(); //最近聊天用户uid
                var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", selfuid);
                if (all != null && all.Any())
                {
                    all = all.OrderByDescending(p => Convert.ToDateTime(p.msgtime)).ToList();
                    /*groupby一下 可以得到有多少人发送了未读消息*/
                    var allGroupByFromuid = all.GroupBy(p => p.fromuid).ToList();
                    /*全部取出来*/
                    foreach (var item in allGroupByFromuid)
                    {
                        var user = new RecentlyChatUserNew();
                        uidList.Add(item.Key);
                        user.userid = item.Key;
                        user.unreadmsgcount = item.Count();

                        var ssouser = _ssoUserOfWork.GetUserByID(user.userid);
                        if (ssouser == null) { continue; }
                        user.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + user.userid;
                        user.username = ssouser.UserName;
                        user.realname = ssouser.RealName;
                        userList.Add(user);
                    }
                    if (userList.Count < 5)
                    {
                        var last = 5 - userList.Count;
                        var queryOther =
                            _unitOfWork.DRecentContacts.Get(p => (p.uID == selfuid || p.ContactID == selfuid))
                                .OrderByDescending(p => p.ContactDate);
                        var list0 = new List<string>();
                        foreach (var q in queryOther)
                        {
                            list0.Add(q.uID == selfuid ? q.ContactID : q.uID);
                        }
                        list0 = list0.Except(uidList).ToList().Take(last).ToList();
                        foreach (var userid in list0)
                        {
                            var user = new RecentlyChatUserNew();
                            user.userid = userid;
                            user.unreadmsgcount = 0;
                            var ssouser = _ssoUserOfWork.GetUserByID(userid);
                            if (ssouser == null) { continue; }
                            user.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + user.userid;
                            user.username = ssouser.UserName;
                            user.realname = ssouser.RealName;
                            userList.Add(user);
                        }
                    }
                }
                else
                {
                    //全部从数据库中取
                    var queryOther =
                        _unitOfWork.DRecentContacts.Get(p => (p.uID == selfuid || p.ContactID == selfuid))
                            .OrderByDescending(p => p.ContactDate).Take(5);
                    foreach (var q in queryOther)
                    {
                        var user = new RecentlyChatUserNew();
                        user.userid = (q.uID == selfuid ? q.ContactID : q.uID);
                        user.unreadmsgcount = 0;
                        var ssouser = _ssoUserOfWork.GetUserByID(user.userid);
                        if (ssouser == null) { continue; }
                        user.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + user.userid;
                        user.username = ssouser.UserName;
                        user.realname = ssouser.RealName;
                        userList.Add(user);
                    }
                }
                foreach (var item in userList)
                {
                    var query =
                        _unitOfWork.DIMMsg.Get(
                            p =>
                                ((p.FromuID == selfuid && p.TouID == item.userid) ||
                                (p.FromuID == item.userid && p.TouID == selfuid)) && p.IsDel != 1 && p.Type !="8")
                            .OrderByDescending(p => p.ID);

                    if (query.Any())
                    {
                        var msgs = query.Take(10).ToList();
                        item.msgcount = msgs.Count;
                        if (!string.IsNullOrEmpty(datetime))
                        {
                            var dt = Convert.ToDateTime(datetime);
                            var query2 = _unitOfWork.DIMMsg.Get(
                                p =>
                                    ((p.FromuID == selfuid && p.TouID == item.userid) ||
                                     (p.FromuID == item.userid && p.TouID == selfuid)) && (p.CreateDate > dt) && p.IsDel != 1 );
                            if (query2 != null && query2.Any())
                            {
                                item.msgcount = query2.Count();
                            }
                            else
                            {
                                item.msgcount = 0;
                            }
                        }
                        var lastmsg = query.FirstOrDefault();
                        var list = MsgServices.ImMsg2Msg(lastmsg);
                        item.msg = (list.msg);
                        item.msgtime = (list.msgtime);
                        item.msgtype = list.msgtype;
                        item.msglist = MsgServices.ImMsg2Msg(msgs);
                    }
                    else
                    {
                        item.msg = "";
                        item.msgtime = "";
                        item.msgtype = 0;
                        item.msglist = new List<Msg>();
                        item.msgcount = 0;
                    }

                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = userList,
                        Error = "",
                        Message = "查询成功",
                        Count = userList.Count,
                        Total = userList.Count
                    });
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = userList,
                        Error = ex,
                        Message = "",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 移动端获取所有群组(已经最近发生的聊天记录) 2019年1月24日 新增
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecentlyChatGroupsNew(string uid, string datetime)
        {
            var grouupList = new List<RecentlyChatGroupNew>(); 
            /*
             * 先从缓存中取该用户有未读聊天消息的群id
             */
            try
            {
                //1.请求所在的工作群组id集合
                var unitid = "";//GetWorkGroupIdList  这个请求中 unitid传空值也可以
                var workgroupIds = new List<string>();

                var url = ConfigHelper.GetConfigString("GetWorkGroupIdList") + "?unitID=" + unitid + "&userID=" + uid;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResult result = JsonConvert.DeserializeObject<PmcJsonResult>(resp);
                if (result != null && result.Content != null)
                    workgroupIds = result.Content;

                //2.请求当前用户所在的聊天群组的id集合（移动端去掉了多人聊天群组--2019年12月30日15:53:46）

                //var selfGroupIds = new List<string>();
                //var queryChatGroups = _unitOfWork.DImGroupDetail.Get(p => p.UserID == uid);
                //if (queryChatGroups != null && queryChatGroups.Any())
                //{
                //    selfGroupIds = queryChatGroups.Select(p=>p.GroupID).ToList();
                //}



                //3.从缓存取未读的群组消息
                var all = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid);
                if (all != null && all.Any())
                {
                    /*全部取出来*/
                    foreach (var item in all)
                    {
                        if (item.Length == 36)
                        {
                            if (workgroupIds.Contains(item))
                            {
                                //工作群
                                workgroupIds.Remove(item);
                            }
                        }
                        //else
                        //{
                        //    if (selfGroupIds.Contains(item)) {
                        //        //聊天群
                        //        selfGroupIds.Remove(item);
                        //    }else
                        //    {
                        //        continue;
                        //    }
                            
                        //}                        

                        var group = new RecentlyChatGroupNew();
                        group.msgtime = "0001/01/01 00:00:00";
                        group.groupid = item;
                        group.msglist = new List<Msg>();
                        var creator = "";
                        var creatorname = "";
                        var groupname = "";
                        //获取群组创建者、创建者姓名、群组名称
                        GetGroupCreator(item,out creator,out creatorname,out groupname);

                        if (string.IsNullOrEmpty(groupname) || string.IsNullOrEmpty(creator))
                        {
                            continue;
                        }

                        group.creatorname = creatorname;
                        group.creator = creator;
                        group.groupname = groupname;

                        //查询未读消息个数，有未读消息的群，一定有与其相关的消息记录
                        var msgUnread = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item  + uid);

                        group.unreadmsgcount = 0;
                        var query =
                            _unitOfWork.DIMMsg.Get(p => p.TouID == item && p.isgroup == 1 && p.IsDel != 1 )
                                .OrderByDescending(p => p.ID);
                        if (query.Any())
                        {
                            var msgs = query.Take(10).ToList();
                            group.msgcount = msgs.Count;
                            if (!string.IsNullOrEmpty(datetime))
                            {
                                var dt = Convert.ToDateTime(datetime);
                                var query2 =
                                    _unitOfWork.DIMMsg.Get(p => p.TouID == item && p.isgroup == 1 && p.CreateDate > dt && p.IsDel != 1 );
                                if (query2 != null && query2.Any())
                                {
                                    group.msgcount = query2.Count();
                                }
                                else
                                {
                                    group.msgcount = 0;
                                }
                            }

                            //通知类/回执类消息 查询已读和未读
                            var noticeMsg = msgs.Where(p => p.Type == "8");
                            if(noticeMsg!=null && noticeMsg.Any())
                            {
                                foreach (var noticeMsgItem in noticeMsg) {
                                    var queryNoticeMsg = _unitOfWork.DGroupNotice.Get(p => p.msgid == noticeMsgItem.ID && p.userid == uid).FirstOrDefault();
                                    if (queryNoticeMsg != null)
                                    {
                                        noticeMsgItem.Duration = 1;
                                    }
                                }                                
                            }
                            
                            var lastmsg = msgs.FirstOrDefault();
                            group.msglist = MsgServices.ImMsg2Msg(msgs);
                            group.msg = group.msglist[0].msg;
                            group.msgtime = group.msglist[0].msgtime;
                            group.msgtruename = group.msglist[0].fromrealname;
                            group.msgtype = group.msglist[0].msgtype;

                        }
                        if (msgUnread != null && msgUnread.Any())
                        {
                            msgUnread.Reverse();
                            group.unreadmsgcount = msgUnread.Count;
                            group.msg = msgUnread[0].msg;
                            group.msgtime = msgUnread[0].msgtime;
                            group.msgtruename = msgUnread[0].fromrealname;
                            group.msgtype = msgUnread[0].msgtype;
                        }
                        if (group.msglist != null && group.msglist.Any())
                        {
                            //新增，查询主题
                            foreach (var itemMsg in group.msglist)
                            {
                                if (!string.IsNullOrEmpty(itemMsg.subjectId))
                                {
                                    var subjectIdInt = Convert.ToInt32(itemMsg.subjectId);
                                    var querySubject = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt).FirstOrDefault();
                                    if (querySubject != null)
                                    {
                                        itemMsg.subjectTitle = querySubject.name;
                                    }
                                }
                            }
                        }        
                        grouupList.Add(group);
                    }

                    //if (selfGroupIds.Any())
                    //{
                    //    workgroupIds.AddRange(selfGroupIds);
                    //}

                    //剩余的工作群组和聊天群组 剩下的群组里没有未读消息，
                    foreach (var item in workgroupIds)
                    {
                        var group = new RecentlyChatGroupNew();
                        group.msgtime = "0001/01/01 00:00:00";
                        group.groupid = item;
                        var creator = "";
                        var creatorname = "";
                        var groupname = "";
                        GetGroupCreator(item, out creator, out creatorname,out groupname);
                        if (string.IsNullOrEmpty(groupname))
                        {
                            //如果群组名称为空，表示该群组已经被删除
                            continue;
                        }
                        group.creatorname = creatorname;
                        group.creator = creator;
                        group.unreadmsgcount = 0;
                        group.groupname = groupname;

                        var query =
                            _unitOfWork.DIMMsg.Get(p => p.TouID == item && p.isgroup == 1 && p.IsDel != 1 )
                                .OrderByDescending(p => p.ID);
                        if (query.Any())
                        {
                            var msgs = query.Take(10).ToList();
                            group.msgcount = msgs.Count;
                            if (!string.IsNullOrEmpty(datetime))
                            {
                                var dt = Convert.ToDateTime(datetime);
                                var query2 =
                                    _unitOfWork.DIMMsg.Get(
                                        p => p.TouID == item && p.isgroup == 1 && p.CreateDate > dt && p.IsDel != 1 );
                                if (query2 != null && query2.Any())
                                {
                                    group.msgcount = query2.Count();
                                }
                                else
                                {
                                    group.msgcount = 0;
                                }
                            }

                            //通知类/回执类消息 查询已读和未读
                            var noticeMsg = msgs.Where(p => p.Type == "8");
                            if (noticeMsg != null && noticeMsg.Any())
                            {
                                foreach (var noticeMsgItem in noticeMsg)
                                {
                                    var queryNoticeMsg = _unitOfWork.DGroupNotice.Get(p => p.msgid == noticeMsgItem.ID && p.userid == uid).FirstOrDefault();
                                    if (queryNoticeMsg != null)
                                    {
                                        //标记为已读
                                        noticeMsgItem.Duration = 1;
                                    }
                                }
                            }

                            var lastmsg = msgs.FirstOrDefault();
                            group.msglist = MsgServices.ImMsg2Msg(msgs);

                            group.msg = group.msglist[0].msg;
                            group.msgtime = group.msglist[0].msgtime;
                            group.msgtruename = group.msglist[0].fromrealname;
                            group.msgtype = group.msglist[0].msgtype;
                        }

                        if(group.msglist!=null && group.msglist.Any())
                        {
                            //新增，查询主题
                            foreach (var itemMsg in group.msglist)
                            {
                                if (!string.IsNullOrEmpty(itemMsg.subjectId))
                                {
                                    var subjectIdInt = Convert.ToInt32(itemMsg.subjectId);
                                    var querySubject = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt).FirstOrDefault();
                                    if (querySubject != null)
                                    {
                                        itemMsg.subjectTitle = querySubject.name;
                                    }
                                }
                            }
                        }

                        

                        grouupList.Add(group);
                    }


                    grouupList=grouupList.OrderByDescending(p => Convert.ToDateTime(p.msgtime)).ToList();


                    //以下为2019年12月19日 新增，目的是查询grouupList中的msglist中是否有回执类消息，并且查询是否已读、未读
                    //但是加上这个后，查询也变得十分慢了


                    //foreach (var item in grouupList)
                    //{
                    //    if (item.msglist == null)
                    //        continue;
                    //    foreach (var msg in item.msglist)
                    //    {
                    //        if (msg.msgtype == 8)
                    //        {
                    //            var idInt = Convert.ToInt32(msg.id0);

                    //            var query = _unitOfWork.DGroupNotice.Get(p => p.msgid == idInt && p.userid == uid).FirstOrDefault();
                    //            if (query != null)
                    //            {
                    //                msg.duration = 1;
                    //            }
                    //        }
                    //    }
                    //}



                    return Json(
                        new
                        {
                            Success = true,
                            Content = grouupList,
                            Error = "",
                            Message = "查询成功",
                            Count = grouupList.Count,
                            Total = grouupList.Count
                        });
                }
            }
            catch (Exception ex)
            {
                LoggerHelper.Error($"GetRecentlyChatGroupsNew", ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
            return Json(
                   new
                   {
                       Success = true,
                       Content = "",
                       Error = "",
                       Message = "查询成功",
                       Count = 0,
                       Total = 0
                   });
        }



        /// <summary>
        /// 获取用户所在工作群组的所有未读消息，如果没有未读消息，那么返回一条最近发生聊天记录的群组聊天消息数据
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecentlyChatGroupSingle(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "uid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                var list = new List<RecentlyChatGroupSingle>() { };//最后返回的数据集合
                var workgroupIds = new List<string>();
                var unitid = "";
                //1.请求所在的工作群组id集合
                var url = ConfigHelper.GetConfigString("GetWorkGroupIdList") + "?unitID=" + unitid + "&userID=" + uid;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResult result = JsonConvert.DeserializeObject<PmcJsonResult>(resp);
                if (result != null && result.Content != null)
                    workgroupIds = result.Content;
                if (!workgroupIds.Any())
                {
                    return Json(
                          new EduJsonResult
                          {
                              Success = true,
                              Content = null,
                              Error = "",
                              Message = "查询成功,但是您并未加入任何工作群",
                              Count = 0,
                              Total = 0
                          });
                }
                //2.查询缓存中有未读聊天记录的群组的id的集合，然后和workgroupIds相比较，排除其中的聊天群组(自建群组)
                var all = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid);
                if (all != null && all.Any())
                {
                    /*全部取出来*/
                    foreach (var item in all)
                    {
                        if (workgroupIds.Contains(item))
                        {
                            var group = new RecentlyChatGroupSingle();
                            //查询未读消息个数，有未读消息的群，一定有与其相关的消息记录
                            var msgUnread = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item + uid);
                            if (msgUnread != null && msgUnread.Any())
                            {
                                group.groupid = item;
                                msgUnread.Reverse();
                                var msg = msgUnread[0]; //取最新的消息  如果有未读消息，那么就不去取最新的消息了，而是获取所有未读消息
                                group.unreadmsgcount = msgUnread.Count;
                                //group.msg = msg.msg;
                                group.msgtime = msg.msgtime;
                                //group.msgtruename = msg.fromrealname;
                                //group.msgtype = msg.msgtype;
                                group.groupicon = GetGroupIcon(item);
                                //group.groupname = msg.torealname;
                                group.msglist = msgUnread;
                                list.Add(group);
                            }
                        }
                    }
                    if (list.Any())
                    {
                        list=list.OrderByDescending(p => Convert.ToDateTime(p.msgtime)).ToList();
                        return Json(
                            new
                            {
                                Success = true,
                                Content = list,
                                Error = "",
                                Message = "查询成功",
                                Count = list.Count,
                                Total = list.Count
                            });
                    }
                }
                //如果没有未读的消息，则返回一条最新的群组聊天消息
                var queryMessage = _unitOfWork.DIMMsg.Get(p => workgroupIds.Contains(p.TouID) && p.IsDel!=1).OrderByDescending(p => p.ID).FirstOrDefault();
                if (queryMessage != null)
                {
                    var item = new RecentlyChatGroupSingle();
                    var kmsg = MsgServices.ImMsg2Msg(queryMessage);
                    item.groupid = queryMessage.TouID;
                    item.groupname = queryMessage.tousername;
                    item.msg = kmsg.msg;
                    item.msgtime = kmsg.msgtime;
                    item.msgtruename = kmsg.fromrealname;
                    item.msgtype = kmsg.msgtype;
                    item.groupicon = GetGroupIcon(item.groupid);
                    item.unreadmsgcount = 0;
                    list.Add(item);
                    return Json(
                        new
                        {
                            Success = true,
                            Content = list,
                            Error = "",
                            Message = "查询成功",
                            Count = 1,
                            Total = 1
                        });
                }




                return Json(
                       new EduJsonResult
                       {
                           Success = true,
                           Content = null,
                           Error = "",
                           Message = "查询成功,并无最近聊天群组数据",
                           Count = 0,
                           Total = 0
                       });
            }
            catch (Exception ex)
            {
                return Json(
                    new EduJsonResult
                    {
                        Success = false,
                        Content = null,
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 获取和某一个对象之间的未读的消息
        /// </summary>
        [HttpGet]
        public IHttpActionResult GetUnreadMessages(string uid,string touid,int isgroup)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "uid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                if (string.IsNullOrEmpty(touid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "touid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                if(isgroup != 0 && isgroup != 1)
                {
                    return Json(
                          new EduJsonResult
                          {
                              Success = false,
                              Content = null,
                              Error = "isgroup参数错误",
                              Message = "查询失败",
                              Count = 0,
                              Total = 0
                          });
                }
                var result = new List<Msg>() { };
                if (isgroup == 0)
                {
                    //------单人消息--start------
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", uid);
                    if (all != null && all.Any())
                    {
                        var queryUnreadMsg = all.Where(p => p.fromuid == touid);
                        if (queryUnreadMsg != null && queryUnreadMsg.Any())
                        {
                            result = queryUnreadMsg.ToList();
                        }
                        if (result.Any())
                        {
                            Task.Run(() =>
                            {
                                //异步执行
                                //从缓存中删除这些消息
                                foreach (var msg in result)
                                {
                                    all.Remove(msg);
                                }
                                RedisHelper.Hash_Remove("IMMsg", uid);
                                MsgServices.ResetRedisKeyValue<Msg>("IMMsg", uid, all);
                            });
                        }                        
                    }
                    //------单人消息--start------
                }
                if (isgroup == 1)
                {
                    //------群组消息--start------
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid + uid);
                    if (all != null && all.Any())
                    {
                        Task.Run(() =>
                        {
                            //异步执行
                            //从缓存中删除这些消息
                            RedisHelper.Hash_Remove("IMGroupMsg", touid + uid);
                        });
                        result = all;
                    }
                    //------群组消息--end------
                }



                return Json(
                       new EduJsonResult
                       {
                           Success = true,
                           Content = result,
                           Error = "",
                           Message = "查询成功",
                           Count = result.Count,
                           Total = result.Count
                       });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return Json(
                    new EduJsonResult
                    {
                        Success = false,
                        Content = null,
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }





        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetMissedMessages(string uid,string touid,string lastmsgid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "uid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                if (string.IsNullOrEmpty(touid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "touid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }

                if (string.IsNullOrEmpty(lastmsgid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "lastmsgid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }

                var lastmsgidInt = Convert.ToInt32(lastmsgid);

                var list = new List<Msg>() { };//最后返回的数据集合
                var query=_unitOfWork.DIMMsg.Get(p => ((p.FromuID == uid && p.TouID == touid && p.isgroup == 0) || (p.FromuID == touid && p.TouID == uid && p.isgroup == 0) || (p.TouID == touid && p.isgroup == 1)) && p.IsDel != 1 && (p.ID > lastmsgidInt)).OrderBy(p => p.ID);
                if (query != null && query.Any()) {

                    //1.群组清除redis中的未读消息(个人和群组的未读消息都要清除)
                    var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid) ?? new List<String>();
                    if (unreadgroupmsg.Any() && unreadgroupmsg.Contains(touid))
                    {
                        unreadgroupmsg.Remove(touid);
                        RedisHelper.Hash_Remove("IMUnreadGroupMsg", uid);
                        RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", uid, unreadgroupmsg);
                    }
                    var unreadmsg = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid + uid);
                    if (unreadmsg != null && unreadmsg.Any())
                    {
                        RedisHelper.Hash_Remove("IMGroupMsg", touid + uid);
                    }

                    //2.个人清除redis中的未读消息(个人和群组的未读消息都要清除)
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", uid);
                    if (all != null && all.Any())
                    {
                        var queryUnreadMsg =
                      all.Where(p => p.fromuid == touid);
                        var unreadMsg = queryUnreadMsg as Msg[] ?? queryUnreadMsg.ToArray();
                        if (unreadMsg.Any())
                        {
                            foreach (var msg in unreadMsg)
                            {
                                all.Remove(msg);
                            }
                            RedisHelper.Hash_Remove("IMMsg", uid);
                            MsgServices.ResetRedisKeyValue<Msg>("IMMsg", uid, all);
                        }
                    }
                    list = MsgServices.ImMsg2Msg(query.ToList());
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = list.Count,
                    });           
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("GetMissedMessages", ex);
                return Json(
                    new EduJsonResult
                    {
                        Success = false,
                        Content = null,
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }




        [HttpGet]
        public IHttpActionResult GetChatGroupOverview(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(
                       new EduJsonResult
                       {
                           Success = false,
                           Content = null,
                           Error = "uid不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                var list = new List<RecentlyChatGroupSingle>() { };//最后返回的数据集合
                var workgroupIds = new List<string>();
                var unitid = "";
                //1.请求所在的工作群组id集合
                var url = ConfigHelper.GetConfigString("GetWorkGroupIdList") + "?unitID=" + unitid + "&userID=" + uid;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResult result = JsonConvert.DeserializeObject<PmcJsonResult>(resp);
                if (result != null && result.Content != null)
                    workgroupIds = result.Content;
                //2.获取所在聊天群组id集合
                //var chatgroupIds = new List<string>();
                //var queryChatGroups= _unitOfWork.DImGroupDetail.Get(p => p.UserID == uid);
                //if (queryChatGroups.Any())
                //{
                //    chatgroupIds = queryChatGroups.Select(p=>p.GroupID).ToList();
                //}
                //if (!workgroupIds.Any()&&!chatgroupIds.Any())
                if (!workgroupIds.Any())
                {
                    return Json(
                          new EduJsonResult
                          {
                              Success = true,
                              Content = null,
                              Error = "",
                              Message = "查询成功,但是您并未加入任何群组",
                              Count = 0,
                              Total = 0
                          });
                }
                //workgroupIds.AddRange(chatgroupIds);
                var otherId = workgroupIds;
                //2.查询缓存中有未读聊天记录的群组的id的集合，然后和workgroupIds相比较，排除其中的聊天群组(自建群组)
                var all = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid);
                if (all != null && all.Any())
                {
                    otherId = workgroupIds.Except(all).ToList();
                    foreach (var item in all)
                    {
                        if (workgroupIds.Contains(item))
                        {
                            var group = new RecentlyChatGroupSingle();
                            //查询未读消息个数，有未读消息的群，一定有与其相关的消息记录
                            var msgUnread = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", item + uid);
                            if (msgUnread != null && msgUnread.Any())
                            {
                                group.groupid = item;
                                msgUnread.Reverse();
                                var msg = msgUnread[0]; //取最新的消息  如果有未读消息，取最新的消息
                                group.unreadmsgcount = msgUnread.Count;
                                group.msg = msg.msg;
                                group.msgtime = msg.msgtime;
                                group.msgtruename = msg.fromrealname;
                                group.msgtype = msg.msgtype;
                                group.groupicon = GetGroupIcon(item);
                                group.groupname = msg.torealname;
                                group.msglist = msgUnread;
                                list.Add(group);
                            }
                            else
                            {
                                //去数据库中查询最近的一条聊天记录
                                var immsg = GetMostRecentlyChatMessage(item, uid);
                                if (immsg != null)
                                {
                                    var model = new RecentlyChatGroupSingle();
                                    var kmsg = MsgServices.ImMsg2Msg(immsg);
                                    model.groupid = immsg.TouID;
                                    model.groupname = immsg.tousername;
                                    model.msg = kmsg.msg;
                                    model.msgtime = kmsg.msgtime;
                                    model.msgtruename = kmsg.fromrealname;
                                    model.msgtype = kmsg.msgtype;
                                    model.groupicon = GetGroupIcon(item);
                                    model.unreadmsgcount = 0;
                                    list.Add(model);
                                }
                            }
                        }
                    }
                }
                //3.除去未读消息的群组，查询其他群组，这里的群组，只取一条最新消息即可
                
                foreach (var id in otherId)
                {
                    var immsg = GetMostRecentlyChatMessage(id, uid);
                    if (immsg != null)
                    {
                        var model = new RecentlyChatGroupSingle();
                        var kmsg = MsgServices.ImMsg2Msg(immsg);
                        model.groupid = immsg.TouID;
                        model.groupname = immsg.tousername;
                        model.msg = kmsg.msg;
                        model.msgtime = kmsg.msgtime;
                        model.msgtruename = kmsg.fromrealname;
                        model.msgtype = kmsg.msgtype;
                        model.groupicon = GetGroupIcon(id);
                        model.unreadmsgcount = 0;
                        list.Add(model);
                    }
                }

                if (list.Any())
                {
                    list = list.OrderByDescending(p => Convert.ToDateTime(p.msgtime)).ToList();
                    return Json(
                        new
                        {
                            Success = true,
                            Content = list,
                            Error = "",
                            Message = "查询成功",
                            Count = list.Count,
                            Total = list.Count
                        });
                }
                return Json(
                       new EduJsonResult
                       {
                           Success = true,
                           Content = null,
                           Error = "",
                           Message = "查询成功,并无最近聊天群组数据",
                           Count = 0,
                           Total = 0
                       });
            }
            catch (Exception ex)
            {
                return Json(
                    new EduJsonResult
                    {
                        Success = false,
                        Content = null,
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 根据uid和群组id获取从数据库中获取最新的一条聊天记录，如果没有，则返回为null
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        private IMMsg GetMostRecentlyChatMessage(string groupid, string uid)
        {
            var queryMessage0 = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid && p.IsDel != 1 ).OrderByDescending(p => p.ID).FirstOrDefault();
            if (queryMessage0!=null)
            {
                return queryMessage0;
            }

            return null;
        }

        /// <summary>
        /// 根据groupid 返回其Icon地址
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        private string GetGroupIcon(string groupid)
        {
            if (groupid.Length == 36)
            {
                return ConfigHelper.GetConfigString("WorkGroupPic") + groupid;
            }
            else
            {
                return ConfigHelper.GetConfigString("IMWebApiGroupPic") + groupid;
            }
        }

        private void GetGroupCreator(string groupid,out string creator, out string creatorname,out string groupname)
        {
            creator = "";
            creatorname = "";
            groupname = "";
            if (groupid.Length == 36)
            {
                var url = ConfigHelper.GetConfigString("GetGroupInfoByGroupId") + "?groupId=" + groupid;
                var resp = HttpWebHelper.HttpGet(url);
                var groupInfo = new GroupInfo2();

                var urlResult = JsonConvert.DeserializeObject<PmcGetGroupInfoJsonResult2>(resp);
                if (urlResult != null && urlResult.Content != null)
                {
                    groupInfo = urlResult.Content;
                    creator = groupInfo.CreateUserID;
                    creatorname = groupInfo.RealName;
                    groupname = groupInfo.Name;
                }
            }
            else
            {
                var goupidInt = Convert.ToInt32(groupid);
                var query = _unitOfWork.DImGroup.Get(p => p.ID == goupidInt).FirstOrDefault();
                if (query != null)
                {
                    creator = query.Creator;
                    var ssoUser = _ssoUserOfWork.GetUserByID(query.Creator);
                    if (ssoUser != null)
                    {
                        creatorname = ssoUser.RealName;
                        groupname = query.Name;
                    }
                    
                }
            }
        }




        /// <summary>
        /// 获取单人聊天的聊天历史记录
        /// </summary>
        /// <param name="uid0">发送人</param>
        /// <param name="uid1">接收人</param>
        /// <param name="datetime">时间（yyyy-MM-dd hh:mm:ss）精确到秒</param>
        /// <param name="keywords">查询关键字</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPersonalChatHistory(string uid0, string uid1, string keywords,int pageNo,int pageSize,string datetime)
        {
            string fromuid = uid0;
            string touid = uid1;
            if (string.IsNullOrEmpty(fromuid)||string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!uid0或者uid1不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(datetime))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "时间参数为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }

            //带关键字的查询 暂时没有用到模糊查询
            if (!string.IsNullOrEmpty(keywords))
            {
                return GetPersonalChatHistoryByKeywords(fromuid, touid, keywords, pageNo, pageSize);
            }
            try
            {
                var dt = Convert.ToDateTime(datetime);
                //var dt2 = dt.AddDays(1);
                /*var query = _unitOfWork.DIMMsg.Get(p =>
                           ((p.FromuID == fromuid && p.TouID == touid) || p.FromuID == touid && p.TouID == fromuid) &&
                           (p.CreateDate.Value >= dt && p.CreateDate.Value <= dt2))
                         .OrderBy(p => p.CreateDate);*/
                var query = _unitOfWork.DIMMsg.Get(p =>
                           ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.IsDel!=1 && p.CreateDate<dt)
                         .OrderByDescending(p => p.ID);
                var totalCount = query.Count();
                var list = new List<Msg>();
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var imMsg in queryPage)
                    {
                        var msg = MsgServices.ImMsg2Msg(imMsg);
                        list.Add(msg);
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功",
                    Count = list.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }


        /// <summary>
        /// 查询个人（人与人）聊天记录 根据关键字查询聊天记录
        /// </summary>
        /// <param name="uid0">发送人</param>
        /// <param name="uid1">接收人</param>
        /// <param name="keywords">查询关键字</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据个数</param>
        /// <returns></returns>
        public IHttpActionResult GetPersonalChatHistoryByKeywords(string uid0, string uid1, string keywords, int pageNo, int pageSize)
        {
            string fromuid = uid0;
            string touid = uid1;
            if (string.IsNullOrEmpty(fromuid) || string.IsNullOrEmpty(touid)||string.IsNullOrEmpty(keywords))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!uid0或者uid1或者keywords为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                var query = _unitOfWork.DIMMsg.Get(p =>
                          ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.IsDel!=1 && p.Msg.Contains(keywords))
                        .OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var list = new List<Msg>();
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var imMsg in queryPage)
                    {
                        var msglist = MsgServices.ImMsg2Msg(imMsg);
                        list.Add(msglist);
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功",
                    Count = list.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }

        /// <summary>
        /// 获取群组内的聊天历史记录(已弃用）
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="datetime"></param>
        /// <param name="keywords"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupChatHistory(string groupid, string keywords, int pageNo, int pageSize, string datetime)
        {
            if (string.IsNullOrEmpty(groupid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!群组id不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(datetime))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "时间参数为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                //带关键字的查询
                if (!string.IsNullOrEmpty(keywords))
                {
                    return GetGroupChatHistoryByKeyWords(groupid, keywords, pageNo, pageSize);
                }
                var dt = Convert.ToDateTime(datetime);
        
                var query = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid && p.IsDel != 1  && p.CreateDate<dt).OrderByDescending(p => p.ID);
                var totalCount = query.Count();
                var list = new List<Msg>();
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var imMsg in queryPage)
                    {
                        if (imMsg.Type == "8")
                        {
                            var queryRead = _unitOfWork.DGroupNotice.Get(p => p.msgid == imMsg.ID).FirstOrDefault();
                            if (queryRead != null)
                            {
                                imMsg.Duration = 1;
                            }
                        }

                        var msg = MsgServices.ImMsg2Msg(imMsg);
                        list.Add(msg);
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功",
                    Count = list.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }


        /// <summary>
        /// 获取群组内的聊天历史记录(新增的，新加了一个参数 uid)
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="datetime"></param>
        /// <param name="keywords"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupChatHistory(string groupid, string keywords, int pageNo, int pageSize, string datetime,string useId)
        {
            if (string.IsNullOrEmpty(groupid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!群组id不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(datetime))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "时间参数为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                //带关键字的查询
                if (!string.IsNullOrEmpty(keywords))
                {
                    return GetGroupChatHistoryByKeyWords(groupid, keywords, pageNo, pageSize);
                }
                var dt = Convert.ToDateTime(datetime);

                var query = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid && p.IsDel != 1 && p.CreateDate < dt).OrderByDescending(p => p.ID);
                var totalCount = query.Count();
                var list = new List<Msg>();
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var imMsg in queryPage)
                    {
                        if (imMsg.Type == "8")
                        {
                            var queryRead = _unitOfWork.DGroupNotice.Get(p => p.msgid == imMsg.ID && p.userid== useId).FirstOrDefault();
                            if (queryRead != null)
                            {
                                imMsg.Duration = 1;
                            }
                        }

                        var msg = MsgServices.ImMsg2Msg(imMsg);

                        //新增，查询主题
                        if (!string.IsNullOrEmpty(msg.subjectId))
                        {
                            var subjectIdInt = Convert.ToInt32(msg.subjectId);
                            var querySubject = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt).FirstOrDefault();
                            if (querySubject != null)
                            {
                                msg.subjectTitle = querySubject.name;
                            }
                        }

                        list.Add(msg);
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功",
                    Count = list.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }

        /// <summary>
        /// 获取聊天历史记录（我与xxx的单人聊天记录、我所在的xxx群组聊天记录)(根据传入的时间范围获取）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="touid"></param>
        /// <param name="dt0"></param>
        /// <param name="dt1"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetChatHistory(string uid, string touid,string dt0,string dt1)
        {
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!用户id不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!聊天对象id不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(dt0))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!开始时间不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(dt1))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!结束时间不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                var startDatetime = Convert.ToDateTime(dt0);
                var endDatetime = Convert.ToDateTime(dt1);

                if (startDatetime > endDatetime)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "操作失败!查询开始时间不能大于查询结束时间！",
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
                }
                
                var query = _unitOfWork.DIMMsg.Get(p =>((p.FromuID == uid && p.TouID == touid && p.isgroup == 0) || (p.FromuID == touid && p.TouID == uid && p.isgroup == 0) || (p.TouID == touid && p.isgroup==1)) && p.IsDel != 1 && (p.CreateDate <= endDatetime&& p.CreateDate>=startDatetime)).OrderBy(p => p.ID);
                var totalCount = query.Count();
                var list = new List<Msg>();
                if (query.Any())
                {
                    foreach (var imMsg in query)
                    {
                        var msg = MsgServices.ImMsg2Msg(imMsg);
                        list.Add(msg);
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功",
                    Count = totalCount,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }




        /// <summary>
        /// 查询群组（群组内）聊天记录 根据关键字查询聊天记录
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="keywords"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public IHttpActionResult GetGroupChatHistoryByKeyWords(string groupid, string keywords, int pageNo, int pageSize)
        {
            if (string.IsNullOrEmpty(groupid)|| string.IsNullOrEmpty(keywords))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!群组id不能为空,查询关键字不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                var query = _unitOfWork.DIMMsg.Get(p => (p.TouID == groupid) && p.IsDel != 1  && p.Msg.Contains(keywords)).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var list = new List<Msg>();
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var imMsg in queryPage)
                    {
                        if (imMsg.Type == "8")
                        {
                            var queryRead = _unitOfWork.DGroupNotice.Get(p => p.msgid == imMsg.ID).FirstOrDefault();
                            if (queryRead != null)
                            {
                                imMsg.Duration = 1;
                            }
                        }
                        var msg = MsgServices.ImMsg2Msg(imMsg);
                        list.Add(msg);
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功",
                    Count = list.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }

        }

        /// <summary>
        /// 查询聊天的图片记录（单人聊天记录-移动端滑动查看图片所用）
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetChatImageHistory(string fromuid, string touid, int pageNo, int pageSize,
            string datetime)
        {
            if (string.IsNullOrEmpty(fromuid) || string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!fromuid或者touid不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(datetime))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "时间参数不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                var dt = Convert.ToDateTime(datetime);
                var query = _unitOfWork.DIMMsg.Get(p =>
                           ((p.Type == "1" || p.Type == "3") && ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid))) && p.IsDel != 1  && p.CreateDate < dt)
                         .OrderByDescending(p => p.ID);
                var totalCount = 0;
                var list = new List<Msg>();
                var resultList= new List<Msg>();
                if (query.Any())
                {
                    totalCount = query.Count();
                    list = MsgServices.ImMsg2Msg(query.ToList());
                    resultList = list.AsEnumerable().Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();

                }
                return Json(new
                {
                    Success = true,
                    Content = resultList,
                    Error = "",
                    Message = "查询成功",
                    Count = resultList.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }

        /// <summary>
        /// 查询聊天的图片记录（群组聊天记录-移动端滑动查看图片所用）
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupChatImageHistory(string groupid, int pageNo, int pageSize,
            string datetime)
        {
            if (string.IsNullOrEmpty(groupid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "操作失败!groupid不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(datetime))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "时间参数不能为空！",
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                var dt = Convert.ToDateTime(datetime);
                var query = _unitOfWork.DIMMsg.Get(p =>
                           ((p.Type == "1" || p.Type == "3")) && p.TouID == groupid && p.IsDel != 1  && p.CreateDate < dt)
                         .OrderByDescending(p => p.ID);
                var totalCount = 0;
                var list = new List<Msg>();
                var resultList = new List<Msg>();
                if (query.Any())
                {
                    totalCount = query.Count();
                    list = MsgServices.ImMsg2Msg(query.ToList());
                    resultList = list.AsEnumerable().Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                }
                return Json(new
                {
                    Success = true,
                    Content = resultList,
                    Error = "",
                    Message = "查询成功",
                    Count = resultList.Count,
                    Total = totalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = ex.ToString(),
                    Message = "查询失败",
                    Count = 0,
                    Total = 0
                });
            }
        }
 
        
        /// <summary>
        /// 退出登录后，将数据库中在线标记记录为0
        /// </summary>
        /// <param name="uid">当前用户ID</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeviceUserOffline()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                string deviceid = HttpContext.Current.Request.Form["deviceid"];
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！用户ID参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(deviceid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！DeviceID参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                var query = _unitOfWork.DUserDevice.Get(p => p.uid == uid && p.deviceid==deviceid).FirstOrDefault();
                if (query != null)
                {
                    _unitOfWork.DUserDevice.Delete(query);
                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                    }
                    return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
                }
                else
                {
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "操作失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 获取最近联系人，移动端转发消息的时候使用
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetForwardUsers(string uid,int pageNo = 1, int pageSize=10)
        {
            /*
             * 先从缓存中取该用户所有的未读消息 然后按照fromuid分组 查看是否够5个 若不够 则再从RecentContacts表中取数据 凑够五个 若大于等于5个 则全部显示到前台界面
             */
            try
            {
                var selfuid = uid;
                var userList = new List<ForwardUser>(); //这个是最后返回的List
                var queryAll =
                    _unitOfWork.DIMMsg.Get(p => (p.FromuID == selfuid || p.TouID == selfuid));
                var idlist= new List<string>();
                if (queryAll != null && queryAll.Any())
                {
                    var query0 = queryAll.GroupBy(p => p.FromuID);
                    var query1 = queryAll.GroupBy(p => p.TouID);
                    foreach (var item in query0)
                    {
                        idlist.Add(item.Key);
                    }
                    foreach (var item in query1)
                    {
                        idlist.Add(item.Key);
                    }
                    //去重
                    idlist = idlist.Distinct().ToList();
                    //去掉自己
                    idlist.Remove(uid);
                    foreach (var item in idlist)
                    {
                        var forwardUser = new ForwardUser();
                        //这个queryK一定不会为空
                        var queryK =
                            _unitOfWork.DIMMsg.Get(
                                p =>
                                    (p.FromuID == item && p.TouID == uid) || (p.FromuID == uid && p.TouID == item))
                                .OrderByDescending(p => p.ID)
                                .FirstOrDefault();
                        forwardUser.isgroup = queryK.isgroup;
                        forwardUser.id = queryK.CreateUser;
                        forwardUser.realname = queryK.fromusername;
                        forwardUser.msgtime = queryK.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                        if (forwardUser.id == uid)
                        {
                            forwardUser.id = queryK.TouID;
                            forwardUser.realname = queryK.tousername;
                        }
                        userList.Add(forwardUser);
                    }
                    //排序
                    userList = userList.OrderByDescending(p => Convert.ToDateTime(p.msgtime)).ToList();

                    if (pageNo == 0 && pageSize == 0)
                    {

                    }
                    else
                    {
                        userList = userList.Skip((pageSize - 1)*pageSize).Take(pageSize).ToList();
                    }

                    return Json(
                        new
                        {
                            Success = true,
                            Content = userList,
                            Error = "",
                            Message = "查询成功",
                            Count = userList.Count,
                            Total = userList.Count
                        });
                }
                return Json(
                        new
                        {
                            Success = true,
                            Content = userList,
                            Error = "",
                            Message = "查询成功，但是列表为空",
                            Count = userList.Count,
                            Total = userList.Count
                        });

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
                    });
            }
        }

        [HttpGet]
        public IHttpActionResult CheckFileExistByMd5(string md5, string filename)
        {
            try
            {
                if (string.IsNullOrEmpty(md5))
                {
                    return Json(
                       new
                       {
                           Success = false,
                           Content = "",
                           Error = "MD5字符串不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                if (string.IsNullOrEmpty(filename))
                {
                    return Json(
                       new
                       {
                           Success = false,
                           Content = "",
                           Error = "文件名不能为空",
                           Message = "查询失败",
                           Count = 0,
                           Total = 0
                       });
                }
                var fileCode = FileService.GetFileCodeByMD5(md5);
                var fileExists = !string.IsNullOrEmpty(fileCode);
                if (fileExists)
                {
                    var fileurl = ConfigHelper.GetConfigString("HfsDownLoadUrl")+"title="+ filename+ "&fileCode="+fileCode;
                    return Json(
                       new
                       {
                           Success = true,
                           Content = fileurl,
                           Error = "",
                           Message = "查询成功",
                           Count = 0,
                           Total = 0
                       });
                }
                return Json(
                       new
                       {
                           Success = false,
                           Content = "",
                           Error = "文件不存在",
                           Message = "查询成功，但是文件不存在，需要重新上传",
                           Count = 0,
                           Total = 0
                       });

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
                    });
            }
        }


        /// <summary>
        /// 删除我发送的文件
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteMyFile()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                string fileid = HttpContext.Current.Request.Form["fileid"];
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(
                           new
                           {
                               Success = false,
                               Content = "",
                               Error = "uid不能为空",
                               Message = "操作失败",
                               Count = 0,
                               Total = 0
                           });
                }
                if (string.IsNullOrEmpty(fileid))
                {
                    return Json(
                           new
                           {
                               Success = false,
                               Content = "",
                               Error = "fileid不能为空",
                               Message = "操作失败",
                               Count = 0,
                               Total = 0
                           });
                }

                var idInt = Convert.ToInt32(fileid);
                var queryMsg = _unitOfWork.DIMMsg.Get(p => p.ID == idInt && p.CreateUser == uid && p.IsDel != 1 ).FirstOrDefault();
                if (queryMsg != null)
                {
                    queryMsg.IsDel = 1;
                    //下载链接置空
                    queryMsg.FileUrl = "";
                    _unitOfWork.DIMMsg.Update(queryMsg);
                    var sqlResult = _unitOfWork.Save();
                    if (sqlResult.ResultType == OperationResultType.Success)
                    {
                        return Json(new
                        {
                            Success = true,
                            Content = "",
                            Error = "",
                            Message = "操作成功",
                            Count = 0,
                            Total = 0
                        });
                    }
                    return Json(
                        new
                        {
                            Success = false,
                            Content = sqlResult.Message,
                            Error = "",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });
                }
                return Json(
                        new
                        {
                            Success = false,
                            Content = "",
                            Error = "未查询到对应的数据",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });
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
                    });
            }
        }
        
        /// <summary>
        /// 获取我发送的文件（群组+个人）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetSendFiles(string uid, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var list = new List<ContactFile>();
                /*
                     * 1.查询我所在的工作群组 发送get请求
                     * 2.查询我所在的聊天群组 查询本地数据库
                     * 3.查询聊天记录中的文件记录 查询本地数据库
                     */

                var queryFiles = _unitOfWork.DIMMsg.Get(p => p.Type == "2" && p.FromuID==uid && p.IsDel != 1 ).OrderByDescending(p => p.ID);
                if (queryFiles != null && queryFiles.Any())
                {
                    foreach (var queryFile in queryFiles)
                    {
                        list.Add(Immsg2ContactFile(queryFile));
                    }
                }
                if (pageSize == 0 && pageNo == 0)
                {
                    return Json(
                       new
                       {
                           Success = true,
                           Content = list,
                           Error = "",
                           Message = "查询成功",
                           Count = list.Count,
                           Total = list.Count
                       });
                }
                var listResult = list.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                return Json(
                       new
                       {
                           Success = true,
                           Content = listResult,
                           Error = "",
                           Message = "查询成功",
                           Count = listResult.Count,
                           Total = list.Count
                       });

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
                    });
            }
        }

        /// <summary>
        /// 获取我收到的文件列表 （群组+个人）
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetReceiveFiles(string uid, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var workgroupIds = new List<string>();
                var chatgroupIds = new List<string>();
                var groupIds = new List<string>();
                var list = new List<ContactFile>();
                /*
                     * 1.查询我所在的工作群组 发送get请求
                     * 2.查询我所在的聊天群组 查询本地数据库
                     * 3.查询聊天记录中的文件记录 查询本地数据库
                     */
                var unitid = "";

                var url = ConfigHelper.GetConfigString("GetWorkGroupIdList") + "?unitID=" + unitid + "&userID=" + uid;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResult result = JsonConvert.DeserializeObject<PmcJsonResult>(resp);
                if (result != null && result.Content != null)
                    workgroupIds = result.Content;

                //var queryChatGroups = _unitOfWork.DImGroupDetail.Get(p => p.UserID == uid);
                //if (queryChatGroups != null && queryChatGroups.Any())
                //{
                //    foreach (var queryChatGroup in queryChatGroups)
                //    {
                //        chatgroupIds.Add(queryChatGroup.GroupID);
                //    }
                //}
                groupIds.AddRange(workgroupIds);
                //groupIds.AddRange(chatgroupIds);
                var queryFiles = _unitOfWork.DIMMsg.Get(p => p.Type == "2" && groupIds.Contains(p.TouID) && p.IsDel != 1 ).OrderByDescending(p => p.ID);
                if (queryFiles != null && queryFiles.Any())
                {
                    foreach (var queryFile in queryFiles)
                    {
                        list.Add(Immsg2ContactFile(queryFile));
                    }
                }
                var queryFilesPersonal = _unitOfWork.DIMMsg.Get(p => p.Type == "2" && p.TouID==(uid) && p.IsDel != 1 ).OrderByDescending(p => p.ID);

                if (queryFilesPersonal != null && queryFilesPersonal.Any())
                {
                    foreach (var queryFile in queryFilesPersonal)
                    {
                        list.Add(Immsg2ContactFile(queryFile));
                    }
                }

                if (pageSize == 0 && pageNo == 0)
                {
                    return Json(
                       new
                       {
                           Success = true,
                           Content = list,
                           Error = "",
                           Message = "查询成功",
                           Count = list.Count,
                           Total = list.Count
                       });
                }
                var listResult = list.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                return Json(
                       new
                       {
                           Success = true,
                           Content = listResult,
                           Error = "",
                           Message = "查询成功",
                           Count = listResult.Count,
                           Total = list.Count
                       });

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
                    });
            }
        }
        

        /// <summary>
        /// 自建群升级为工作群
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult SelfGroup2WorkGroup()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                string groupid = HttpContext.Current.Request.Form["groupid"];
                var groupidInt = Convert.ToInt32(groupid);
                var ssoUser = _ssoUserOfWork.GetUserByID(uid);
                if (ssoUser == null)
                {
                    return Json(
                        new
                        {
                            Success = false,
                            Content = "",
                            Error = "无法从sso中查询到对应的用户",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });
                }

                var queryGroup = _unitOfWork.DImGroup.Get(p => p.ID == groupidInt).FirstOrDefault();
                if (queryGroup != null)
                {
                    var queryMembers = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
                    if (queryMembers != null && queryMembers.Any())
                    {
                        var membersList = queryMembers.Select(p => p.UserID).ToList();
                        if (membersList.Contains(uid))
                        {
                            var members = membersList.Where(member => member != queryGroup.Creator).Aggregate(string.Empty, (current, member) => current + (member + ";"));
                            //把最后的分号去掉;

                            if (string.IsNullOrEmpty(members))
                            {
                                return Json(
                                   new
                                   {
                                       Success = false,
                                       Content = "",
                                       Error = "群组成员为空，无法进行升级",
                                       Message = "操作失败",
                                       Count = 0,
                                       Total = 0
                                   });
                            }
                            members = members.Substring(0, members.Length - 1);
                            //发送创建工作群请求
                            var unitid = ssoUser.OrgId;
                            var url = ConfigHelper.GetConfigString("CreateWorkGroup") + "?name=" + queryGroup.Name + "&createUserID=" + uid + "&unitID=" +
                                      unitid + "&memberIdList=" + members + "&summary="+queryGroup.Des;
                            var resp = HttpWebHelper.HttpGet(url);
                            PmcJsonResultCreateGroup result = JsonConvert.DeserializeObject<PmcJsonResultCreateGroup>(resp);
                            if (result != null &&  result.Content != null)
                            {
                                if (result.Success)
                                {
                                    //替换logo为请求的url，否则字符串太长了
                                    result.Content.Logo = ConfigHelper.GetConfigString("WorkGroupPic") + result.Content.ID;
                                    //删除自建群，更新聊天记录中的群组id为新的工作群id
                                    _unitOfWork.DImGroup.Delete(queryGroup);
                                    _unitOfWork.DImGroupDetail.Delete(queryMembers);
                                    var queryImmsg = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid);
                                    if (queryImmsg != null && queryImmsg.Any())
                                    {
                                        foreach (var imMsg in queryImmsg)
                                        {
                                            imMsg.TouID = result.Content.ID;
                                            _unitOfWork.DIMMsg.Update(imMsg);
                                        }
                                    }
                                    var sqlResult=_unitOfWork.Save();
                                    if (sqlResult.ResultType == OperationResultType.Success)
                                    {
                                        //更新缓存数据
                                        foreach (var memberid in membersList)
                                        {
                                            UpdateGroupUnreadMsgRedis(memberid, groupid, result.Content.ID);
                                        }
                                        return Json(new
                                        {
                                            Success = true,
                                            Content = result.Content,
                                            Error = "",
                                            Message = "操作成功",
                                            Count = 0,
                                            Total = 0
                                        });
                                    }
                                    return Json(
                                        new
                                        {
                                            Success = false,
                                            Content = sqlResult.Message,
                                            Error = "",
                                            Message = "操作失败",
                                            Count = 0,
                                            Total = 0
                                        });
                                }
                                return Json(
                                    new
                                    {
                                        Success = false,
                                        Content = "",
                                        Error = result.Error,
                                        Message = "操作失败",
                                        Count = 0,
                                        Total = 0
                                    });
                            }
                        }
                        return Json(
                           new
                           {
                               Success = false,
                               Content = "",
                               Error = "您不是群组成员，不能够执行此操作",
                               Message = "操作失败",
                               Count = 0,
                               Total = 0
                           });
                    }
                    return Json(
                        new
                        {
                            Success = false,
                            Content = "",
                            Error = "未查询到群组成员",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        }); 
                }
                return Json(
                        new
                        {
                            Success = false,
                            Content = "",
                            Error = "未查询到群组信息",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });

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
                    });
            }
        }



        /// <summary>
        /// 自建群升级为工作群(仅限管理员更新数据使用)
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult SelfGroup2WorkGroupForWork()
        {
            try
            {
                string groupid = HttpContext.Current.Request.Form["groupid"];
                var groupidInt = Convert.ToInt32(groupid);
                

                var queryGroup = _unitOfWork.DImGroup.Get(p => p.ID == groupidInt).FirstOrDefault();
                if (queryGroup != null)
                {
                    var ssoUser = _ssoUserOfWork.GetUserByID(queryGroup.Creator);
                    var queryMembers = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid);
                    if (queryMembers != null && queryMembers.Any())
                    {
                        var membersList = queryMembers.Select(p => p.UserID).ToList();
                        if (true)
                        {
                            var members = membersList.Where(member => member != queryGroup.Creator).Aggregate(string.Empty, (current, member) => current + (member + ";"));
                            //把最后的分号去掉;

                            if (string.IsNullOrEmpty(members))
                            {
                                return Json(
                                   new
                                   {
                                       Success = false,
                                       Content = "",
                                       Error = "群组成员为空，无法进行升级",
                                       Message = "操作失败",
                                       Count = 0,
                                       Total = 0
                                   });
                            }
                            members = members.Substring(0, members.Length - 1);
                            //发送创建工作群请求
                            var unitid = ssoUser.OrgId;
                            var url = ConfigHelper.GetConfigString("CreateWorkGroup") + "?name=" + queryGroup.Name + "&createUserID=" + queryGroup.Creator + "&unitID=" + unitid + "&memberIdList=" + members + "&summary=" + queryGroup.Des;
                            var resp = HttpWebHelper.HttpGet(url);
                            PmcJsonResultCreateGroup result = JsonConvert.DeserializeObject<PmcJsonResultCreateGroup>(resp);
                            if (result != null && result.Content != null)
                            {
                                if (result.Success)
                                {
                                    //替换logo为请求的url，否则字符串太长了
                                    result.Content.Logo = ConfigHelper.GetConfigString("WorkGroupPic") + result.Content.ID;
                                    //删除自建群，更新聊天记录中的群组id为新的工作群id
                                    _unitOfWork.DImGroup.Delete(queryGroup);
                                    _unitOfWork.DImGroupDetail.Delete(queryMembers);
                                    var queryImmsg = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid);
                                    if (queryImmsg != null && queryImmsg.Any())
                                    {
                                        foreach (var imMsg in queryImmsg)
                                        {
                                            imMsg.TouID = result.Content.ID;
                                            _unitOfWork.DIMMsg.Update(imMsg);
                                        }
                                    }
                                    var sqlResult = _unitOfWork.Save();
                                    if (sqlResult.ResultType == OperationResultType.Success)
                                    {
                                        //更新缓存数据
                                        foreach (var memberid in membersList)
                                        {
                                            UpdateGroupUnreadMsgRedis(memberid, groupid, result.Content.ID);
                                        }
                                        return Json(new
                                        {
                                            Success = true,
                                            Content = result.Content,
                                            Error = "",
                                            Message = "操作成功",
                                            Count = 0,
                                            Total = 0
                                        });
                                    }
                                    return Json(
                                        new
                                        {
                                            Success = false,
                                            Content = sqlResult.Message,
                                            Error = "",
                                            Message = "操作失败",
                                            Count = 0,
                                            Total = 0
                                        });
                                }
                                return Json(
                                    new
                                    {
                                        Success = false,
                                        Content = "",
                                        Error = result.Error,
                                        Message = "操作失败",
                                        Count = 0,
                                        Total = 0
                                    });
                            }
                        }
                        return Json(
                           new
                           {
                               Success = false,
                               Content = "",
                               Error = "您不是群组成员，不能够执行此操作",
                               Message = "操作失败",
                               Count = 0,
                               Total = 0
                           });
                    }
                    return Json(
                        new
                        {
                            Success = false,
                            Content = "",
                            Error = "未查询到群组成员",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });
                }
                return Json(
                        new
                        {
                            Success = false,
                            Content = "",
                            Error = "未查询到群组信息",
                            Message = "操作失败",
                            Count = 0,
                            Total = 0
                        });

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
                    });
            }
        }







        /// <summary>
        /// 拼接群组头像
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="glist"></param>
        /// <returns></returns>
        private string GetGroupPhoto(string creator,string glist)
        {
            glist = creator + ";" + glist;
            var mList = glist.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (mList.Length == 2)
            {
                var imgUrl0 = ConfigHelper.GetConfigString("SsoPic") + mList[0];
                var imgUrl1 = ConfigHelper.GetConfigString("SsoPic") + mList[1];
                var imageByte0 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl0);
                var imageByte1 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl1);
                var img0 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte0);
                var img1 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte1);
                var imgCut0 = Edu.Image.Tools.ImageTool.CutEllipse(img0, new Rectangle(0, 0, img0.Width, img0.Height),new Size(100, 100));
                var imgCut1 = Edu.Image.Tools.ImageTool.CutEllipse(img1, new Rectangle(0, 0, img1.Width, img1.Height),new Size(100, 100));
                var imgCombi = Edu.Image.Tools.ImageTool.CombinImage(imgCut0, imgCut1);
                var strbase64 = Edu.Image.Tools.ImageTool.ImgToBase64String(new Bitmap(imgCombi));

                
                return "data:image/png;base64," + strbase64;
            }
            else if (mList.Length == 3)
            {
                var imgUrl0 = ConfigHelper.GetConfigString("SsoPic") + mList[0];
                var imgUrl1 = ConfigHelper.GetConfigString("SsoPic") + mList[1];
                var imgUrl2 = ConfigHelper.GetConfigString("SsoPic") + mList[2];
                var imageByte0 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl0);
                var imageByte1 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl1);
                var imageByte2 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl2);
                var img0 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte0);
                var img1 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte1);
                var img2 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte2);
                var imgCut0 = Edu.Image.Tools.ImageTool.CutEllipse(img0, new Rectangle(0, 0, img0.Width, img0.Height), new Size(100, 100));
                var imgCut1 = Edu.Image.Tools.ImageTool.CutEllipse(img1, new Rectangle(0, 0, img1.Width, img1.Height), new Size(100, 100));
                var imgCut2 = Edu.Image.Tools.ImageTool.CutEllipse(img2, new Rectangle(0, 0, img2.Width, img2.Height), new Size(100, 100));
                var imgCombi = Edu.Image.Tools.ImageTool.Combin3Image(imgCut0, imgCut1,imgCut2);
                var strbase64 = Edu.Image.Tools.ImageTool.ImgToBase64String(new Bitmap(imgCombi));
                return "data:image/png;base64," + strbase64;
            }
            else if (mList.Length == 4)
            {
                var imgUrl0 = ConfigHelper.GetConfigString("SsoPic") + mList[0];
                var imgUrl1 = ConfigHelper.GetConfigString("SsoPic") + mList[1];
                var imgUrl2 = ConfigHelper.GetConfigString("SsoPic") + mList[2];
                var imgUrl3 = ConfigHelper.GetConfigString("SsoPic") + mList[3];
                var imageByte0 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl0);
                var imageByte1 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl1);
                var imageByte2 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl2);
                var imageByte3 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl3);
                var img0 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte0);
                var img1 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte1);
                var img2 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte2);
                var img3 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte3);
                var imgCut0 = Edu.Image.Tools.ImageTool.CutEllipse(img0, new Rectangle(0, 0, img0.Width, img0.Height), new Size(100, 100));
                var imgCut1 = Edu.Image.Tools.ImageTool.CutEllipse(img1, new Rectangle(0, 0, img1.Width, img1.Height), new Size(100, 100));
                var imgCut2 = Edu.Image.Tools.ImageTool.CutEllipse(img2, new Rectangle(0, 0, img2.Width, img2.Height), new Size(100, 100));
                var imgCut3 = Edu.Image.Tools.ImageTool.CutEllipse(img3, new Rectangle(0, 0, img3.Width, img3.Height), new Size(100, 100));
                var imgCombi = Edu.Image.Tools.ImageTool.Combin4Image(imgCut0, imgCut1, imgCut2,imgCut3);
                var strbase64 = Edu.Image.Tools.ImageTool.ImgToBase64String(new Bitmap(imgCombi));
                return "data:image/png;base64," + strbase64;
            }
            else if (mList.Length >= 5)
            {
                var imgUrl0 = ConfigHelper.GetConfigString("SsoPic") + mList[0];
                var imgUrl1 = ConfigHelper.GetConfigString("SsoPic") + mList[1];
                var imgUrl2 = ConfigHelper.GetConfigString("SsoPic") + mList[2];
                var imgUrl3 = ConfigHelper.GetConfigString("SsoPic") + mList[3];
                var imgUrl4 = ConfigHelper.GetConfigString("SsoPic") + mList[4];
                var imageByte0 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl0);
                var imageByte1 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl1);
                var imageByte2 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl2);
                var imageByte3 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl3);
                var imageByte4 = Edu.Image.Tools.ImageTool.GetImageFromResponse(imgUrl4);
                var img0 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte0);
                var img1 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte1);
                var img2 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte2);
                var img3 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte3);
                var img4 = Edu.Image.Tools.ImageTool.ByteArray2Image(imageByte4);
                var imgCut0 = Edu.Image.Tools.ImageTool.CutEllipse(img0, new Rectangle(0, 0, img0.Width, img0.Height), new Size(100, 100));
                var imgCut1 = Edu.Image.Tools.ImageTool.CutEllipse(img1, new Rectangle(0, 0, img1.Width, img1.Height), new Size(100, 100));
                var imgCut2 = Edu.Image.Tools.ImageTool.CutEllipse(img2, new Rectangle(0, 0, img2.Width, img2.Height), new Size(100, 100));
                var imgCut3 = Edu.Image.Tools.ImageTool.CutEllipse(img3, new Rectangle(0, 0, img3.Width, img3.Height), new Size(100, 100));
                var imgCut4 = Edu.Image.Tools.ImageTool.CutEllipse(img4, new Rectangle(0, 0, img4.Width, img4.Height), new Size(100, 100));
                var imgCombi = Edu.Image.Tools.ImageTool.Combin5Image(imgCut0, imgCut1, imgCut2, imgCut3,imgCut4);
                var strbase64 = Edu.Image.Tools.ImageTool.ImgToBase64String(new Bitmap(imgCombi));
                return "data:image/png;base64," + strbase64;
            }

            return ConfigHelper.GetConfigString("DefaultGroupLogo");

        }


        /// <summary>
        /// 获取自建群群组成员（IM视频聊天页面使用）
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetGroupMembers(string groupID)
        {
            try
            {
                var query = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupID).OrderByDescending(p=>p.isAdmin);
                List<UserViewGroupMember> list = new List<UserViewGroupMember>();
                foreach (var item in query)
                {
                    var member = new UserViewGroupMember();
                    member.id = item.UserID;
                    member.realName = item.NickName;
                    member.icon = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.UserID;
                    member.isAdmin = (item.isAdmin == 1);
                    list.Add(member);
                }
                return Json(
                    new
                    {
                        Success = true,
                        Data = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = 0
                    });
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Data = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 获取视频会议中群组聊天的消息内容
        /// </summary>
        /// <param name="groupid">群组id</param>
        /// <param name="datetime">取小于这个时间后的数据</param>
        /// <returns></returns>
        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetConferenceMsg(string groupid,string datetime)
        {
            try
            {
                var pageSize = 10;
                var dt = Convert.ToDateTime(datetime);
                //取小于dt这个时间后的10条数据，按照时间从大到小排序
                var query = _unitOfWork.DConferenceMsg.Get(p => p.conferenceid == groupid && p.datetime< dt).OrderByDescending(p=>p.id).Take(pageSize);
                List<ConferenceChatModel> list = new List<ConferenceChatModel>();
                foreach (var item in query)
                {
                    var model = new ConferenceChatModel();
                    model.conferenceId = item.conferenceid;
                    model.uid = item.uid;
                    model.trueName = item.truename;
                    model.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.uid;
                    model.msgContent = item.msgContent;
                    model.msgType = item.msgtype;
                    model.dateTime = item.datetime.Value.ToString("yyyy/MM/dd HH:mm:ss");
                    model.ext = item.ext;
                    list.Add(model);
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = 0
                    });
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
                    });
            }
        }


        /// <summary>
        /// 添加聊天记录（群组视频会议）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult CreateConferenceMsg()
        {
            try
            {
                string conferenceId = HttpContext.Current.Request.Form["conferenceId"];
                string uid = HttpContext.Current.Request.Form["uid"];
                string trueName = HttpContext.Current.Request.Form["trueName"];
                string msgContent = HttpContext.Current.Request.Form["msgContent"];//暂时没用到


                if (string.IsNullOrEmpty(conferenceId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "会议id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "发送者id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(trueName))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "发送者姓名不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(msgContent))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "发送内容不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (msgContent.Length > 255)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "发送内容不能超过255个字符",
                        Count = 0,
                        Total = 0
                    });
                }
                var msg = new ConferenceMsg()
                {
                    conferenceid = conferenceId,
                    uid = uid,
                    truename = trueName,
                    photo = "",
                    msgContent = msgContent,
                    msgtype = 0,
                    datetime = DateTime.Now,
                    ext = "",
                };
                _unitOfWork.DConferenceMsg.Insert(msg);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    //返回类型为ConferenceChatModel
                    var model = new ConferenceChatModel()
                    {
                        conferenceId = msg.conferenceid,
                        uid=msg.uid,
                        trueName = msg.truename,
                        photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + msg.uid,
                        msgContent=msg.msgContent,
                        msgType = msg.msgtype,
                        dateTime = msg.datetime.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                        ext=msg.ext
                    };

                    return Json(new
                    {
                        Success = true,
                        Content = model,
                        Error = "",
                        Message = "创建成功",
                        Count = 0,
                        Total = 0
                    });
                    
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.ToString(),
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 根据群组id，去查询其对应的视频会议的协同研讨的id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        
        public IHttpActionResult GetConferenceDiscuss(string id)
        {
            try
            {
                var query = _unitOfWork.DConferenceDiscuss.Get(p => p.groupid == id).FirstOrDefault();
                var discussid = "";
                if (query != null)
                {
                    discussid = query.discussid;
                }

                return Json(new
                {
                    Success = true,
                    Content = discussid,
                    Error = "",
                    Message = "",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 创建群组视频会议对应的协同研讨id(实际上是将协同研讨的id和群组id对应起来存放到数据库中）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult CreateConferenceDiscuss()
        {
            try
            {
                string groupid = HttpContext.Current.Request.Form["groupid"];
                string discussid = HttpContext.Current.Request.Form["discussid"];
                if (string.IsNullOrEmpty(groupid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(discussid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "协同研讨id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                var model = new ConferenceDiscuss();

                //如果已经存在一条记录，那么就更新
                var query= _unitOfWork.DConferenceDiscuss.Get(p=>p.groupid==groupid).FirstOrDefault();
                if (query != null)
                {
                    query.createdate = new DateTime();
                    query.discussid = discussid;
                    _unitOfWork.DConferenceDiscuss.Update(query);
                    model = query;
                }
                else
                {
                    model = new ConferenceDiscuss()
                    {
                        createdate = new DateTime(),
                        creator = "",
                        discussid = discussid,
                        groupid = groupid,
                        remark = "",
                    };
                    _unitOfWork.DConferenceDiscuss.Insert(model);
                }

                
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = model,
                        Error = "",
                        Message = "创建成功",
                        Count = 0,
                        Total = 0
                    });

                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.ToString(),
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// Android客户端，向服务端发送从个推服务中获取的deviceid
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult SetAndroidUserDeviceToken()
        {
            try
            {
                string userid = HttpContext.Current.Request.Form["userid"];
                string deviceid = HttpContext.Current.Request.Form["deviceid"];
                string devicetoken = HttpContext.Current.Request.Form["devicetoken"];
                if (string.IsNullOrEmpty(userid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "用户id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(deviceid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "设备id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                if (string.IsNullOrEmpty(devicetoken))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "个推的deviceid不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                var query = _unitOfWork.DUserDevice.Get(p =>
                    p.uid == userid || p.devicetoken == devicetoken);
                if (query != null)
                {
                    _unitOfWork.DUserDevice.Delete(query);
                }
                
                    var model = new UserDevice()
                    {
                        uid = userid,
                        deviceid = deviceid,
                        devicetoken = devicetoken,
                        createdate = DateTime.Now,
                        msgcount = 0,
                        devicetype = "android",
                        isonline = 1,
                    };
                    _unitOfWork.DUserDevice.Insert(model);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "创建成功",
                        Count = 0,
                        Total = 0
                    });

                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.ToString(),
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 根据群组id获取群组的创建者和创建者的真实姓名
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupCreator(string groupid)
        {
            try
            {
                var creator = "";
                var realName = "";
                var groupname = "";
                GetGroupCreator(groupid, out creator, out realName,out groupname);

                var model = new GroupCreator
                {
                    creator = creator,
                    realName = realName,
                };

                return Json(new { Success = true, Content = model, Error = "", Message = "", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "", Count = 0, Total = 0 });
            }
        }




        /// <summary>
        /// 用户将该消息转换为通知消息 暂时没用到
        /// </summary>        
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SetNotice()
        {
            try
            {
                //当前用户ID
                string uid = HttpContext.Current.Request.Form["uid"];
                //当前消息ID
                string msgid = HttpContext.Current.Request.Form["msgid"];

                var msgidInt = Convert.ToInt32(msgid);
                
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！用户ID参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(msgid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！消息ID参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }

                var query = _unitOfWork.DIMMsg.Get(p => p.ID == msgidInt && p.IsDel != 1 ).FirstOrDefault();
                if (query != null)
                {
                    if (query.FromuID != uid)
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!用户ID和消息发送者ID不一致", Count = 0, Total = 0 });
                    }
                    if (query.Type=="8")
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!该消息已经是通知类型消息", Count = 0, Total = 0 });
                    }
                    if (query.isgroup == 0)
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!该消息不是群组内的消息", Count = 0, Total = 0 });
                    }

                    query.Type = "8";
                    query.NoticeCreateData = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    _unitOfWork.DIMMsg.Update(query);

                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                    }
                    return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
                }
                else
                {
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 获取通知消息的反馈情况
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="msgid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetNoticeFeedback(string uid,int msgid)
        {

            //var query = _unitOfWork.DGroupNotice.Get(p => p.msgid == msgid).OrderByDescending(p => p.id);

            //1.查询该消息所在群组的所有用户
            //2.查询已经读的用户
            //3.返回已读的用户和未读的用户

            var queryMsg = _unitOfWork.DIMMsg.Get(p => p.ID == msgid).FirstOrDefault();
            if (queryMsg == null)
            {
                return Json(new { Success = false, Content = "", Error = "", Message = "未查询到相关消息", Count = 0, Total = 0 });
            }

            if (queryMsg.FromuID != uid)
            {
                return Json(new { Success = false, Content = "", Error = "", Message = "该消息不是您发送的消息，无法查看相关信息", Count = 0, Total = 0 });
            }

            var list0 = new List<NoticeFeedbackMember>();//已读成员
            var list1 = new List<NoticeFeedbackMember>();//未读成员
            var list2 = new List<GroupNotice>();

            var queryNoticeMember = _unitOfWork.DGroupNotice.Get(p => p.msgid == msgid);
            if(queryNoticeMember!=null && queryNoticeMember.Any())
            {
                list2 = queryNoticeMember.ToList();
            }


            var touids = new List<string>();
            if (queryMsg.TouID.Length == 36)
            {
                //工作群组的ID长度的36位的，自建群的群组ID是从1开始自增的int，所以长度不可能达到36位
                touids = ApiService.GetWorkGroupMembers(queryMsg.TouID);
            }
            else
            {
                touids = ApiService.GetSelfGroupMembers(queryMsg.TouID);
            }
            //touids里是所有的成员
            foreach(var item in touids)
            {
                //排查掉自己
                if (item == queryMsg.FromuID) continue;
                var ssoMember = _ssoUserOfWork.GetUserByID(item);
                if (ssoMember == null) continue;

                var member = new NoticeFeedbackMember();
                member.userId = item;
                member.realName = ssoMember.RealName;
                var readMember = list2.Where(p => p.userid == item).FirstOrDefault();
                if (readMember != null)
                {
                    member.readTime = readMember.createdate;
                    list0.Add(member);
                }
                else
                {
                    member.readTime = "0001/01/01 00:00:00"; ;
                    list1.Add(member);
                }
            }

            if (list0.Any())
            {
                list0.OrderBy(p => Convert.ToDateTime(p.readTime)).ToList();
            }


            var result = new NoticeFeedbackResult();
            result.readList = list0;
            result.unReadList = list1;
            return Json(new { Success = true, Content = result, Error = "", Message = "", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 普通用户将通知消息设置为已读状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ReadNotice()
        {
            try
            {
                //当前用户ID
                string uid = HttpContext.Current.Request.Form["uid"];
                //当前消息ID
                string msgid = HttpContext.Current.Request.Form["msgid"];

                var msgidInt = Convert.ToInt32(msgid);

                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！用户ID参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(msgid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！消息ID参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                var query = _unitOfWork.DIMMsg.Get(p => p.ID == msgidInt && p.IsDel != 1 ).FirstOrDefault();
                if (query != null)
                {
                    if (query.FromuID == uid)
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!用户ID和消息发送者ID相同，无法设置为已读类型", Count = 0, Total = 0 });
                    }
                    if (query.Type!="8")
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!该消息不是通知类型消息", Count = 0, Total = 0 });
                    }
                    if (query.isgroup == 0)
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!该消息不是群组内的消息", Count = 0, Total = 0 });
                    }

                    var queryGroupNotice = _unitOfWork.DGroupNotice.Get(p=>p.userid==uid && p.msgid == msgidInt).FirstOrDefault();
                    if (queryGroupNotice != null)
                    {
                        return Json(new { Success = false, Content = "", Error = "", Message = "操作失败!您已设置过该消息为已读", Count = 0, Total = 0 });
                    }

                    var groupNotice = new GroupNotice
                    {
                        msgid = msgidInt,
                        groupid = query.TouID,//群组id
                        userid = uid,
                        createdate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                    };
                    _unitOfWork.DGroupNotice.Insert(groupNotice);
                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                    }
                    return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
                }
                else
                {
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }

            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "操作失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 查询群组中回执类消息汇总
        /// </summary>
        /// <param name="groupid"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupNoticeMessage(string groupid, string datetime)
        {
            try
            {
                var list = new List<GroupNoticeMessage>();
                var queryMsg = new List<IMMsg>();
                if (string.IsNullOrEmpty(datetime))
                {
                    //取全部
                    var query = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid && p.IsDel != 1 && p.Type=="8").OrderByDescending(p=>p.ID);

                    if (query != null && query.Any())
                    {
                        queryMsg = query.ToList();                        
                    }
                }
                else
                {
                    var dt = Convert.ToDateTime(datetime);
                    //取小于这个时间的10条消息
                    var query = _unitOfWork.DIMMsg.Get(p => p.TouID == groupid && p.IsDel != 1 && p.Type == "8" && p.CreateDate<dt).Take(10).OrderByDescending(p => p.ID);

                    if (query != null && query.Any())
                    {
                        queryMsg = query.ToList();                        
                    }
                }


                var touids = new List<string>();
                foreach (var item in queryMsg)
                {
                    var list0 = new List<NoticeFeedbackMember>();//已读成员
                    var list1 = new List<NoticeFeedbackMember>();//未读成员
                    var list2 = new List<GroupNotice>();

                    if (item.TouID.Length == 36)
                    {
                        touids = ApiService.GetWorkGroupMembers(item.TouID);
                    }
                    else
                    {
                        touids = ApiService.GetSelfGroupMembers(item.TouID);
                    }
                    var queryNoticeMember = _unitOfWork.DGroupNotice.Get(p => p.msgid == item.ID);
                    if (queryNoticeMember != null && queryNoticeMember.Any())
                    {
                        list2 = queryNoticeMember.ToList();
                    }
                    foreach (var uid in touids)
                    {
                        //排查掉自己
                        if (uid == item.FromuID) continue;
                        var ssoMember = _ssoUserOfWork.GetUserByID(uid);
                        if (ssoMember == null) continue;

                        var member = new NoticeFeedbackMember();
                        member.userId = uid;
                        member.realName = ssoMember.RealName;
                        var readMember = list2.Where(p => p.userid == uid).FirstOrDefault();
                        if (readMember != null)
                        {
                            member.readTime = readMember.createdate;
                            list0.Add(member);
                        }
                        else
                        {
                            member.readTime = "0001/01/01 00:00:00"; ;
                            list1.Add(member);
                        }
                    }

                    list.Add(
                        new GroupNoticeMessage()
                        {
                            msg = MsgServices.ImMsg2Msg(item),
                            readList = list0,
                            unReadList = list1,
                        });
                }

                return Json(new { Success = true, Content = list, Error = "", Message = "查询成功", Count = list.Count, Total = list.Count });

            }
            catch(Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "", Count = 0, Total = 0 });
            }
        }







        private void UpdateGroupUnreadMsgRedis(string uid, string oidgroupid, string newgroupid)
        {
            var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid) ?? new List<String>();
            if (unreadgroupmsg.Contains(oidgroupid))
            {
                unreadgroupmsg.Remove(oidgroupid);
                unreadgroupmsg.Add(newgroupid);
                RedisHelper.Hash_Remove("IMUnreadGroupMsg", uid);
                RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", uid, unreadgroupmsg);
            }
            var unreadmsgs=MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", oidgroupid + uid);
            if (unreadmsgs != null)
            {
                MsgServices.ResetRedisKeyValue("IMGroupMsg", newgroupid+uid, unreadmsgs);
            }
            RedisHelper.Hash_Remove("IMGroupMsg", oidgroupid + uid);
        }



        public IHttpActionResult DeleteDiscuss()
        {
            try
            {
                string id = HttpContext.Current.Request.Form["id"];
                var query = _unitOfWork.DPlanDiscuss.Get(p => p.DiscussID == id).FirstOrDefault();
                if (query != null)
                {
                    _unitOfWork.DPlanDiscuss.Delete(query);
                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(new
                        {
                            Success = true,
                            Content = "",
                            Error = "",
                            Message = "操作成功，删除了一条数据",
                            Count = 0,
                            Total = 0
                        });
                    }
                }
                return Json(new
                {
                    Success = true,
                    Content = "",
                    Error = "",
                    Message = "操作成功，查询数据结果为1",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 获取群组列表成员，在SignalR中使用，返回为一个List字符串
        /// </summary>
        /// <param name="groupId">群组ID</param>
        /// <returns></returns>
        [HttpGet]
        [NoAccessToken]
        public List<string> GetMembersList(string groupId)
        {
            LoggerHelper.Info(groupId);
            try
            {
                var list = new List<string>();
                var query = _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupId);
                if (query != null&&query.Any())
                {
                    foreach (var item in query)
                    {
                        list.Add(item.UserID);
                    }
                }
                return list;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }


        /// <summary>
        /// 获取群组列表成员，在SignalR中使用，返回为一个List字符串
        /// </summary>
        /// <param name="groupId">群组ID</param>
        /// <returns></returns>
        [HttpPost]        
        public IHttpActionResult RecallMessage()
        {
            //当前用户ID
            string uid = HttpContext.Current.Request.Form["uid"];
            //当前消息ID
            var msgidInt = Convert.ToInt32(HttpContext.Current.Request.Form["msgid"]);


            var query = _unitOfWork.DIMMsg.Get(p => p.FromuID == uid && p.ID == msgidInt && p.IsDel !=1).FirstOrDefault();
            if (query == null)
            {
                return Json(new { Success = false, Content = "", Error = "", Message = "未查询到对应的消息", Count = 0, Total = 0 });
            }

            query.IsDel = 1;

            _unitOfWork.DIMMsg.Update(query);
            var result = _unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                Task.Run(() =>
                {
                    //如果缓存中有，也把缓存中的消息去掉
                    if (query.isgroup == 1)
                    {
                        //群组聊天消息
                        var touids = new List<string>();
                        if (query.TouID.Length == 36)
                        {
                            //工作群组的ID长度的36位的，自建群的群组ID是从1开始自增的int，所以长度不可能达到36位
                            touids = ApiService.GetWorkGroupMembers(query.TouID);
                        }
                        else
                        {
                            touids = ApiService.GetSelfGroupMembers(query.TouID);
                        }
                        foreach(var item in touids)
                        {
                            MsgServices.RemoveRedisKeyValue("IMGroupMsg", query.TouID + item, query);
                        }
                    }
                    else
                    {
                        //单人聊天消息
                        MsgServices.RemoveRedisKeyValue("IMMsg", query.TouID, query);
                    }
                });
                return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });      
            } 
            return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
        }



        [HttpPost]
        public IHttpActionResult SetNoticeSwitch()
        {
            //当前用户ID
            string uid = HttpContext.Current.Request.Form["uid"];
            //开关类型
            var type = Convert.ToInt32(HttpContext.Current.Request.Form["type"]);
            //开？关
            var isOpen = Convert.ToInt32(HttpContext.Current.Request.Form["isOpen"]);

            var query = _unitOfWork.DUserNoticeSwitch.Get(p => p.uid == uid).FirstOrDefault();
            if (query == null)
            {
                //创建一条新的记录
                var item = new UserNoticeSwitch();
                item.uid = uid;
                if (type == 1)
                {
                    item.type1 = isOpen;
                }
                else if (type == 2)
                {
                    item.type2 = isOpen;
                }
                else if (type == 3)
                {
                    item.type3 = isOpen;
                }
                _unitOfWork.DUserNoticeSwitch.Insert(item);

            }
            else
            {
                if (type == 1)
                {
                    query.type1 = isOpen;
                }
                else if (type == 2)
                {
                    query.type2 = isOpen;
                }
                else if (type == 3)
                {
                    query.type3 = isOpen;
                }
                _unitOfWork.DUserNoticeSwitch.Update(query);
            }
            var result = _unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {              
                return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
        }

        [HttpGet]
        public IHttpActionResult GetNoticeSwitch(string uid)
        {

            var query = _unitOfWork.DUserNoticeSwitch.Get(p => p.uid == uid).FirstOrDefault();
            if (query == null)
            {
                //没有记录，默认全为true
                var item = new UserNoticeSwitch();
                item.id = 0;
                item.uid = uid;
                item.type1 = 1;
                item.type2 = 1;
                item.type3 = 1;
                return Json(new { Success = true, Content = item, Error = "", Message = "查询成功", Count = 1, Total = 1 });
            }
            return Json(new { Success = true, Content = query, Error = "", Message = "查询成功", Count = 1, Total = 1 });
        }







        [HttpGet]
        [HttpPost]
        public IHttpActionResult ApiTest()
        {
            return Json(new { Success = true, Content = "测试接口获取成功,", Error = "", Message = "查询成功", Count = 0, Total = 0 });
        }

        
        [HttpGet]
        [HttpPost]
        public IHttpActionResult ApiTestNoAccessToken()
        {
            return Json(new { Success = true, Content = "测试接口获取成功,没有传递accesstoken也可以访问", Error = "", Message = "查询成功", Count = 0, Total = 0 });
        }

        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetUserOnlieJson()
        {
            var list0 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine");
            //var list1 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLineApp");

            return Json(new { Success = true, Content = list0, Error = "", Message = "查询成功", Count = 0, Total = 0 });

        }

        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetUserOnlieJson2()
        {
            var list1 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLineApp");

            return Json(new { Success = true, Content = list1, Error = "", Message = "查询成功", Count = 0, Total = 0 });
           
        }


        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetSsoUserInfo(string id)
        {
            try
            {
                var ssoUser = _ssoUserOfWork.GetUserByID(id);
                return Json(new { Success = true, Content = ssoUser, Error = "", Message = "查询成功", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = "查询异常！"+ex, Message = "查询失败", Count = 0, Total = 0 });
            }
            
        }



        public ContactFile Immsg2ContactFile(IMMsg msg)
        {
            var file = new ContactFile();
            file.fromuid = msg.CreateUser;
            file.touid = msg.TouID;
            file.createtime = msg.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            file.id = msg.ID.ToString();
            file.filename = msg.Msg;
            file.fileurl = msg.FileUrl;
            file.filesize = msg.Duration;
            file.fromrealname = msg.fromusername;
            file.groupname = "";
            if (msg.isgroup == 1)
            {
                file.groupname = msg.tousername;
            }
            file.torealname = msg.tousername;
            return file;
        }


        public MyGroup ImGroup2MyGroup(ImGroup group)
        {
            var item =new MyGroup();
            item.ID = group.ID.ToString();
            item.Name = group.Name;
            item.Logo = ConfigHelper.GetConfigString("IMWebApiGroupPic") + group.ID;
            item.Summary = group.Des;
            item.CanManage = false;
            item.CreateUserID = group.Creator;
            item.PostTime = group.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            item.GroupType = 1;//自建群
            return item;
        }



        [NoAccessToken]
        [HttpGet]
        public HttpResponseMessage Pic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return GetDefaultGroupImg2();
            try
            {
                var idInt = Convert.ToInt32(id);
                var query = _unitOfWork.DImGroup.Get(p => p.ID == idInt).FirstOrDefault();
                if (query != null)
                {
                    if (!string.IsNullOrEmpty(query.Photo) && query.Photo.Contains("data:image/"))
                    {
                        var base64 = query.Photo.Replace("data:image/webp;base64,", "").Replace("data:image/png;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
                        byte[] imgBytes = Convert.FromBase64String(base64);
                        
                        //return File(imgBytes, "image/png");

                        HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                        result.Content = new ByteArrayContent(imgBytes);
                        //Inline是直接显示,attachment作为附件下载
                        result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                        result.Content.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue("Inline") { };
                        return result;

                    }
                    //如果为空，返回一个默认的
                    return GetDefaultGroupImg2();
                }
            }
            catch (Exception ex)
            {
                return GetDefaultGroupImg2();
            }
            return GetDefaultGroupImg2();
        }



        private HttpResponseMessage GetDefaultGroupImg2()
        {
            var base64 = ConfigHelper.GetConfigString("DefaultGroupLogo").Replace("data:image/png;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");
            byte[] defaultimgBytes1 = Convert.FromBase64String(base64);

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(defaultimgBytes1);
            //Inline是直接显示,attachment作为附件下载
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("Inline") { };
            return result;
        }



        /// <summary>
        /// 移动端分享文件的时候，上传的接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult UploadFileForShare()
        {
            var fileList = System.Web.HttpContext.Current.Request.Files;
            
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                var fileCode = (string)result.Content;        
                
                var fileUrl = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/Download?title=" +
                                           file.FileName + "&fileCode=" + result.Content;
                return Json(new { Success = true, Content = fileUrl, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }



        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult TokenTest(ModelToken model)
        {
            if (model != null)
                return Ok("11");
            return Unauthorized();
        }





        /// <summary>
        /// 群组中新建主题
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddGroupSubject()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string groupid = HttpContext.Current.Request.Form["groupId"];
                string subjectName = HttpContext.Current.Request.Form["subjectName"];
                var uid = userId;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(groupid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(subjectName))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "主题名称不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                //查询是否满足四个主题
                var queryAll = _unitOfWork.DGroupSubject.GetIQueryable(p=>p.groupid== groupid && p.isdel!=1);
                if(queryAll!=null && queryAll.Any()&& queryAll.Count()>=4)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "当前群组主题已经超过4个，无法继续新建",
                        Count = 0,
                        Total = 0
                    });
                }



                var model = new GroupSubject();
                model.groupid = groupid;
                model.name = subjectName;
                model.createtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                model.isdel = 0;
                model.isend = 0;
                model.creator = userId;
                model.remark = string.Empty;
                model.endtime = string.Empty;
                _unitOfWork.DGroupSubject.Insert(model);

                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = model,
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 群组中修改/编辑主题
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditGroupSubject()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string subjectId = HttpContext.Current.Request.Form["subjectId"];
                string subjectName = HttpContext.Current.Request.Form["subjectName"];
                var uid = userId;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                
                if (string.IsNullOrEmpty(subjectName))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "主题名称不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(subjectId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "主题id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var subjectIdInt = Convert.ToInt32(subjectId);
                var query = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt && p.isdel == 0 && p.creator==uid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到对应的主题",
                        Count = 0,
                        Total = 0
                    });
                }
                query.name = subjectName;
                query.createtime= DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _unitOfWork.DGroupSubject.Update(query);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = query,
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 删除群组主题
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteGroupSubject()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string subjectId = HttpContext.Current.Request.Form["subjectId"];
                var uid = userId;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
               
                if (string.IsNullOrEmpty(subjectId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "主题id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var subjectIdInt = Convert.ToInt32(subjectId);
                var query = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt && p.isdel == 0 && p.creator == uid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到对应的主题(创建者为查询到对应的主题)",
                        Count = 0,
                        Total = 0
                    });
                }
                query.isdel = 1;
                query.endtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                _unitOfWork.DGroupSubject.Update(query);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 结束群组主题(该接口已经弃用）
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EndGroupSubject()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string subjectId = HttpContext.Current.Request.Form["subjectId"];
                var uid = userId;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                if (string.IsNullOrEmpty(subjectId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "主题id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var subjectIdInt = Convert.ToInt32(subjectId);
                var query = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt && p.isdel == 0 && p.creator == uid).FirstOrDefault();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到对应的主题",
                        Count = 0,
                        Total = 0
                    });
                }
                query.isend = 1;
                _unitOfWork.DGroupSubject.Update(query);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 查询所有群组主题（包括已经删除的）
        /// </summary>       
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetAllGroupSubject(string goupId, int pageNo, int pageSize)
        {
            try
            {
                if (string.IsNullOrEmpty(goupId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var query = _unitOfWork.DGroupSubject.Get(p =>  p.groupid == goupId).OrderByDescending(p => p.id); ;
                if (query != null && query.Any())
                {
                    if (pageNo == 0 && pageSize == 0)
                    {
                        //查询全部
                        return Json(new
                        {
                            Success = true,
                            Content = query.ToList(),
                            Error = "",
                            Message = "",
                            Count = query.Count(),
                            Total = query.Count(),
                        });

                    }
                    else
                    {
                        //分页查询
                        var result = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            Success = true,
                            Content = result,
                            Error = "",
                            Message = "",
                            Count = result.Count,
                            Total = query.Count(),
                        });
                    }
                }

                return Json(new
                {
                    Success = true,
                    Content = "",
                    Error = "",
                    Message = "查询成功,但是未查询到数据",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 查询群组主题（未删除的，有效的主题）
        /// </summary>       
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupSubject(string goupId,int pageNo,int pageSize)
        {
            var list = new List<GroupSubject>() { };
            try
            {
                if (string.IsNullOrEmpty(goupId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var query = _unitOfWork.DGroupSubject.Get(p => p.isdel == 0 && p.groupid == goupId ).OrderByDescending(p => p.id); ;
                if (query != null && query.Any())
                {
                    if(pageNo==0 && pageSize == 0)
                    {
                        //查询全部
                        return Json(new
                        {
                            Success = true,
                            Content = query.ToList(),
                            Error = "",
                            Message = "",
                            Count = query.Count(),
                            Total = query.Count(),
                        });

                    }
                    else
                    {
                        //分页查询
                        var result = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            Success = true,
                            Content = result,
                            Error = "",
                            Message = "",
                            Count = result.Count,
                            Total = query.Count(),
                        });
                    }
                }

                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功,但是未查询到数据",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = list,
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }

        /// <summary>
        /// 查询已经删除的群组主题
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetDeletedGroupSubject(string groupId, int pageNo, int pageSize)
        {
            var list = new List<GroupSubject>() { };
            try
            {
                if (string.IsNullOrEmpty(groupId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var query = _unitOfWork.DGroupSubject.Get(p => p.isdel == 1 && p.groupid == groupId).OrderByDescending(p => p.id); ;
                if (query != null && query.Any())
                {
                    if (pageNo == 0 && pageSize == 0)
                    {
                        //查询全部
                        return Json(new
                        {
                            Success = true,
                            Content = query.ToList(),
                            Error = "",
                            Message = "",
                            Count = query.Count(),
                            Total = query.Count(),
                        });

                    }
                    else
                    {
                        //分页查询
                        var result = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                        return Json(new
                        {
                            Success = true,
                            Content = result,
                            Error = "",
                            Message = "",
                            Count = result.Count,
                            Total = query.Count(),
                        });
                    }
                }

                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功,但是未查询到数据",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = list,
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 根据时间查询主题下的消息
        /// 若时间字符串为空，则表示查询全部，否则，是查询小于该时间的pageSize条消息
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="subjectId"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupSubjectMessage(string groupId, string subjectId, int pageNo, int pageSize, string dateTime)
        {
            try
            {
                var list = new List<Msg>();//返回的结果
                if (string.IsNullOrEmpty(groupId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (pageSize<=0)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "pageSize不能为0",
                        Count = 0,
                        Total = 0
                    });
                }
                if (pageNo <= 0)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "pageNo不能为0",
                        Count = 0,
                        Total = 0
                    });
                }

                if (string.IsNullOrEmpty(subjectId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "主题id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(dateTime))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "查询时间不能为空",
                        Count = 0,
                        Total = 0
                    });
                }


                var subjectIdInt = Convert.ToInt32(subjectId);
                var querySubject = _unitOfWork.DGroupSubject.Get(p => p.id == subjectIdInt).FirstOrDefault();

                if (querySubject == null){
                    return Json(new
                    {
                        Success = false,
                        Content = list,
                        Error = "",
                        Message = "未查询到对应的主题",
                        Count = 0,
                        Total = 0
                    });
                }

                var dt = Convert.ToDateTime(dateTime);
                var query = _unitOfWork.DIMMsg.Get(p => p.IsDel == 0 && p.isgroup == 1 && p.TouID == groupId && p.SubjectId == subjectId && p.CreateDate<dt).OrderByDescending(p => p.ID);
                if (query != null && query.Any())
                {

                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var imMsg in queryPage)
                    {
                        var msg = MsgServices.ImMsg2Msg(imMsg);

                        if (!string.IsNullOrEmpty(msg.subjectId))
                        {
                            //添加主题名称
                            msg.subjectTitle = querySubject.name;
                        }
                        list.Add(msg);
                    }

                    return Json(new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "",
                        Count = list.Count,
                        Total = query.Count()
                    });
                }
                return Json(new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "查询成功,但是未查询到数据",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = new List<Msg>() { },
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 添加收藏
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddUserFavorites()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string type = HttpContext.Current.Request.Form["type"];
                string content = HttpContext.Current.Request.Form["content"];
                string cid = HttpContext.Current.Request.Form["cid"];
                
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(type))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "type不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                //if (string.IsNullOrEmpty(content))
                //{
                //    return Json(new
                //    {
                //        Success = false,
                //        Content = "",
                //        Error = "",
                //        Message = "content不能为空",
                //        Count = 0,
                //        Total = 0
                //    });
                //}
                if (string.IsNullOrEmpty(cid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "cid不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var model = new UserFavorites();
                var typeInt= Convert.ToInt32(type);
                var query = _unitOfWork.DUserFavorites.Get(p => p.cid == cid && p.type == typeInt && p.uid == userId).FirstOrDefault();
                if (query != null)
                {
                    query.createtime= DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

                    model = query;
                    _unitOfWork.DUserFavorites.Update(query);
                }
                else
                {
                    model.uid = userId;
                    model.content = content;
                    model.cid = cid;
                    model.type = typeInt;
                    model.createtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                    model.remark = string.Empty;
                    _unitOfWork.DUserFavorites.Insert(model);
                }               

                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = model,
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                LoggerHelper.Error(result.ToString());
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = result.Message,
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 删除收藏
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteUserFavorites()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string id = HttpContext.Current.Request.Form["id"];

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "创建者不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(id))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                var idInt = Convert.ToInt32(id);
                var query = _unitOfWork.DUserFavorites.Get(p => p.id == idInt).FirstOrDefault();
                if (query != null)
                {
                    _unitOfWork.DUserFavorites.Delete(query);
                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(new
                        {
                            Success = true,
                            Content = "",
                            Error = "",
                            Message = "操作成功",
                            Count = 0,
                            Total = 0
                        });
                    }
                    LoggerHelper.Error(result.ToString());
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = result.Message,
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
                return Json(new
                {
                    Success = false,
                    Content = "未查询到对应的收藏数据",
                    Error = "",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });


            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        [HttpGet]
        public IHttpActionResult GetUserFavorites(string uid, int type,int pageNo,int pageSize)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "uid不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (pageNo<0 || pageSize < 0) 
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "pageNo和pageSize不能小于0",
                        Count = 0,
                        Total = 0
                    });
                }
                var result = new List<UserFavorites>() { };
                var totalCount = 0;
                if (type == 0)
                {
                    //全部类型的收藏
                    var query = _unitOfWork.DUserFavorites.Get(p => p.uid == uid).OrderByDescending(p=>p.id);
                    if (query != null && query.Any())
                    {
                        totalCount = query.Count();
                        result = query.Skip((pageNo - 1)*pageSize).Take(pageSize).ToList();
                    }
                }
                else
                {
                    //指定类型的收藏
                    var query = _unitOfWork.DUserFavorites.Get(p => p.uid == uid && p.type== type).OrderByDescending(p => p.id);
                    if (query != null && query.Any())
                    {
                        totalCount = query.Count();
                        result = query.Skip((pageNo - 1) * pageSize).Take(pageSize).ToList();
                    }
                }
                foreach(var item in result)
                {
                    if (string.IsNullOrEmpty(item.content))
                    {
                        if (item.type == 1)
                        {
                            //聊天消息类型
                            var msgList = new List<Msg>() { };                            
                            if (item.cid.Contains(";"))
                            {
                                //多条聊天消息一起收藏的
                                var ids=item.cid.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                                var idsInt = Array.ConvertAll<string, int>(ids, s => int.Parse(s));
                                var queryMsg = _unitOfWork.DIMMsg.Get(p => idsInt.Contains(p.ID) && p.IsDel != 1).FirstOrDefault();
                                if (queryMsg != null)
                                {
                                    msgList.Add(MsgServices.ImMsg2Msg(queryMsg));
                                    item.content = JsonConvert.SerializeObject(msgList);
                                }
                            }
                            else
                            {
                                //单条聊天消息
                                var msgIdInt = Convert.ToInt32(item.cid);
                                var queryMsg = _unitOfWork.DIMMsg.Get(p => p.ID == msgIdInt && p.IsDel != 1).FirstOrDefault();
                                if (queryMsg != null)
                                {
                                    //如果是图片类型，将filename字段存放图片的名称
                                    
                                    var msg = MsgServices.ImMsg2Msg(queryMsg);
                                    if (queryMsg.Type == "1")
                                    {
                                        var imageUrl = queryMsg.Msg;
                                        if (!imageUrl.Contains("http")) { imageUrl = "http:" + imageUrl; }
                                        Uri uri = new Uri(imageUrl);
                                        string queryString = uri.Query;
                                        NameValueCollection col = GetQueryString(queryString);

                                        msg.filename = col["title"]==null? col["?title"]:col["title"];
                                    }
                                    msgList.Add(msg);



                                    item.content = JsonConvert.SerializeObject(msgList);
                                }
                            }
                        }
                    }
                }

                //去掉空的
                if (result.Any())
                {
                    result = result.Where(p => !string.IsNullOrEmpty(p.content)).ToList();
                }


                return Json(
                   new
                   {
                       Success = true,
                       Content = result,
                       Error = "",
                       Message = "查询成功",
                       Count = result.Count,
                       Total = totalCount,
                   });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("GetUserFavorites", ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        [HttpGet]
        public IHttpActionResult CheckIsUserFavorite(string uid, int type, string cid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "uid不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                if (string.IsNullOrEmpty(cid))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "cid不能为空",
                        Count = 0,
                        Total = 0
                    });
                }

                //指定类型的收藏
                var query = _unitOfWork.DUserFavorites.Get(p => p.uid == uid && p.type == type && p.cid == cid).FirstOrDefault();
                if (query != null)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = query.id.ToString(),
                        Error = "",
                        Message = "收藏存在",
                        Count = 0,
                        Total = 0
                    });
                }

                return Json(
                   new
                   {
                       Success = true,
                       Content = "",
                       Error = "",
                       Message = "收藏不存在",
                       Count = 0,
                       Total = 0,
                   });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error("GetUserFavorites", ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }







        /// <summary>
        /// App日志记录
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AppLog()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["userId"];
                string content = HttpContext.Current.Request.Form["content"];
                var uid = userId;

                LoggerHelper.Info($"{DateTime.Now.ToString()}{Environment.NewLine}用户ID：{userId}{Environment.NewLine}日志内容：{content}");

                return Json(
                    new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "",
                        Count = 0,
                        Total = 0
                    });
            }
            catch (Exception ex)
            {
                LoggerHelper.Info($"{DateTime.Now.ToString()}{Environment.NewLine}AppLog报错{Environment.NewLine}错误内容：{ex.Message}{Environment.NewLine}{ex.ToString()}");
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        ///   <summary> 
        ///  将查询字符串解析转换为名值集合.
        ///   </summary> 
        ///   <param name="queryString"></param> 
        ///   <returns></returns> 
        public NameValueCollection GetQueryString(string queryString)
        {
            return GetQueryString(queryString, null, true);
        }

        ///   <summary> 
        ///  将查询字符串解析转换为名值集合.
        ///   </summary> 
        ///   <param name="queryString"></param> 
        ///   <param name="encoding"></param> 
        ///   <param name="isEncoded"></param> 
        ///   <returns></returns> 
        public NameValueCollection GetQueryString(string queryString, Encoding encoding, bool isEncoded)
        {
            queryString = queryString.Replace(" ? ", "");
            NameValueCollection result = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(queryString))
            {
                int count = queryString.Length;
                for (int i = 0; i < count; i++)
                {
                    int startIndex = i;
                    int index = -1;
                    while (i < count)
                    {
                        char item = queryString[i];
                        if (item == '=')
                        {
                            if (index < 0)
                            {
                                index = i;
                            }
                        }
                        else if (item == '&')
                        {
                            break;
                        }
                        i++;
                    }
                    string key = null;
                    string value = null;
                    if (index >= 0)
                    {
                        key = queryString.Substring(startIndex, index - startIndex);
                        value = queryString.Substring(index + 1, (i - index) - 1);
                    }
                    else
                    {
                        key = queryString.Substring(startIndex, i - startIndex);
                    }
                    if (isEncoded)
                    {
                        result[MyUrlDeCode(key, encoding)] = MyUrlDeCode(value, encoding);
                    }
                    else
                    {
                        result[key] = value;
                    }
                    if ((i == (count - 1)) && (queryString[i] == '&'))
                    {
                        result[key] = string.Empty;
                    }
                }
            }
            return result;
        }

        //   <summary> 
        ///  解码URL.
        ///   </summary> 
        ///   <param name="encoding"> null为自动选择编码 </param> 
        ///   <param name="str"></param> 
        ///   <returns></returns> 
        public string MyUrlDeCode(string str, Encoding encoding)
        {
            if (encoding == null)
            {
                Encoding utf8 = Encoding.UTF8;
                // 首先用utf-8进行解码                      
                string code = HttpUtility.UrlDecode(str.ToUpper(), utf8);
                // 将已经解码的字符再次进行编码. 
                string encode = HttpUtility.UrlEncode(code, utf8).ToUpper();
                if (str == encode)
                    encoding = Encoding.UTF8;
                else
                    encoding = Encoding.GetEncoding("gb2312");
            }
            return HttpUtility.UrlDecode(str, encoding);
        }



    }
}