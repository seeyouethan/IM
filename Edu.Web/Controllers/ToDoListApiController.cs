using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Edu.Entity;
using Edu.Models.Enums;
using Edu.Models.Models;
using Edu.Service;
using Edu.Tools;

namespace Edu.Web.Controllers
{
    public class ToDoListApiController : BaseControl
    {
        /// <summary>
        /// 获取时间范围内是否有日程安排(以群组为单位)
        /// </summary>
        /// <param name="groupid">群组ID</param>
        /// <param name="dt0">开始时间</param>
        /// <param name="dt1">结束时间</param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public JsonResult GetGroupTimeSpanSchedule(string groupid, DateTime dt0, DateTime dt1)
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
                    }, JsonRequestBehavior.AllowGet);
                }
                var list = new List<GroupTimeSpanScheduleView>();
                var dt = dt0;
                while (dt <= dt1)
                {
                    var query =
                        unitOfWork.DPlan.Get(p => p.GroupID == groupid && p.isdel != 1 && p.StartDate <= dt && p.EndDate > dt);
                    if (query != null && query.Any())
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy-MM-dd"), haswork = true });
                    }
                    else
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy-MM-dd"), haswork = false });
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
                        Count = "",
                        Total = ""
                    }, JsonRequestBehavior.AllowGet);
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
                    }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 获取时间范围内是否有日程安排(以个人为单位)
        /// </summary>
        /// <param name="dt0">开始时间</param>
        /// <param name="dt1">结束时间</param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public JsonResult GetPersonalTimeSpanSchedule(DateTime dt0, DateTime dt1)
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
                    }, JsonRequestBehavior.AllowGet);
                }
                var selfuid = LoginUserService.ssoUserID;
                var list = new List<GroupTimeSpanScheduleView>();
                var dt = dt0;
                while (dt <= dt1)
                {
                    var query =
                        unitOfWork.DPlan.Get(p => (p.Creator == selfuid || p.ManagerPerson.Contains(selfuid) || p.ExecutivesPerson == selfuid) && p.isdel != 1 && p.StartDate <= dt && p.EndDate > dt);
                    if (query != null && query.Any())
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy-MM-dd"), haswork = true });
                    }
                    else
                    {
                        list.Add(new GroupTimeSpanScheduleView() { datetime = dt.ToString("yyyy-MM-dd"), haswork = false });
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
                    }, JsonRequestBehavior.AllowGet);
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
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取时间范围内是否有日程安排(以群组为单位)
        /// </summary>
        /// <param name="groupid">群组ID</param>
        /// <param name="dt">日期</param>
        /// <param name="pageNo">页码（从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public JsonResult GetGroupSchedule(string groupid, DateTime dt, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var list = new List<ScheduleView>();
                var query =
                         unitOfWork.DPlan.Get(p => p.GroupID == groupid && p.isdel != 1 && p.StartDate <= dt && p.EndDate > dt && p.Level == 0).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                foreach (var item in queryPage)
                {
                    var schedule = Plan2ScheduleView(item);
                    list.Add(schedule);
                }
                //子任务
                foreach (var scheduleView in list)
                {
                    SetSubSchedule(scheduleView);
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
                    }, JsonRequestBehavior.AllowGet);
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
                    }, JsonRequestBehavior.AllowGet);
            }
        }
        /// <summary>
        /// 查询日程详情明细
        /// </summary>
        /// <param name="scheduleid">日程ID</param>
        /// <returns></returns>
        public JsonResult GetScheduleDetail(string scheduleid)
        {
            try
            {
                var selfuid = LoginUserService.ssoUserID;
                var item = new ScheduleDetailView();
                var query =
                         unitOfWork.DPlan.Get(p => p.Guid == scheduleid && p.isdel != 1).FirstOrDefault();
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
                    }, JsonRequestBehavior.AllowGet);
                }

                var ssoUser = ssoUserOfWork.GetUserByID(query.ExecutivesPerson);
                var DoUserTrueName = "";
                if (ssoUser != null)
                {
                    DoUserTrueName = ssoUser.RealName;
                }
                item.ID = query.Guid;
                item.Title = query.Title;
                item.Content = query.Content;
                item.CreateDate = query.CreateDate.Value.ToString("yyyy/MM/dd hh:mm");
                item.StartDate = query.StartDate.Value.ToString("yyyy/MM/dd hh:mm");
                item.EndDate = query.EndDate.Value.ToString("yyyy/MM/dd hh:mm");
                item.Priority = query.Priority == 0 ? "一般" : query.Priority == 1 ? "紧急" : "特急";
                item.CreatorPhoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + query.Creator;
                item.DoUserPhoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + query.ExecutivesPerson;
                item.Completing = query.Completing + "%";
                item.DoUserTrueName = DoUserTrueName;
                item.DoUser = query.ExecutivesPerson;
                item.Creator = query.Creator;
                item.CreatorTrueName = query.CreatorTrueName;
                item.CallDate = EnumHelper.EnumDescriptionText<CallDtae>(query.CallDate);
                var mList = new List<ManagerPersonView>();
                if (!string.IsNullOrEmpty(query.ManagerPerson))
                {
                    var strs= query.ManagerPerson.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var s in strs)
                    {
                        var user = ssoUserOfWork.GetUserByID(s);
                        if (user == null) continue;
                        var copytouser = new ManagerPersonView();
                        copytouser.UserId = s;
                        copytouser.RealName = user.RealName;
                        copytouser.Photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + s;
                        mList.Add(copytouser);
                    }
                }
                item.ManagerPerson = mList;
                //item.Tag = query.Creator == selfuid
                //    ? "发起者"
                //    : query.ManagerPerson.Contains(selfuid) ? "抄送者" : query.ExecutivesPerson == selfuid ? "执行者" : "";
                var queryfather = unitOfWork.DPlan.Get(p => p.Guid == query.ParentID).FirstOrDefault();
                if (queryfather != null)
                {
                    item.FatherTitle = queryfather.Title;
                }
                //查询工作日志
                var listLogs = new List<ScheduleLogView>();
                //查询工作日志中包含的文件
                var queryLogs =
                    unitOfWork.DPlanProgress.Get(p => p.PlanGuid == item.ID)
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
                            var queryplanfile = unitOfWork.DPlanFile.Get(p => p.ID == id).FirstOrDefault();
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
                return Json(
                    new
                    {
                        Success = true,
                        Content = item,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0
                    }, JsonRequestBehavior.AllowGet);
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
                    }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 获取时间范围内是否有日程安排(以个人为单位)
        /// </summary>
        /// <param name="dt">日期</param>
        /// <param name="pageNo">页码（从1开始）</param>
        /// <param name="pageSize">每页数据个数（默认为10）</param>
        /// <returns></returns>
        [HttpGet]
        [ValidateInput(false)]
        public JsonResult GetPersonalSchedule(DateTime dt, int pageNo = 1, int pageSize = 10)
        {
            try
            {
                var selfuid = LoginUserService.ssoUserID;
                var list = new List<ScheduleView>();
                var query =
                         unitOfWork.DPlan.Get(p => (p.Creator == selfuid || p.ManagerPerson.Contains(selfuid) || p.ExecutivesPerson == selfuid) && p.isdel != 1 && p.StartDate <= dt && p.EndDate > dt && p.Level == 0).OrderBy(p => p.CreateDate);
                var totalCount = query.Count();
                var queryPage = query.Skip((pageNo - 1) * pageSize).Take(pageSize);
                foreach (var item in queryPage)
                {
                    var schedule = Plan2ScheduleView(item);
                    list.Add(schedule);
                }
                //子任务
                foreach (var scheduleView in list)
                {
                    SetSubSchedule(scheduleView);
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
                    }, JsonRequestBehavior.AllowGet);
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
                    }, JsonRequestBehavior.AllowGet);
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
        public JsonResult CreateSchedule(string title,string creator,string startdate,string enddate,int priority,int calldate,string person0,string person1,string content,string groupid,string parentid,string realname)
        {
            var uid = LoginUserService.ssoUserID;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (priority > 2)
            {
                return Json(new { Success = false, Content = "", Error = "【优先级】参数不正确", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (calldate > 5)
            {
                return Json(new { Success = false, Content = "", Error = "【提醒】参数不正确", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var dt0 = Convert.ToDateTime(startdate);
                var dt1 = Convert.ToDateTime(enddate);
                if (dt0 > dt1)
                {
                    return Json(new { Success = false, Content = "", Error = "开始时间不能大于结束时间", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                var item = new Plan();
                item.Title = title;
                item.Creator = creator;
                item.StartDate = dt0;
                item.EndDate = dt1;
                item.Priority = priority;
                item.CallDate = calldate;
                item.ManagerPerson = person0;
                item.ExecutivesPerson = person1;
                item.Content = content;
                item.GroupID = groupid;
                item.ParentID = parentid; //非子任务 parentid都设置为0
                item.CreateDate = DateTime.Now;
                item.Creator = uid;
                item.Guid = Guid.NewGuid().ToString();
                item.Completing = 0;
                item.CompleteBZ = "";
                item.Adress = "";
                item.isTop = 0;
                item.isdel = 0;
                item.Level = GetLevel(item.ParentID);
                item.Fid = GetFid(item.ParentID);
                item.CreatorTrueName = realname;
                unitOfWork.DPlan.Insert(item);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = Plan2ScheduleView(item), Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
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
        public JsonResult EditSchedule(string sid,string title, string startdate, string enddate, int priority, int calldate, string person0, string person1, string content)
        {
            var uid = LoginUserService.ssoUserID;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(sid))
            {
                return Json(new { Success = false, Content = "", Error = "请输入正确的sid", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (priority > 2)
            {
                return Json(new { Success = false, Content = "", Error = "【优先级】参数不正确", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (calldate > 5)
            {
                return Json(new { Success = false, Content = "", Error = "【提醒】参数不正确", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var dt0 = Convert.ToDateTime(startdate);
                var dt1 = Convert.ToDateTime(enddate);
                if (dt0 > dt1)
                {
                    return Json(new { Success = false, Content = "", Error = "开始时间不能大于结束时间", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                var item = unitOfWork.DPlan.Get(p => p.Guid == sid && p.isdel != 1).FirstOrDefault();
                if (item == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未找到相关的工作任务", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                item.Title = title;
                item.StartDate = dt0;
                item.EndDate = dt1;
                item.Priority = priority;
                item.CallDate = calldate;
                item.ManagerPerson = person0;
                item.ExecutivesPerson = person1;
                item.Content = content;
                
                unitOfWork.DPlan.Update(item);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = Plan2ScheduleView(item), Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 删除对应的guid为sid的工作任务（软删除）
        /// </summary>
        /// <param name="sid">工作任务的guid</param>
        /// <returns></returns>
        public JsonResult DeleteSchedule(string sid)
        {
            var uid = LoginUserService.ssoUserID;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(sid))
            {
                return Json(new { Success = false, Content = "", Error = "请输入正确的sid", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var query = unitOfWork.DPlan.Get(p => p.Guid == sid && p.isdel != 1).FirstOrDefault();
                if (query == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未找到相关的工作任务", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                query.isdel = 1;
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
        }




        /// <summary>
        /// 创建工作日志
        /// </summary>
        /// <param name="planid">工作任务ID</param>
        /// <param name="creator">创建人</param>
        /// <param name="content">工作日志内容</param>
        /// <param name="curprogress">当前工作任务完成度，整数类型</param>
        /// <returns></returns>
        public JsonResult CreateScheduleLog(string planid,string creator, string content,int curprogress)
        {
            var uid = creator;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(content))
            {
                return Json(new { Success = false, Content = "", Error = "工作日志内容不能为空", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var log = new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = uid;
                log.CreateDate = DateTime.Now;
                log.Content = content;
                log.PlanID = 0;//这个字段冗余了，暂时赋值为0吧，不受影响
                log.PlanGuid = planid;
                log.CurProgress = curprogress;
                unitOfWork.DPlanProgress.Insert(log);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = PlanLog2ScheduleLogView(log), Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 编辑工作日志
        /// </summary>
        /// <param name="planid">工作任务ID</param>
        /// <param name="creator">创建人</param>
        /// <param name="content">工作日志内容</param>
        /// <param name="curprogress">当前工作任务完成度，整数类型</param>
        /// <returns></returns>
        public JsonResult EditScheduleLog(string planid, string creator, string content, int curprogress)
        {
            var uid = creator;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(content))
            {
                return Json(new { Success = false, Content = "", Error = "工作日志内容不能为空", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var log = new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = uid;
                log.CreateDate = DateTime.Now;
                log.Content = content;
                log.PlanID = 0;//这个字段冗余了，暂时赋值为0吧，不受影响
                log.PlanGuid = planid;
                log.CurProgress = curprogress;
                unitOfWork.DPlanProgress.Insert(log);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { Success = true, Content = PlanLog2ScheduleLogView(log), Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// 删除工作日志
        /// </summary>
        /// <param name="id">工作日志ID</param>
        /// <returns></returns>
        public JsonResult DeleteScheduleLog(string id)
        {
            var uid = LoginUserService.ssoUserID;
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new { Success = false, Content = "", Error = "当前用户未登录", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { Success = false, Content = "", Error = "工作日志ID不能为空", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            try
            {
                var query = unitOfWork.DPlanProgress.Get(p => p.Guid == id && p.IsDel != 1).FirstOrDefault();
                if (query == null)
                {
                    return Json(new { Success = false, Content = "", Error = "未查询到相关工作日志数据", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                }
                //判断是否有删除权限,工作任务的创建者和执行者才有权限去删除
                var queryPlan = unitOfWork.DPlan.Get(p => p.Guid == query.PlanGuid && p.isdel != 1).FirstOrDefault();
                if (uid != query.Creator)
                {
                    if (queryPlan == null)
                    {
                        return Json(new { Success = false, Content = "", Error = "未查询到相关工作任务数据", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                    }
                    if (uid != queryPlan.Creator && uid != queryPlan.ExecutivesPerson)
                    {
                        {
                            return Json(new { Success = false, Content = "", Error = "您当前没有权限删除该工作日志", Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                query.IsDel = 1;
                unitOfWork.DPlanProgress.Update(query);
                //更新工作日志
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    //获取这条记录之前的最新的工作日志 修改齐对应的工作任务的完成度的百分比
                    if (queryPlan != null)
                    {
                        var querylogs =
                            unitOfWork.DPlanProgress.Get(p => p.PlanGuid == query.PlanGuid && p.IsDel != 1)
                                .OrderByDescending(p => p.CreateDate);
                        if (querylogs.Any())
                        {
                            queryPlan.Completing = querylogs.ToList()[0].CurProgress;
                        }
                        else
                        {
                            queryPlan.Completing = 0;
                        }
                        unitOfWork.DPlan.Update(queryPlan);
                        result = unitOfWork.Save();
                        if (result.ResultType == OperationResultType.Success)
                        {
                            return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new { Success = false, Content = "", Error = result.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.Message, Message = "操作失败", Count = 0, Total = 0 }, JsonRequestBehavior.AllowGet);
            }
        }








        private void SetSubSchedule(ScheduleView plan)
        {
            var query = unitOfWork.DPlan.Get(p => p.ParentID == plan.ID && p.isdel != 1);
            if (query != null && query.Any())
            {
                foreach (var item in query)
                {
                    var schedule = Plan2ScheduleView(item);
                    SetSubSchedule(schedule);
                    plan.SubSchedules.Add(schedule);
                }
            }
        }

        private ScheduleView Plan2ScheduleView(Plan plan)
        {
            var selfuid = LoginUserService.ssoUserID;

            var ssoUser = ssoUserOfWork.GetUserByID(plan.ExecutivesPerson);
            var DoUserTrueName = "";
            if (ssoUser != null)
            {
                DoUserTrueName = ssoUser.RealName;
            }

            var schedule = new ScheduleView()
            {
                ID = plan.Guid,
                Fid = plan.Fid,
                Level = plan.Level,
                Title = plan.Title,
                CreateDate = plan.CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                StartDate = plan.StartDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                EndDate = plan.EndDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                TopDateTime = plan.TopDate == null ? "" : plan.TopDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                Priority = plan.Priority == 0 ? "一般" : plan.Priority == 1 ? "紧急" : "特急",
                Completing = plan.Completing + "%",
                DoUserTrueName = DoUserTrueName,
                DoUser = plan.ExecutivesPerson,
                Creator = plan.Creator,
                CreatorTrueName = plan.CreatorTrueName,
                Tag = plan.Creator == selfuid ? "发起者" : plan.ManagerPerson.Contains(selfuid) ? "抄送者" : plan.ExecutivesPerson == selfuid ? "执行者" : "",
                CanEdit = plan.Creator == selfuid,
                CanCreateWorkLog = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid,
                CanCreateSub = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid,
                CanDelete = plan.Creator == selfuid,
                CanTop = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid || plan.ManagerPerson.Contains(selfuid),
                Content = plan.Content,
                CallDate = EnumHelper.EnumDescriptionText<CallDtae>(plan.CallDate),
            };
            var mList = new List<ManagerPersonView>();
            if (!string.IsNullOrEmpty(plan.ManagerPerson))
            {
                var strs = plan.ManagerPerson.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in strs)
                {
                    var user = ssoUserOfWork.GetUserByID(s);
                    if (user == null) continue;
                    var copytouser = new ManagerPersonView();
                    copytouser.UserId = s;
                    copytouser.RealName = user.RealName;
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


        /// <summary>
        /// 根据parentID获取level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int GetLevel(string id)
        {
            if (id == "0") return 0;
            var query = unitOfWork.DPlan.Get(p => p.Guid == id).FirstOrDefault();
            if (query != null) return (query.Level + 1);
            return 0;
        }

        private string GetFid(string id)
        {
            var count = 0;
            var query = unitOfWork.DPlan.Get(p => p.ParentID == id);
            if (id == "0")
            {
                count = query.Count();
                if (count < 9)
                {
                    return "0" + (count + 1) + ".";
                }
                return (count + 1) + ".";
            }
            var queryParent = unitOfWork.DPlan.Get(p => p.Guid == id).FirstOrDefault();
            count = query.Count();
            if (count < 9)
            {
                return queryParent.Fid + "0" + (count + 1) + ".";
            }
            return queryParent.Fid + (count + 1) + ".";
        }

    }
}


