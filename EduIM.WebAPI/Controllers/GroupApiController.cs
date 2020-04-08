using System;
using System.Web.Http;
using Edu.Service;
using Edu.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Edu.Models.Models.Msg;
using Edu.Service.Service.Message;
using EduIM.WebAPI.Filters;
using Edu.Entity;
using Edu.Models.Models;
using System.Web;

namespace EduIM.WebAPI.Controllers
{

    [NoAccessToken]
    public class GroupApiController : ApiController
    {
        private readonly ssoUser _ssoUserOfWork = new ssoUser();
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

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
                            Photo = System.Configuration.ConfigurationManager.AppSettings.Get("IMWebApiGroupPic") +
                                    item.ID, //这里作为一个接口url返还回去，目的是将base64字符串作为一个图片文件返还回去
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
        /// 获取当前群组的群组成员
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupId">当前群组id</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetSelfGroupMember(string uid, string groupid)
        {
            try
            {
                var selfuid = uid;
                var query =
                    _unitOfWork.DImGroupDetail.Get(p => p.GroupID == groupid)
                        .OrderByDescending(p => p.isAdmin);
                var totalCount = 0;
                totalCount = query.Count();
                List<EduIM.WebAPI.Models.GroupMember> list = new List<EduIM.WebAPI.Models.GroupMember>();
                //获取所有
                totalCount = query.Count();
                foreach (var item in query)
                {
                    var member = new EduIM.WebAPI.Models.GroupMember
                    {
                        isAdmin = item.isAdmin == 1,
                        photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + item.UserID,
                        uid = item.UserID,
                        realName = item.NickName,
                    };
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
                        Total = totalCount,
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
        /// 获取当前群组的公告
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupAnnouncement(int groupid)
        {
            try
            {
                var query =
                    _unitOfWork.DGroupAnnouncement.Get(p => p.groupid == groupid)
                        .OrderByDescending(p => p.id).ToList();
                var totalCount = 0;
                totalCount = query.Count();

                return Json(
                    new
                    {
                        Success = true,
                        Content = query,
                        Error = "",
                        Message = "查询成功",
                        Count = query.Count,
                        Total = totalCount,
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
        /// 新建群组公告
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CreateGroupAnnouncement()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                int groupid = Convert.ToInt32(HttpContext.Current.Request.Form["groupid"]);
                string title = HttpContext.Current.Request.Form["title"];
                string content = HttpContext.Current.Request.Form["content"];
                string creatorname = HttpContext.Current.Request.Form["creatorname"];

                var model = new GroupAnnouncement
                {
                    groupid = groupid,
                    datetime = DateTime.Now,
                    content = content,
                    creator = uid,
                    title = title,
                    creatorname = creatorname,
                };
                _unitOfWork.DGroupAnnouncement.Insert(model);

                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(
                    new
                    {
                        Success = true,
                        Content = model,
                        Error = "",
                        Message = "",
                        Count = 1,
                        Total = 1,
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
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 编辑群组公告
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditGroupAnnouncement()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                int aid = Convert.ToInt32(HttpContext.Current.Request.Form["aid"]);
                string title = HttpContext.Current.Request.Form["title"];
                string content = HttpContext.Current.Request.Form["content"];

                var query = _unitOfWork.DGroupAnnouncement.Get(p => p.id == aid).FirstOrDefault();

                if (query == null)
                {
                    return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到相关数据",
                        Count = 0,
                        Total = 0
                    });
                }
                if (query.creator != uid)
                {
                    return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "编辑人和创建人不同，无法修改公告",
                        Count = 0,
                        Total = 0
                    });
                }
                query.title = title;
                query.content = content;
                query.datetime = DateTime.Now;

                _unitOfWork.DGroupAnnouncement.Update(query);

                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(
                    new
                    {
                        Success = true,
                        Content = query,
                        Error = "",
                        Message = "",
                        Count = 1,
                        Total = 1,
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
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        /// <summary>
        /// 删除群组公告
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteGroupAnnouncement()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                int aid = Convert.ToInt32(HttpContext.Current.Request.Form["aid"]);

                var query = _unitOfWork.DGroupAnnouncement.Get(p => p.id == aid).FirstOrDefault();

                if (query == null)
                {
                    return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "未查询到相关数据",
                        Count = 0,
                        Total = 0
                    });
                }
                if (query.creator != uid)
                {
                    return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "编辑人和创建人不同，无法删除公告",
                        Count = 0,
                        Total = 0
                    });
                }

                _unitOfWork.DGroupAnnouncement.Delete(query);

                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(
                    new
                    {
                        Success = true,
                        Content = "",
                        Error = "",
                        Message = "",
                        Count = 1,
                        Total = 1,
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
                        Message = "操作失败",
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
        public IHttpActionResult ExitGroup()
        {
            try
            {
                string userId = HttpContext.Current.Request.Form["uid"];
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
        /// 获取当前群组的最新的公告
        /// </summary>
        /// <param name="groupId">当前群组id</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetFirstGroupAnnouncement(int groupid)
        {
            try
            {
                var query =
                    _unitOfWork.DGroupAnnouncement.Get(p => p.groupid == groupid)
                        .OrderByDescending(p => p.id).FirstOrDefault();
                if (query != null)
                {
                    return Json(
                    new
                    {
                        Success = true,
                        Content = query,
                        Error = "",
                        Message = "查询成功",
                        Count = 1,
                        Total = 1,
                    });
                }
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "查询结果为空",
                        Count = 0,
                        Total = 0,
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
    }

}