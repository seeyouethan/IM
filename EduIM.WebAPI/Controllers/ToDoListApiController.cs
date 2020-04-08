using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Edu.Entity;
using Edu.Tools;
using Edu.Models.Enums;
using Edu.Models.IM;
using Edu.Models.JobAssignment;
using Edu.Models.Models;
using Edu.Service.Service;
using EduIM.WebAPI.Filters;
using EduIM.WebAPI.Models;
using EduIM.WebAPI.Service;
using Newtonsoft.Json;
using Edu.Service.Service.Message;

namespace EduIM.WebAPI.Controllers
{
    
    //[WebApiTracker]
    public class ToDoListApiController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private readonly ssoUser _ssoUserOfWork = new ssoUser();

        /// <summary>
        /// 获取时间范围内是否有日程安排(以群组为单位)
        /// </summary>
        /// <param name="groupid">群组ID</param>
        /// <param name="dt0">开始时间</param>
        /// <param name="dt1">结束时间</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupTimeSpanSchedule(string groupid, DateTime dt0, DateTime dt1)
        {
            try
            {
                if (dt0 > dt1)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "查询失败,查询的开始时间不能大于结束时间。",
                        Count = 0,
                        Total = 0
                    });
                }
                var list = new List<GroupTimeSpanScheduleView>();
                var dt = dt0;
                while (dt <= dt1)
                {
                    var dtNext = dt.AddDays(1);
                    var query =
                        _unitOfWork.DPlan.Get(p => p.GroupID == groupid && p.isdel != 1 && (p.StartDate < dtNext && p.EndDate > dt));
                    if (query != null && query.Any())
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy/MM/dd"), haswork = true });
                    }
                    else
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy/MM/dd"), haswork = false });
                    }
                    dt = dt.AddDays(1);
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
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
        /// 获取某一天的日程安排(以群组为单位)
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupid">群组ID</param>
        /// <param name="dt">日期</param>
        /// <param name="pageNo">页码（从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupSchedule(string userId, string groupid, DateTime dt, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var dt0 = dt;
                var dt1 = dt.AddDays(1);
                var list = new List<ScheduleView>();
                var query =
                         _unitOfWork.DPlan.Get(p => p.GroupID == groupid && p.isdel != 1 && (p.StartDate < dt1 && p.EndDate > dt0) && p.Level == 0).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                foreach (var item in queryPage)
                {
                    var schedule = Plan2ScheduleView(userId, item);
                    list.Add(schedule);
                }
                //子任务
                foreach (var scheduleView in list)
                {
                    SetSubSchedule(userId, scheduleView);
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = pageSize,
                        Total = totalCount,
                        //currentPage = pageNo,
                        //pageCount = Math.Ceiling(((float)totalCount / pageSize))
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
        /// 获取某一天的日程安排(以个人为单位)
        /// </summary>
        /// <param name="userId">当前登录用户ID</param>
        /// <param name="dt">日期</param>
        /// <param name="pageNo">页码（从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPersonalSchedule(string userId, DateTime dt, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var dt0 = dt;
                var dt1 = dt0.AddDays(1);
                var selfuid = userId;
                var list = new List<ScheduleView>();
                var query =
                         _unitOfWork.DPlan.Get(p => (p.Creator == selfuid || p.ManagerPerson.Contains(selfuid) || p.ExecutivesPerson == selfuid) && p.isdel != 1 && (p.StartDate < dt1 && p.EndDate > dt0) && p.Level == 0).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                foreach (var item in queryPage)
                {
                    var schedule = Plan2ScheduleView(userId, item);
                    list.Add(schedule);
                }
                //子任务
                foreach (var scheduleView in list)
                {
                    SetSubSchedule(userId, scheduleView);
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
                        //currentPage = pageNo,
                        //pageCount = Math.Ceiling(((float)totalCount / pageSize))
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
        /// 获取日程安排（工作交办）的详细信息数据
        /// </summary>
        /// <param name="scheduleid">日程ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetScheduleDetail(string scheduleid)
        {
            try
            {
                var query =
                    _unitOfWork.DPlan.Get(p => p.Guid == scheduleid && p.isdel != 1).FirstOrDefault();
                if (scheduleid.Length < 36)
                {
                    var idInt = Convert.ToInt32(scheduleid);
                    query =
                        _unitOfWork.DPlan.Get(p => p.ID == idInt && p.isdel != 1).FirstOrDefault();
                }
                var item = new ScheduleDetailView();
                if (query == null)
                {
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = "",
                        Message = "查询失败,未查询到相关数据。",
                        Count = 0,
                        Total = 0
                    });
                }
                var ssoUser = _ssoUserOfWork.GetUserByID(query.ExecutivesPerson);

                //设置已读
                query.isread = 1;
                item.ID = query.Guid;
                item.Title = query.Title;
                item.Content = query.Content;
                item.CreateDate = query.CreateDate.Value.ToString("yyyy/MM/dd HH:mm");
                item.StartDate = query.StartDate.Value.ToString("yyyy/MM/dd HH:mm");
                item.EndDate = query.EndDate.Value.ToString("yyyy/MM/dd HH:mm");
                item.Priority = query.Priority == 0 ? "一般" : query.Priority == 1 ? "紧急" : "特急";
                item.CreatorPhoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + query.Creator;
                item.DoUserPhoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + query.ExecutivesPerson;
                item.Completing = query.Completing + "%";
                item.DoUserTrueName = ssoUser==null?"":ssoUser.RealName;
                item.DoUser = query.ExecutivesPerson;
                item.Creator = query.Creator;
                item.CreatorTrueName = query.CreatorTrueName;
                item.CallDate = Edu.Tools.EnumHelper.EnumDescriptionText<CallDtae>(query.CallDate);
                item.Address = query.Address;
                var mList = new List<ManagerPersonView>();
                if (!string.IsNullOrEmpty(query.ManagerPerson))
                {
                    var strs = query.ManagerPerson.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in strs)
                    {
                        var ssoUserCopy = _ssoUserOfWork.GetUserByID(s);
                        if (ssoUserCopy == null) continue;
                        var copytouser = new ManagerPersonView();
                        copytouser.UserId = s;
                        copytouser.RealName = ssoUserCopy.RealName;
                        copytouser.Photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + s;
                        mList.Add(copytouser);
                    }
                }
                item.ManagerPerson = mList;
                //item.Tag = query.Creator == selfuid
                //    ? "发起者"
                //    : query.ManagerPerson.Contains(selfuid) ? "抄送者" : query.ExecutivesPerson == selfuid ? "执行者" : "";
                var queryfather = _unitOfWork.DPlan.Get(p => p.Guid == query.ParentID).FirstOrDefault();
                if (queryfather != null)
                {
                    item.FatherTitle = queryfather.Title;
                }
                //查询工作日志
                var listLogs = new List<ScheduleLogView>();
                //查询工作日志中包含的文件
                var queryLogs =
                    _unitOfWork.DPlanProgress.Get(p => p.PlanGuid == item.ID)
                        .OrderByDescending(p => p.CreateDate);
                foreach (var log in queryLogs)
                {
                    var logView = PlanLog2ScheduleLogView(log);
                    if (!string.IsNullOrEmpty(log.Files))
                    {
                        var fileArry = log.Files.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var fileid in fileArry)
                        {
                            var id = Convert.ToInt32(fileid);
                            var queryplanfile = _unitOfWork.DPlanFile.Get(p => p.ID == id).FirstOrDefault();
                            if (queryplanfile != null)
                            {
                                var file = PlanLogFile2ScheduleLogFileView(queryplanfile);
                                logView.LogFiles.Add(file);
                            }
                        }
                    }
                    listLogs.Add(logView);
                }
                item.PlanLogs = listLogs;

                Task.Run(() =>
                {
                    _unitOfWork.DPlan.Update(query);
                    _unitOfWork.Save();
                });

                return Json(
                    new
                    {
                        Success = true,
                        Content = item,
                        Error = "",
                        Message = "查询成功",
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
        /// 获取时间范围内是否有日程安排(以个人为单位)
        /// </summary>
        /// <param name="userId">当前登录用户ID</param>
        /// <param name="dt0">开始时间</param>
        /// <param name="dt1">结束时间</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPersonalTimeSpanSchedule(string userId, DateTime dt0, DateTime dt1)
        {
            try
            {
                if (dt0 > dt1)
                {
                    return Json(new { Success = false, Content = "", Error = "", Message = "查询失败,查询的开始时间不能大于结束时间。", Count = 0, Total = 0 });
                }
                var selfuid = userId;
                var list = new List<GroupTimeSpanScheduleView>();
                var dt = dt0;
                while (dt <= dt1)
                {
                    var query =
                        _unitOfWork.DPlan.Get(p => (p.Creator == selfuid || p.ManagerPerson.Contains(selfuid) || p.ExecutivesPerson == selfuid) && p.isdel != 1 && p.StartDate <= dt && p.EndDate > dt);
                    if (query != null && query.Any())
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy/MM/dd"), haswork = true });
                    }
                    else
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy/MM/dd"), haswork = false });
                    }
                    dt = dt.AddDays(1);
                }
                return Json(new { Success = true, Content = list, Error = "", Message = "查询成功", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 新建工作任务
        /// </summary>
        /// <param name="title"></param>
        /// <param name="creator"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="priority"></param>
        /// <param name="calldate"></param>
        /// <param name="person0"></param>
        /// <param name="person1"></param>
        /// <param name="content"></param>
        /// <param name="groupid"></param>
        /// <param name="parentid"></param>
        /// <param name="realname"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CreateSchedule()
        {
            try
            {
                var title = HttpContext.Current.Request.Form["title"];
                var creator = HttpContext.Current.Request.Form["creator"];
                var startdate = HttpContext.Current.Request.Form["startdate"];
                var enddate = HttpContext.Current.Request.Form["enddate"];
                var priority = Convert.ToInt32(HttpContext.Current.Request.Form["priority"]);
                var calldate = Convert.ToInt32(HttpContext.Current.Request.Form["calldate"]);
                var person0 = HttpContext.Current.Request.Form["person0"];
                var person1 = HttpContext.Current.Request.Form["person1"];
                var content = HttpContext.Current.Request.Form["content"];
                var groupid = HttpContext.Current.Request.Form["groupid"];
                var parentid = HttpContext.Current.Request.Form["parentid"];
                var realname = HttpContext.Current.Request.Form["realname"];
                var address = HttpContext.Current.Request.Form["address"];
                if (string.IsNullOrEmpty(creator))
                {
                    return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(content))
                {
                    return Json(new { Success = false, Content = "", Error = "工作交办的具体描述不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (priority > 2)
                {
                    return Json(new { Success = false, Content = "", Error = "【优先级】参数不正确", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (calldate > 5)
                {
                    return Json(new { Success = false, Content = "", Error = "【提醒】参数不正确", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(realname))
                {
                    return Json(new { Success = false, Content = "", Error = "【创建者真实姓名】参数不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(person1))
                {
                    return Json(new { Success = false, Content = "", Error = "【执行人】参数不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                var dt0 = Convert.ToDateTime(startdate);
                var dt1 = Convert.ToDateTime(enddate);
                if (dt0 > dt1)
                {
                    return Json(new { Success = false, Content = "", Error = "开始时间不能大于结束时间", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (dt0 == Convert.ToDateTime("0001-01-01 00:00:00"))
                {
                    return Json(new { Success = false, Content = "", Error = "【开始时间为0】参数不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (dt1 == Convert.ToDateTime("0001-01-01 00:00:00"))
                {
                    return Json(new { Success = false, Content = "", Error = "【结束时间为0】参数不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                var item = new Plan();
                item.Title = title;
                item.Creator = creator;
                item.StartDate = dt0;
                item.EndDate = dt1;
                item.Priority = priority;
                item.CallDate = calldate;
                item.ManagerPerson = person0;
                if (string.IsNullOrEmpty(person0))
                {
                    item.ManagerPerson = "";//防止后面出现null，查询报错
                }
                item.ExecutivesPerson = person1;
                item.Content = content;
                item.GroupID = groupid;
                item.ParentID = parentid; //非子任务 parentid都设置为0
                item.CreateDate = DateTime.Now;
                item.Guid = Guid.NewGuid().ToString();
                item.Completing = 0;
                item.CompleteBZ = "";
                item.Adress = "";
                item.isTop = 0;
                item.isdel = 0;
                item.isread = 0;
                item.Level = GetLevel(item.ParentID);
                item.Fid = GetFid(item.ParentID);
                item.CreatorTrueName = realname;
                item.Address = address;
                _unitOfWork.DPlan.Insert(item);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {

                    Task.Run(() =>
                    {
                        try
                        {
                            if (item.CallDate != 0)
                            {
                                //1.请求所在的工作群组id集合
                                var url = ConfigHelper.GetConfigString("GetGroupInfo") + "?groupID=" + item.GroupID + "&userID=" + creator;
                                var resp = HttpWebHelper.HttpGet(url);
                                var groupInfo = new GroupInfo();

                                PmcGetGroupInfoJsonResult urlResult = JsonConvert.DeserializeObject<PmcGetGroupInfoJsonResult>(resp);
                                if (urlResult != null && urlResult.Content != null)
                                    groupInfo = urlResult.Content;
                                var dt = "";
                                var scheduleModel = new Edu.Models.JobAssignment.ScheduleModel()
                                {
                                    id = item.ID,
                                    title = item.Title,
                                    startDate = item.StartDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                                    endDate = item.EndDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                                    callDate = item.CallDate,
                                    groupId = item.GroupID,
                                    content = item.Content,
                                    groupName = groupInfo == null ? "" : groupInfo.Name ?? "",
                                    exePerson = item.ExecutivesPerson,
                                };
                                var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                                //计算要发送的时间
                                switch (item.CallDate)
                                {
                                    case 1:
                                        dt = Convert.ToDateTime(item.StartDate).AddMinutes(-5).ToString("yyyy/MM/dd HH:mm:ss");
                                        break;
                                    case 2:
                                        dt = Convert.ToDateTime(item.StartDate).AddMinutes(-10).ToString("yyyy/MM/dd HH:mm:ss");
                                        break;
                                    case 3:
                                        dt = Convert.ToDateTime(item.StartDate).AddMinutes(-15).ToString("yyyy/MM/dd HH:mm:ss");
                                        break;
                                    case 4:
                                        dt = Convert.ToDateTime(item.StartDate).AddMinutes(-30).ToString("yyyy/MM/dd HH:mm:ss");
                                        break;
                                    case 5:
                                        dt = Convert.ToDateTime(item.StartDate).AddMinutes(-60).ToString("yyyy/MM/dd HH:mm:ss");
                                        break;
                                    case 6:
                                        dt = Convert.ToDateTime(item.StartDate).AddDays(-1).ToString("yyyy/MM/dd HH:mm:ss");
                                        break;
                                }

                                if (!string.IsNullOrEmpty(dt))
                                {
                                    scheduleModel.noticeDate = dt;
                                    //发送提前通知
                                    queueAddress.Send<Edu.Models.JobAssignment.ScheduleModel>(scheduleModel);
                                    //防止接受队列那边redis并发 
                                    Thread.Sleep(1000);
                                }
                                scheduleModel.noticeDate = scheduleModel.startDate;
                                //发送开始时间的通知
                                queueAddress.Send<Edu.Models.JobAssignment.ScheduleModel>(scheduleModel);
                            }

                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error(ex.Message);
                        }
                    });


                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 编辑工作任务
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="title"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="priority"></param>
        /// <param name="calldate"></param>
        /// <param name="person0"></param>
        /// <param name="person1"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditSchedule()
        {
            try
            {
                var sid = HttpContext.Current.Request.Form["sid"];
                var title = HttpContext.Current.Request.Form["title"];
                var startdate = HttpContext.Current.Request.Form["startdate"];
                var enddate = HttpContext.Current.Request.Form["enddate"];
                var priority = Convert.ToInt32(HttpContext.Current.Request.Form["priority"]);
                var calldate = Convert.ToInt32(HttpContext.Current.Request.Form["calldate"]);
                var person0 = HttpContext.Current.Request.Form["person0"];
                var person1 = HttpContext.Current.Request.Form["person1"];
                var content = HttpContext.Current.Request.Form["content"];
                if (string.IsNullOrEmpty(sid))
                {
                    return Json(new { Success = false, Content = "", Error = "请输入正确的sid", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (priority > 2)
                {
                    return Json(new { Success = false, Content = "", Error = "【优先级】参数不正确", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (calldate > 5)
                {
                    return Json(new { Success = false, Content = "", Error = "【提醒】参数不正确", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(person1))
                {
                    return Json(new { Success = false, Content = "", Error = "【执行人】参数不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                var dt0 = Convert.ToDateTime(startdate);
                var dt1 = Convert.ToDateTime(enddate);
                if (dt0 > dt1)
                {
                    return Json(new { Success = false, Content = "", Error = "开始时间不能大于结束时间", Message = "操作失败", Count = 0, Total = 0 });
                }
                var item = _unitOfWork.DPlan.Get(p => p.Guid == sid && p.isdel != 1).FirstOrDefault();
                if (item == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未找到相关的工作任务", Message = "操作失败", Count = 0, Total = 0 });
                }
                item.Title = title;
                //item.Creator = creator;
                item.StartDate = dt0;
                item.EndDate = dt1;
                item.Priority = priority;
                item.CallDate = calldate;
                item.ManagerPerson = person0;
                if (string.IsNullOrEmpty(person0))
                {
                    item.ManagerPerson = "";//防止后面出现null，查询报错
                }
                item.ExecutivesPerson = person1;
                item.Content = content;
                _unitOfWork.DPlan.Update(item);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 删除对应的guid为sid的工作任务（软删除）
        /// </summary>
        /// <param name="userId">当前用户的ID</param>
        /// <param name="sid">工作任务的guid</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteSchedule()
        {
            var userId = HttpContext.Current.Request.Form["userId"];
            var sid = HttpContext.Current.Request.Form["sid"];
            var uid = userId;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 });
            }
            if (string.IsNullOrEmpty(sid))
            {
                return Json(new { Success = false, Content = "", Error = "请输入正确的sid", Message = "操作失败", Count = 0, Total = 0 });
            }
            try
            {
                var query = _unitOfWork.DPlan.Get(p => p.Guid == sid && p.isdel != 1).FirstOrDefault();
                if (query == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未找到相关的工作任务", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (query.Creator != uid)
                {
                    return Json(new { Success = false, Content = "", Error = "您不是该工作任务的创建者，无法删除该工作任务", Message = "操作失败", Count = 0, Total = 0 });
                }
                query.isdel = 1;
                _unitOfWork.DPlan.Update(query);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 创建工作日志
        /// </summary>
        /// <param name="planid">工作任务ID</param>
        /// <param name="creator">创建人ID</param>
        /// <param name="content">工作日志内容</param>
        /// <param name="curprogress">当前工作任务完成度，整数类型</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CreateScheduleLog()
        {
            try
            {
                var planid = HttpContext.Current.Request.Form["planid"];
                var creator = HttpContext.Current.Request.Form["creator"];
                var content = HttpContext.Current.Request.Form["content"];
                var curprogress = Convert.ToInt32(HttpContext.Current.Request.Form["curprogress"]);
                var uid = creator;
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(content))
                {
                    return Json(new { Success = false, Content = "", Error = "工作日志内容不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                var log = new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = uid;
                log.CreateDate = DateTime.Now;
                log.Content = content;
                log.PlanID = 0;//这个字段冗余了，暂时赋值为0吧，不受影响
                log.PlanGuid = planid;
                log.CurProgress = curprogress;
                _unitOfWork.DPlanProgress.Insert(log);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = PlanLog2ScheduleLogView(log), Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 编辑工作日志
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="logid">工作日志ID</param>
        /// <param name="content">工作日志内容</param>
        /// <param name="curprogress">当前工作任务完成度，整数类型</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult EditScheduleLog()
        {
            var userId = HttpContext.Current.Request.Form["userId"];
            var logid = HttpContext.Current.Request.Form["logid"];
            var content = HttpContext.Current.Request.Form["content"];
            var curprogress = Convert.ToInt32(HttpContext.Current.Request.Form["curprogress"]);

            if (string.IsNullOrEmpty(logid))
            {
                return Json(new { Success = false, Content = "", Error = "工作日志ID不能为空", Message = "操作失败", Count = 0, Total = 0 });
            }
            if (string.IsNullOrEmpty(content))
            {
                return Json(new { Success = false, Content = "", Error = "工作日志内容不能为空", Message = "操作失败", Count = 0, Total = 0 });
            }
            try
            {
                var log = _unitOfWork.DPlanProgress.Get(p => p.Guid == logid && p.IsDel != 1).FirstOrDefault();
                if (log == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未查询到相关工作日志", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (log.Creator != userId)
                {
                    return Json(new { Success = false, Content = "", Error = "您不是该工作日志的创建者，无法进行编辑操作", Message = "操作失败", Count = 0, Total = 0 });
                }
                log.Content = content;
                log.CurProgress = curprogress;
                _unitOfWork.DPlanProgress.Update(log);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = PlanLog2ScheduleLogView(log), Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 删除工作日志
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="logid">工作日志ID</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult DeleteScheduleLog()
        {
            var userId = HttpContext.Current.Request.Form["userId"];
            var logid = HttpContext.Current.Request.Form["logid"];
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { Success = false, Content = "", Error = "userId不能为空", Message = "操作失败", Count = 0, Total = 0 });
            }
            if (string.IsNullOrEmpty(logid))
            {
                return Json(new { Success = false, Content = "", Error = "工作日志ID不能为空", Message = "操作失败", Count = 0, Total = 0 });
            }
            try
            {
                var query = _unitOfWork.DPlanProgress.Get(p => p.Guid == logid && p.IsDel != 1).FirstOrDefault();
                if (query == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未查询到相关工作日志数据", Message = "操作失败", Count = 0, Total = 0 });
                }
                //判断是否有删除权限,工作任务的创建者和执行者才有权限去删除
                var queryPlan = _unitOfWork.DPlan.Get(p => p.Guid == query.PlanGuid && p.isdel != 1).FirstOrDefault();
                if (userId != query.Creator)
                {
                    if (queryPlan == null)
                    {
                        return Json(new { Success = false, Content = "", Error = "未查询到相关工作任务数据", Message = "操作失败", Count = 0, Total = 0 });
                    }
                    if (userId != queryPlan.Creator && userId != queryPlan.ExecutivesPerson)
                    {
                        {
                            return Json(new { Success = false, Content = "", Error = "您当前没有权限删除该工作日志", Message = "操作失败", Count = 0, Total = 0 });
                        }
                    }
                }
                query.IsDel = 1;
                _unitOfWork.DPlanProgress.Update(query);
                //更新工作日志
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    //获取这条记录之前的最新的工作日志 修改齐对应的工作任务的完成度的百分比
                    if (queryPlan != null)
                    {
                        var querylogs =
                            _unitOfWork.DPlanProgress.Get(p => p.PlanGuid == query.PlanGuid && p.IsDel != 1)
                                .OrderByDescending(p => p.CreateDate);
                        if (querylogs.Any())
                        {
                            queryPlan.Completing = querylogs.ToList()[0].CurProgress;
                        }
                        else
                        {
                            queryPlan.Completing = 0;
                        }
                        _unitOfWork.DPlan.Update(queryPlan);
                        result = _unitOfWork.Save();
                        if (result.ResultType == OperationResultType.Success)
                        {
                            return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                        }
                        return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
                    }
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 取消勾选，设置工作任务完成状态为非完成状态
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SetScheduleUnFinished()
        {
            try
            {
                var planid = HttpContext.Current.Request.Form["planid"];
                var uid = HttpContext.Current.Request.Form["uid"];
                var truename = HttpContext.Current.Request.Form["usertruename"];
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(planid))
                {
                    return Json(new { Success = false, Content = "", Error = "工作任务ID不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(truename))
                {
                    return Json(new { Success = false, Content = "", Error = "真实姓名不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                var query = _unitOfWork.DPlan.Get(p => p.Guid == planid && p.isdel != 1).FirstOrDefault();
                if (query != null)
                {
                    var querylog =
                    _unitOfWork.DPlanProgress.Get(p => p.PlanGuid == planid && p.IsDel != 1)
                        .OrderByDescending(p => p.CreateDate);
                    var lastcompelting = 0;
                    //查询倒数第二条记录
                    if (querylog.Any() && querylog.Count() >= 2)
                    {
                        lastcompelting = querylog.ToList()[1].CurProgress;
                    }
                    //更新任务的完成状态
                    query.Completing = lastcompelting;
                    query.CompleteDate = null;
                    _unitOfWork.DPlan.Update(query);
                    //记录任务日志
                    var log = new PlanProgress();
                    log.Guid = Guid.NewGuid().ToString();
                    log.Creator = LoginUserService.ssoUserID;
                    log.CreateDate = DateTime.Now;
                    log.Content = truename+"设置任务为未完成状态";
                    log.PlanID = query.ID;
                    log.PlanGuid = query.Guid;
                    log.CurProgress = lastcompelting;
                    _unitOfWork.DPlanProgress.Insert(log);
                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                    }
                    return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = "未找到相应的工作任务", Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 勾选，设置工作任务完成为100%
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SetScheduleFinished()
        {
            try
            {
                var planid = HttpContext.Current.Request.Form["planid"];
                var uid = HttpContext.Current.Request.Form["uid"];
                var truename = HttpContext.Current.Request.Form["usertruename"];
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(planid))
                {
                    return Json(new { Success = false, Content = "", Error = "工作任务ID不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(truename))
                {
                    return Json(new { Success = false, Content = "", Error = "真实姓名不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                var log = new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = uid;
                log.CreateDate = DateTime.Now;
                log.Content = truename + "设置任务为完成状态";
                log.PlanID = 0;//这个字段冗余了，暂时赋值为0吧，不受影响
                log.PlanGuid = planid;
                log.CurProgress = 100;
                _unitOfWork.DPlanProgress.Insert(log);
                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    var queryPlan = _unitOfWork.DPlan.Get(p => p.Guid == planid).FirstOrDefault();
                    if (queryPlan != null)
                    {
                        queryPlan.CompleteDate = DateTime.Now;
                        queryPlan.Completing = 100;
                        queryPlan.isread = 1;
                        _unitOfWork.DPlan.Update(queryPlan);
                        result = _unitOfWork.Save();
                        if (result.ResultType == OperationResultType.Success)
                        {
                            return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                        }
                        return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
                    }
                    return Json(new { Success = false, Content = "", Error = "未找到相应的工作任务", Message = "操作失败", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 根据用户ID和代表任务名称模糊查找工作交办任务
        /// </summary>
        /// <param name="userid"></param>
        /// <param name="title"></param>
        /// <param name="dt0"></param>
        /// <param name="dt1"></param>
        /// <param name="timespan"></param>
        /// <param name="pageNo"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetPersonalScheduleByTitle(string userid, string title,DateTime dt0,DateTime dt1, bool timespan, int pageNo = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(userid))
            {
                return Json(new { Success = false, Content = "", Error = "用户ID不能为空", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (string.IsNullOrEmpty(title.Trim()))
            {
                return Json(new { Success = false, Content = "", Error = "查找关键字不能为空", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (timespan && (dt1 < dt0))
            {
                return Json(new { Success = false, Content = "", Error = "查询开始时间不能大于查询结束时间", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (pageNo < 0)
            {
                return Json(new { Success = false, Content = "", Error = "查询页码不能小于0", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (pageSize < 10)
            {
                return Json(new { Success = false, Content = "", Error = "查询每页数据不能小于0", Message = "查询失败", Count = 0, Total = 0 });
            }
            var list = new List<ScheduleView>();
            if (timespan)
            {
                //查找时间范围内的
                var query = _unitOfWork.DPlan.Get( p=> (p.Creator == userid || p.ManagerPerson.Contains(userid) || p.ExecutivesPerson == userid) && p.Title.Contains(title) && p.isdel != 1 && p.StartDate < dt1 && p.EndDate > dt0).OrderByDescending(p => p.CreateDate);
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var item in queryPage)
                    {
                        var schedule = Plan2ScheduleView(userid, item);
                        list.Add(schedule);
                    }
                }
                return Json(list.Any() ? new { Success = true, Content = list, Error = "", Message = "查询成功", Count = list.Count, Total = query.Count() } : new { Success = true, Content = list, Error = "", Message = "查询成功,但是未查询到相关数据", Count = 0, Total = 0 });
            }
            else
            {
                //查询所有的
                var query = _unitOfWork.DPlan.Get(p => (p.Creator == userid || p.ManagerPerson.Contains(userid) || p.ExecutivesPerson == userid) && p.Title.Contains(title) && p.isdel != 1).OrderByDescending(p => p.CreateDate);
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var item in queryPage)
                    {
                        var schedule = Plan2ScheduleView(userid, item);
                        list.Add(schedule);
                    }
                }
                return Json(list.Any() ? new { Success = true, Content = list, Error = "", Message = "查询成功", Count = list.Count, Total = query.Count() } : new { Success = true, Content = list, Error = "", Message = "查询成功,但是未查询到相关数据", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 根据用户ID查询已经超过通知时间，但是并没有进行通知的工作人物
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [HttpGet][NoAccessToken]
        public IHttpActionResult GetUnReadSchedules(string userid)
        {
            if (!string.IsNullOrEmpty(userid))
            {
                var dt = DateTime.Now;
                var query = _unitOfWork.DPlan.Get(p =>
                    p.ExecutivesPerson == userid && p.isdel !=1 && p.isread == 0 && p.StartDate < dt &&
                    p.EndDate > dt && p.CallDate!=0);
                var list=new List<ScheduleModel>();
                if (query.Any())
                {
                    foreach (var item in query)
                    {
                        //1.请求所在的工作群组id集合
                        var url = ConfigHelper.GetConfigString("GetGroupInfo") + "?groupID=" + item.GroupID + "&userID=" + item.Creator;
                        var resp = HttpWebHelper.HttpGet(url);
                        var groupInfo = new GroupInfo();

                        PmcGetGroupInfoJsonResult urlResult = JsonConvert.DeserializeObject<PmcGetGroupInfoJsonResult>(resp);
                        if (urlResult != null && urlResult.Content != null)
                            groupInfo = urlResult.Content;

                        LogHelper.Info(resp);
                        var scheduleModel = new Edu.Models.JobAssignment.ScheduleModel()
                        {
                            id = item.ID,
                            title = item.Title,
                            startDate = item.StartDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                            endDate = item.EndDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                            callDate = item.CallDate,
                            groupId = item.GroupID,
                            content = item.Content,
                            groupName = groupInfo == null ? "" : groupInfo.Name ?? "",
                            exePerson = item.ExecutivesPerson,
                            noticeDate = ""
                        };
                        list.Add(scheduleModel);
                    }
                }
                return Json(
                    new EduJsonResult
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "",
                        Count = list.Count,
                        Total = list.Count
                    });
            }
            return Json(
                new EduJsonResult
                {
                    Success = false,
                    Content = null,
                    Error = "userid不能为空",
                    Message = "",
                    Count = 0,
                    Total = 0
                });
        }

        private void SetSubSchedule(string userId, ScheduleView plan)
        {
            var query = _unitOfWork.DPlan.Get(p => p.ParentID == plan.ID && p.isdel != 1);
            if (query != null && query.Any())
            {
                foreach (var item in query)
                {
                    var schedule = Plan2ScheduleView(userId, item);
                    SetSubSchedule(userId, schedule);
                    plan.SubSchedules.Add(schedule);
                }
            }
        }

        private ScheduleView Plan2ScheduleView(string userId, Plan plan)
        {
            var ssoUser = _ssoUserOfWork.GetUserByID(plan.ExecutivesPerson);

            var selfuid = userId;
            var schedule = new ScheduleView();
            schedule.ID = plan.Guid;
            schedule.Fid = plan.Fid;
            schedule.Level = plan.Level;
            schedule.Title = plan.Title;
            schedule.CreateDate = plan.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            schedule.StartDate = plan.StartDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            schedule.EndDate = plan.EndDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            schedule.TopDateTime = plan.TopDate == null ? "" : plan.TopDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
            schedule.Priority = plan.Priority == 0 ? "一般" : plan.Priority == 1 ? "紧急" : "特急";
            schedule.Completing = plan.Completing + "%";
            schedule.DoUserTrueName = ssoUser==null?"": ssoUser.RealName;
            schedule.DoUser = plan.ExecutivesPerson;
            schedule.Creator = plan.Creator;
            schedule.CreatorTrueName = plan.CreatorTrueName;
            schedule.Address = plan.Address;
            if (!string.IsNullOrEmpty(plan.ManagerPerson))
            {
                schedule.Tag = plan.Creator == selfuid
                    ? "发起者"
                    : plan.ManagerPerson.Contains(selfuid) ? "抄送者" : plan.ExecutivesPerson == selfuid ? "执行者" : "";
                schedule.CanTop = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid || plan.ManagerPerson.Contains(selfuid);
            }
            else
            {
                schedule.Tag = plan.Creator == selfuid
                       ? "发起者" : plan.ExecutivesPerson == selfuid ? "执行者" : "";
                schedule.CanTop = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid;
            }
            schedule.CanEdit = plan.Creator == selfuid;
            schedule.CanCreateWorkLog = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid;
            schedule.CanCreateSub = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid;
            schedule.CanDelete = plan.Creator == selfuid;
            schedule.Content = plan.Content;
            schedule.CallDate = Edu.Tools.EnumHelper.EnumDescriptionText<CallDtae>(plan.CallDate);
            var mList = new List<ManagerPersonView>();
            if (!string.IsNullOrEmpty(plan.ManagerPerson))
            {
                var strs = plan.ManagerPerson.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in strs)
                {
                    var ssoUserCopy = _ssoUserOfWork.GetUserByID(s);
                    if (ssoUserCopy == null) continue;
                    var copytouser = new ManagerPersonView();
                    copytouser.UserId = s;
                    copytouser.RealName = ssoUserCopy.RealName;
                    copytouser.Photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + s;
                    mList.Add(copytouser);
                }
            }
            schedule.ManagerPerson = mList;
            return schedule;
        }

        private ScheduleLogView PlanLog2ScheduleLogView(PlanProgress log)
        {
            var item = new ScheduleLogView()
            {
                Guid = log.Guid,
                Creator = log.Creator,
                CreateDate = log.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                Content = log.Content,
                CurProgress = log.CurProgress + "%",
            };
            return item;
        }

        private ScheduleLogFileView PlanLogFile2ScheduleLogFileView(PlanFile file)
        {
            var item = new ScheduleLogFileView()
            {
                Guid = file.Guid,
                Creator = file.Creator,
                CreateDate = file.CreateTime.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                FileName = file.FileName,
                FileExtension = file.FileExtension,
                FileUrl = file.FileUrl,
                NewFileName = file.NewFileName
            };
            return item;
        }

        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetGroupScheduleByTitle(string userid, string groupid, string title, DateTime dt0, DateTime dt1, bool timespan, int pageNo = 1, int pageSize = 10)
        {
            if (string.IsNullOrEmpty(groupid))
            {
                return Json(new { Success = false, Content = "", Error = "群组ID不能为空", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (string.IsNullOrEmpty(title.Trim()))
            {
                return Json(new { Success = false, Content = "", Error = "查找关键字不能为空", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (timespan && (dt1 < dt0))
            {
                return Json(new { Success = false, Content = "", Error = "查询开始时间不能大于查询结束时间", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (pageNo < 0)
            {
                return Json(new { Success = false, Content = "", Error = "查询页码不能小于0", Message = "查询失败", Count = 0, Total = 0 });
            }
            if (pageSize < 10)
            {
                return Json(new { Success = false, Content = "", Error = "查询每页数据不能小于0", Message = "查询失败", Count = 0, Total = 0 });
            }
            var list = new List<ScheduleView>();
            if (timespan)
            {
                //查找时间范围内的
                var query = _unitOfWork.DPlan.Get(p => p.GroupID == groupid && p.Title.Contains(title) && p.isdel != 1 && p.StartDate < dt1 && p.EndDate > dt0).OrderByDescending(p => p.CreateDate);
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var item in queryPage)
                    {
                        var schedule = Plan2ScheduleView(userid, item);
                        list.Add(schedule);
                    }
                }
                return Json(list.Any() ? new { Success = true, Content = list, Error = "", Message = "查询成功", Count = list.Count, Total = query.Count() } : new { Success = true, Content = list, Error = "", Message = "查询成功,但是未查询到相关数据", Count = 0, Total = 0 });
            }
            else
            {
                //查询所有的
                var query = _unitOfWork.DPlan.Get(p => p.GroupID == groupid && p.Title.Contains(title) && p.isdel == 0).OrderByDescending(p => p.CreateDate);
                if (query.Any())
                {
                    var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                    foreach (var item in queryPage)
                    {
                        var schedule = Plan2ScheduleView(userid, item);
                        list.Add(schedule);
                    }
                }
                return Json(list.Any() ? new { Success = true, Content = list, Error = "", Message = "查询成功", Count = list.Count, Total = query.Count() } : new { Success = true, Content = list, Error = "", Message = "查询成功,但是未查询到相关数据", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 稍后提醒
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult QuartzSchedule()
        {
            try
            {
                var id = Convert.ToInt32(HttpContext.Current.Request.Form["id"]);
                var title = HttpContext.Current.Request.Form["title"];
                var startDate = HttpContext.Current.Request.Form["startDate"];
                var endDate = HttpContext.Current.Request.Form["endDate"];
                var callDate = Convert.ToInt32(HttpContext.Current.Request.Form["callDate"]);
                var groupId = HttpContext.Current.Request.Form["groupId"];
                var content = HttpContext.Current.Request.Form["content"];
                var groupName = HttpContext.Current.Request.Form["groupName"];
                var exePerson = HttpContext.Current.Request.Form["exePerson"];
                if (string.IsNullOrEmpty(title))
                {
                    return Json(new  { Success = false, Content = "", Error = "title不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(startDate))
                {
                    return Json(new  { Success = false, Content = "", Error = "startDate不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(groupId))
                {
                    return Json(new  { Success = false, Content = "", Error = "groupId不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(content))
                {
                    return Json(new  { Success = false, Content = "", Error = "content不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }
                
                if (string.IsNullOrEmpty(exePerson))
                {
                    return Json(new  { Success = false, Content = "", Error = "exePerson不能为空", Message = "操作失败", Count = 0, Total = 0 });
                }

                var scheduleModel = new Edu.Models.JobAssignment.ScheduleModel()
                {
                    id = id,
                    title = title,
                    startDate = startDate,
                    endDate = endDate,
                    callDate = callDate,
                    groupId = groupId,
                    content = content,
                    groupName = groupName,
                    exePerson = exePerson,
                };
                scheduleModel.noticeDate = DateTime.Now.AddMinutes(10).ToString("yyyy/MM/dd HH:mm:ss");
                var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;

                queueAddress.Send<Edu.Models.JobAssignment.ScheduleModel>(scheduleModel);

                return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }



        /// <summary>
        /// 不再推送 向rabbtmq中写入一条消息，用来取消scheduler中的job  传入的参数是Plan的ID
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public IHttpActionResult CancelQuartzSchedule()
        {
            try
            {
                var id = HttpContext.Current.Request.Form["id"];
                //这里存放的是该日程对应的已经建立的定时任务的 name ,服务端那边是根据name去删除定时任务的
                var jobs = MsgServices.GetRedisKeyValue<string>("a.scheduler.job", id);
                if (jobs.Any())
                {
                    var schedulerJobModel = new SchedulerJobModel()
                    {
                        id = id,
                        jobList = jobs
                    };

                    var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;

                    queueAddress.Send<Edu.Models.JobAssignment.SchedulerJobModel>(schedulerJobModel);
                }
                return Json(new EduJsonResult(){ Success = true, Content = "", Error = null, Message = $"操作成功，停止了{jobs.Count}条定时任务", Count = 0, Total = 0 });

            }
            catch (Exception ex)
            {
                return Json(new EduJsonResult { Success = false, Content = null, Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetLevel(string id)
        {
            if (id == "0") return 0;
            var query = _unitOfWork.DPlan.Get(p => p.Guid == id).FirstOrDefault();
            if (query != null) return (query.Level + 1);
            return 0;
        }

        private string GetFid(string id)
        {
            var count = 0;
            var query = _unitOfWork.DPlan.Get(p => p.ParentID == id);
            count = query.Count();
            if (id == "0")
            {
                if (count < 9)
                {
                    return "0" + (count + 1) + ".";
                }
                return (count + 1) + ".";
            }
            else
            {
                var queryParent = _unitOfWork.DPlan.Get(p => p.Guid == id).FirstOrDefault();
                if (count < 9)
                {
                    return queryParent.Fid + "0" + (count + 1) + ".";
                }
                return queryParent.Fid + (count + 1) + ".";
            }
        }

        [HttpGet]
        [NoAccessToken]
        public IHttpActionResult GetDoUsers(string uid)
        {
            try {
                var ssoUser = _ssoUserOfWork.GetUserByID(uid);
                var douser = new CopyToUser();
                douser.uid = uid;
                douser.name = ssoUser==null?"": ssoUser.RealName;
                douser.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + uid;
                return Json(new { Success = true, Content = douser, Error = "", Message = "", Count = 1, Total = 1 });
            }
            catch(Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "", Count = 0, Total = 0 });
            }
            
        }

    }
}