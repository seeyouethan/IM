using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Cnki.NetworkDisk.Client;
using Edu.Entity;
using Edu.Models.Models;
using Edu.Service;
using Edu.Tools;
using KNet.AAMS.Foundation.Model;
using KNet.Core;
using KNet.Data.Entity;
using OKMS.RPMS.Model.ViewModels;
using OKMS.Chapter.Model;

namespace Edu.Web.Controllers
{
    public class ToDoListController : BaseControl
    {
        // GET: ToDoList
        public ActionResult Index(string groupid)
        {
            ViewBag.groupid = groupid;
            return View();
        }

        /// <summary>
        /// 工作交办
        /// </summary>
        /// <returns></returns>
        public ActionResult MyWork()
        {
            //页面上需要 当前用户的uid logo realname
            ViewBag.userId = LoginUserService.ssoUserID;
            ViewBag.realName = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.logo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            return View();
        }

        public async Task<ActionResult> MyWorkPmc()
        {
            var selfuid = LoginUserService.ssoUserID;
            /*写入到缓存中  存到Redis里面 Key为AllMembers cookie存放的话 大小不够 */
            await Task.Run(() =>
            {
                var currentuser = ssoUserOfWork.GetUserByID(selfuid);
                var memberList = ssoUserOfWork.GetMembersOfUnit(currentuser.OrgId);
                RedisHelper.Hash_Set<List<object>>("AllMembers", currentuser.OrgId, memberList);
            });
            return View();
        }

        public ActionResult FullCalendar()
        {
            return View();
        }

        public ActionResult MoreDetail(string uid)
        {
            ViewBag.uid = uid;
            return View();
        }

        public ActionResult Detail(string uid)
        {
            ViewBag.uid = uid;
            return View();
        }

        public ActionResult DetailNew(string uid, string top)
        {
            ViewBag.uid = uid;
            ViewBag.top = top; //箭头的高度
            ViewBag.id = LoginUserService.ssoUserID;
            ViewBag.realName = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.icon = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            return View();
        }

        public JsonResult DetailNewAPI(string uid)
        {
            var fatherTitle = "无";
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                var creatorPhoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + query.Creator;
                var creatorName = query.CreatorTrueName;
                var queryfather = unitOfWork.DPlan.Get(p => p.Guid == query.ParentID).FirstOrDefault();
                if (queryfather != null)
                {
                    fatherTitle = queryfather.Title;
                }
                return Json(new
                {
                    r = true,
                    data = query,
                    creatorPhoto = creatorPhoto,
                    creatorName = creatorName,
                    fatherTitle = fatherTitle
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// fullcalendar的获取数据源将 月、周、日的格式分开了，因为在前端视图中，月视图展现的工作任务跨度会少一天，所以在查询月视图的时候需要添加一天，在周视图上查询出的数据的时间格式不能带时分秒，在日视图上查询出的数据的时间格式需要携带时分秒
        /// </summary>
        /// <param name="d1"></param>
        /// <param name="d2"></param>
        /// <returns></returns>
        public JsonResult GetCalendarPlanMonth(DateTime d1, DateTime d2)
        {
            var uid = LoginUserService.ssoUserID;
            var query =
                unitOfWork.DPlan.Get(
                    p =>
                        p.StartDate < d2 && p.EndDate > d1 &&
                        (p.ExecutivesPerson == uid || p.Creator == uid || p.ManagerPerson.Contains(uid)) && p.isdel != 1);
            var list = new List<PlanFullCalendarView>();
            if (query != null)
            {
                foreach (var item in query)
                {
                    var plan = new PlanFullCalendarView();
                    plan.id = item.Guid;
                    plan.title = item.Title;
                    plan.start = item.StartDate.Value.ToString("yyyy-MM-dd");
                    //plan.end = item.EndDate.Value.AddDays(1).ToString("yyyy-MM-dd"); //要加上一天 因为界面显示的时候会提前一天结束
                    plan.end = item.EndDate.Value.ToString("yyyy-MM-dd"); //要加上一天 因为界面显示的时候会提前一天结束
                    list.Add(plan);
                }
            }
            return Json(new {data = list}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCalendarPlanWeek(DateTime d1, DateTime d2)
        {
            var uid = LoginUserService.ssoUserID;
            var query =
                unitOfWork.DPlan.Get(
                    p =>
                        p.StartDate < d2 && p.EndDate > d1 &&
                        (p.ExecutivesPerson == uid || p.Creator == uid || p.ManagerPerson.Contains(uid)) && p.isdel != 1);
            var list = new List<PlanFullCalendarView>();
            if (query != null)
            {
                foreach (var item in query)
                {
                    var plan = new PlanFullCalendarView();
                    plan.id = item.Guid;
                    plan.title = item.Title;
                    plan.start = item.StartDate.Value.ToString("yyyy-MM-dd");
                    plan.end = item.EndDate.Value.ToString("yyyy-MM-dd");
                    list.Add(plan);
                }
            }
            return Json(new {data = list}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCalendarPlanDay(DateTime d1, DateTime d2)
        {
            if (d1 == d2)
            {
                d2 = d1.AddDays(1);
            }
            var uid = LoginUserService.ssoUserID;
            var query =
                unitOfWork.DPlan.Get(
                    p =>
                        p.StartDate < d2 && p.EndDate > d1 &&
                        (p.ExecutivesPerson == uid || p.Creator == uid || p.ManagerPerson.Contains(uid)) && p.isdel != 1);
            var list = new List<PlanFullCalendarView>();
            if (query != null)
            {
                foreach (var item in query)
                {
                    var plan = new PlanFullCalendarView();
                    plan.id = item.Guid;
                    plan.title = item.Title;
                    plan.start = item.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    plan.end = item.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss"); 
                    //plan.end = item.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                    list.Add(plan);
                }
            }
            return Json(new {data = list}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据用户id获得任务列表  创建、执行、抄送
        /// 根据用于ID获取其任务，然后将每个任务对应的最顶级的任务展示出来
        /// 2018年9月27日 修改为 如果有一条任务和我相关（我创建的、抄送给我的、我执行的） 那么该任务的所有子任务 我是都可以查看的
        /// 2018年9月28日 修改为 添加置顶功能的查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GetListByUid(DateTime dt0, DateTime dt1, int pageNo = 1)
        {
            if (dt0 == dt1)
            {
                dt1 = dt0.AddDays(1);
            }
            var uid = LoginUserService.ssoUserID;
            var list = new List<ScheduleView>();
            var query =
                unitOfWork.DPlan.Get(
                    p =>
                        (p.ExecutivesPerson == uid || p.Creator == uid || p.ManagerPerson.Contains(uid)) && p.isdel != 1 &&
                        (p.StartDate < dt1 && p.EndDate > dt0))
                    .OrderBy(p => p.Fid);
            if (query.Any())
            {
                foreach (var plan in query)
                {
                    var planview = Plan2ScheduleView(plan);
                    planview.IsTop = false;
                    list.Add(planview);
                }
                //置顶的任务列表
                var queryTop = unitOfWork.DPlanTop.Get(p => p.Creator == uid).OrderByDescending(p => p.CreateDate);
                if (queryTop.Any())
                {
                    foreach (var planView in list)
                    {
                        var s = queryTop.Where(p => p.PlanID == planView.ID);
                        if (s.Any())
                        {
                            planView.TopDateTime = s.FirstOrDefault().CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                            planView.IsTop = true;
                        }
                    }
                }
                list = list.OrderByDescending(p => p.TopDateTime).ThenByDescending(p=>p.CreateDate).ToList();
            }
            if (list.Any())
            {
                //暂时先把分页去掉
                //list = list.Skip((pageNo - 1)*20).Take(20).ToList();
            }
            return Json(new {list = list}, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据主任务uid获取子任务
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ActionResult GetSubPlanByUid(string uid)
        {
            var selfuid = LoginUserService.ssoUserID;
            var list = new List<ScheduleView>();
            var query = unitOfWork.DPlan.Get(p => p.ParentID == uid && p.isdel != 1);
            if (query != null && query.Any())
            {
                foreach (var plan in query)
                {
                    list.Add(Plan2ScheduleView(plan));
                }
            }
            //置顶的任务列表
            var queryTop = unitOfWork.DPlanTop.Get(p => p.Creator == selfuid).OrderByDescending(p => p.CreateDate);
            if (queryTop.Any())
            {
                foreach (var planView in list)
                {
                    var s = queryTop.Where(p => p.PlanID == planView.ID);
                    if (s.Any())
                    {
                        planView.TopDateTime = s.FirstOrDefault().CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                        planView.IsTop = true;
                    }
                }
            }
            list = list.OrderByDescending(p => p.TopDateTime).ThenByDescending(p => p.CreateDate).ToList();
            return Json(new { list = list }, JsonRequestBehavior.AllowGet);
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
                CreateDate = plan.CreateDate.Value.ToString("yyyy/MM/dd hh:mm"),
                StartDate = plan.StartDate.Value.ToString("yyyy/MM/dd hh:mm"),
                EndDate = plan.EndDate.Value.ToString("yyyy/MM/dd hh:mm"),
                TopDateTime = plan.TopDate == null ? "" : plan.TopDate.Value.ToString("yyyy/MM/dd HH:mm:ss"),
                Priority = plan.Priority == 0 ? "一般" : plan.Priority == 1 ? "紧急" : "特急",
                Completing = plan.Completing + "%",
                DoUserTrueName = DoUserTrueName,
                DoUser = plan.ExecutivesPerson,
                Creator = plan.Creator,
                CreatorTrueName = plan.CreatorTrueName,
                Tag = (plan.Creator == selfuid ? "发起者" : plan.ManagerPerson.Contains(selfuid) ? "抄送者" : plan.ExecutivesPerson == selfuid ? "执行者" : ""),
                CanEdit = plan.Creator == selfuid,
                CanCreateWorkLog = (plan.Creator == selfuid || plan.ExecutivesPerson == selfuid),
                CanCreateSub = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid,
                CanDelete = plan.Creator == selfuid,
                CanTop = plan.Creator == selfuid || plan.ExecutivesPerson == selfuid || plan.ManagerPerson.Contains(selfuid),
            };
            return schedule;
        }

        public List<PlanView> GetOrderPlanViews(List<Plan> plans)
        {
            var list=new List<PlanView>();
            if (plans != null && plans.Any())
            {
                foreach (var plan in plans)
                {
                    var planview = Plan2PlanView(plan);
                    for (var i = 1; i < plans.Count(); i++)
                    {
                        var planfather = plans[i];
                        var planfatherview= Plan2PlanView(planfather);
                        if (plan.Fid.StartsWith(planfather.Fid))
                        {
                            if (plan.Level == planfather.Level)
                            {
                                //同一级别
                            }
                            else
                            {
                                //父子关系
                                planfatherview.subworks.Add(planview);
                            }
                        }
                    }
                }
            }
            return null;
        } 


        public Plan GetTopParentPlan(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid).FirstOrDefault();
            if (query == null) return null;
            if (query.ParentID != "0")
            {
                return GetTopParentPlan(query.ParentID);
            }
            return query;
        }

        /// <summary>
        /// 根据群组获得任务列表
        /// </summary>
        /// <param name="gid">群id</param>
        /// <param name="dt0">开始时间</param>
        /// <param name="dt1">结束时间</param>
        /// <param name="pageNo">页码</param>
        /// <param name="pageSize">每页数据条数</param>
        /// <returns></returns>
        public ActionResult GetListByGroupid(string gid, DateTime dt0, DateTime dt1, int pageNo = 1,int pageSize=5)
        {
            if (dt0 == dt1)
            {
                dt1 = dt0.AddDays(1);
            }
            var selfuid = LoginUserService.ssoUserID;
            var list = new List<ScheduleView>();
            if (pageNo < 1)
                return Json(new {list = list}, JsonRequestBehavior.AllowGet);
            //gid = _groupid;
            var query =
                unitOfWork.DPlan.Get(
                    p => p.GroupID == gid && p.isdel != 1 && p.StartDate < dt1 && p.EndDate > dt0 && p.ParentID == "0")
                    .OrderByDescending(p => p.ID).ThenByDescending(p=>p.TopDate)
                    .Skip((pageNo - 1)* pageSize)
                    .Take(pageSize);
            if (query != null && query.Any())
            {
                foreach (var plan in query)
                {
                    var planview = Plan2ScheduleView(plan);
                    //SetSubWork(planview);
                    list.Add(planview);
                }
                //只有第一页才加载第一项的子任务
                if (pageNo == 1)
                {
                    var uid = list[0].ID;
                    var subplan = unitOfWork.DPlan.Get(p => p.ParentID == uid && p.isdel != 1).OrderByDescending(p => p.ID).ThenByDescending(p => p.TopDate);
                    if (subplan != null && subplan.Any())
                    {
                        foreach (var plan in subplan)
                        {
                            var planview = Plan2ScheduleView(plan);
                            list[0].SubSchedules.Add(planview);
                        }
                    }
                }
                //置顶的任务列表
                var queryTop = unitOfWork.DPlanTop.Get(p => p.Creator == selfuid).OrderByDescending(p => p.CreateDate);
                if (queryTop.Any())
                {
                    foreach (var planView in list)
                    {
                        var s = queryTop.Where(p => p.PlanID == planView.ID);
                        if (s.Any())
                        {
                            planView.TopDateTime = s.FirstOrDefault().CreateDate.Value.ToString("yyyy/MM/dd HH:mm:ss");
                            planView.IsTop = true;
                        }
                    }
                }
                list = list.OrderByDescending(p => p.TopDateTime).ThenByDescending(p => p.CreateDate).ToList();
            }
            return Json(new {list = list}, JsonRequestBehavior.AllowGet);
        }

        

        /// <summary>
        /// 递归查找子任务
        /// </summary>
        /// <param name="plan"></param>
        public void SetSubWork(PlanView plan)
        {
            var query = unitOfWork.DPlan.Get(p => p.ParentID == plan.uid && p.isdel == 0);
            if (query != null && query.Any())
            {
                foreach (var item in query)
                {
                    var planview = Plan2PlanView(item);
                    SetSubWork(planview);
                    plan.subworks.Add(planview);
                }
            }
        }



        public JsonResult CheckCanSetWorkFinished(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.ExecutivesPerson == LoginUserService.ssoUserID || query.Creator == LoginUserService.ssoUserID)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "您不是该任务的执行者，暂无权限设置任务状态！\n"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckCanCreateSubWork(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.ExecutivesPerson == LoginUserService.ssoUserID || query.Creator == LoginUserService.ssoUserID)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "您不是该任务的执行者，暂无权限创建子任务！\n"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckCanDeleteWork(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.Creator == LoginUserService.ssoUserID)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "您不是该任务的执行者，暂无权限删除任务！\n"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckCanEditWork(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.Creator == LoginUserService.ssoUserID)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "您不是该任务的创建者，暂无权限编辑任务！\n"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 是否有添加工作日志的权限 只有个创建人、执行人 才有该权限
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult CheckCanAddWorkLog(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.ExecutivesPerson == LoginUserService.ssoUserID || query.Creator == LoginUserService.ssoUserID)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "您不是该任务的创建者/执行者，暂无权限添加工作日志！\n"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }


        public JsonResult CheckCanTopWork(string uid)
        {
            var selfuid = LoginUserService.ssoUserID;
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.ExecutivesPerson == selfuid || query.Creator == selfuid || query.ManagerPerson.Contains(selfuid))
                {
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false, m = "权限不足，无法置顶工作任务！\n" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 是否有添加工作日志的权限 只有个创建人、执行人 才有该权限
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult CheckCanAddDeleteWorkLog(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                if (query.ExecutivesPerson == LoginUserService.ssoUserID || query.Creator == LoginUserService.ssoUserID)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "您不是该任务的创建者/执行者，暂无权限删除工作日志！\n"}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 任务置顶
        /// </summary>
        /// <param name="planid"></param>
        /// <returns></returns>
        public JsonResult SetWorkTop(string planid)
        {
            try
            {
                var selfuid = LoginUserService.ssoUserID;
                var list = new List<ScheduleView>();
                var query = unitOfWork.DPlan.Get(p => p.Guid==planid && p.isdel!=1).ToList();
                if (query.Any())
                {
                    var item =new PlanTop();
                    item.Guid= Guid.NewGuid().ToString();
                    item.PlanID = planid;
                    item.Creator = selfuid;
                    item.CreateDate=DateTime.Now;
                    unitOfWork.DPlanTop.Insert(item);
                    var result = unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(
                            new {
                                status = 0,
                                msg = "操作成功"
                            }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(
                       new
                       {
                           status = -1,
                           msg = "操作失败!"+result.Message,
                           data = list,
                       }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(
                       new
                       {
                           status = -1,
                           msg = "操作失败!未找到相关数据"
                       }, JsonRequestBehavior.AllowGet);
                }
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
        /// 取消任务置顶
        /// </summary>
        /// <param name="planid"></param>
        /// <returns></returns>
        public JsonResult DeleteWorkTop(string planid)
        {
            try
            {
                var selfuid = LoginUserService.ssoUserID;
                var list = new List<ScheduleView>();
                var query = unitOfWork.DPlanTop.Get(p => p.PlanID == planid && p.Creator == selfuid).ToList();
                if (query.Any())
                {
                    unitOfWork.DPlanTop.Delete(query);
                    var result = unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        return Json(
                            new
                            {
                                status = 0,
                                msg = "操作成功"
                            }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(
                       new
                       {
                           status = -1,
                           msg = "操作失败!" + result.Message,
                           data = list,
                       }, JsonRequestBehavior.AllowGet);
                }
                return Json(
                    new
                    {
                        status = -1,
                        msg = "操作失败!未找到相关数据"
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



        public JsonResult UploadWorkLogFileHfs()
        {
            var uploadfilename = Request.Files[0];
            var upload = UploadNew(uploadfilename);
            var result = Json(upload, JsonRequestBehavior.AllowGet);
            return result;
        }

        public JsonResult CreateWorkLogFile(string filename, string fileurl, string newfilename)
        {
            var item = new PlanFile();
            item.Guid = Guid.NewGuid().ToString();
            item.Creator = LoginUserService.ssoUserID;
            item.CreateTime = DateTime.Now;
            item.FileName = filename;
            var sindex = 0;
            try
            {
                sindex = filename.LastIndexOf(".");
            }
            catch (Exception ex)
            {
                sindex = 0;
            }
            item.FileExtension = filename.Substring(sindex + 1);
            item.NewFileName = newfilename;
            item.FileUrl = fileurl;
            unitOfWork.DPlanFile.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new {r = true, fileid = item.ID}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置任务为完成状态
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult SetWorkFinished(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                query.Completing = 100;
                query.CompleteDate = DateTime.Now;

                //添加一条任务日志
                var log = new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = LoginUserService.ssoUserID;
                log.CreateDate = DateTime.Now;
                log.Content = "点击确认，完成任务";
                log.PlanID = query.ID;
                log.PlanGuid = query.Guid;
                log.CurProgress = 100;
                unitOfWork.DPlanProgress.Insert(log);
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetWorkUnfinished(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                var querylog =
                    unitOfWork.DPlanProgress.Get(p => p.PlanGuid == query.Guid && p.IsDel != 1)
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
                unitOfWork.DPlan.Update(query);
                //记录任务日志
                var log = new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = LoginUserService.ssoUserID;
                log.CreateDate = DateTime.Now;
                log.Content = "点击取消确认，取消完成任务";
                log.PlanID = query.ID;
                log.PlanGuid = query.Guid;
                log.CurProgress = lastcompelting;
                unitOfWork.DPlanProgress.Insert(log);

                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true, completing = lastcompelting}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);

            }

            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 设置子任务为完成状态 暂时没用到
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult SetSubWorkFinished(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                query.Completing = 100;
                query.CompleteDate = DateTime.Now;
                query.CompleteBZ = "首页点击确认子任务完成";
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 创建项目视图、创建子项目视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Create(string gid = "0", string fatherId = "")
        {
            ViewBag.fathertitle = "";
            ViewBag.ManagerPerson = "";
            if (fatherId != "")
            {
                var query = unitOfWork.DPlan.Get(p => p.Guid == fatherId && p.isdel != 1).FirstOrDefault();
                ViewBag.fathertitle = query.Title;
                gid = query.GroupID;
                //新创建的子任务的抄送人是父任务的创建者和执行者，两者可能为同一个人，去重
                ViewBag.ManagerPerson = query.Creator == query.ExecutivesPerson
                    ? query.Creator + ";"
                    : query.Creator + ";" + query.ExecutivesPerson + ";";
            }
            ViewBag.fatherid = fatherId;
            ViewBag.groupid = gid;
            return View();
        }

        public ActionResult CreateWorkLog(string uid)
        {
            ViewBag.uid = uid;
            return View();
        }

        public JsonResult CreateWorkLogData(string uid, int completing, string logContent, string files)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                query.Completing = completing;
                if (completing == 100)
                {
                    query.CompleteDate = DateTime.Now;
                }
                unitOfWork.DPlan.Update(query);
                var querylog = new PlanProgress();
                querylog.Guid = Guid.NewGuid().ToString();
                ;
                querylog.Content = logContent;
                querylog.CreateDate = DateTime.Now;
                querylog.Creator = LoginUserService.ssoUserID;
                querylog.CurProgress = completing;
                querylog.PlanGuid = uid;
                querylog.PlanID = query.ID;
                querylog.Files = files;
                unitOfWork.DPlanProgress.Insert(querylog);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetWorkDetail(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                return Json(new {r = true, data = query}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false}, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 获取创建者的头像和真实姓名
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCreator()
        {
            return Json(
                new
                {
                    photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID,
                    realname = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName
                }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 选择成员界面
        /// </summary>
        /// <param name="type"></param>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public ActionResult ChooseMember(int type, string groupid = "0")
        {
            /*
             * type=0  选择抄送人 可以选择多个
             * type=1  选择执行人 只能选择一个
             * groupid 不为空的时候 选人范围为群组的成员 为0的时候 选择范围为全部人员             * 
             */
            ViewBag.groupid = groupid;
            ViewBag.type = type;
            return View();
        }

        public ActionResult ChooseMemberNew(int type, string groupid = "0")
        {
            /*
             * type=0  选择抄送人 可以选择多个
             * type=1  选择执行人 只能选择一个
             * groupid 不为空的时候 选人范围为群组的成员 为0的时候 选择范围为全部人员             * 
             */
            ViewBag.groupid = groupid;
            ViewBag.type = type;
            return View();
        }

        

        public JsonResult GetChoosenMembers(string ids)
        {
            var list = new List<DeptUser>();
            if (!string.IsNullOrEmpty(ids))
            {
                var arr = ids.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var id in arr)
                {
                    var user = ssoUserOfWork.GetUserByID(id);
                    if (user == null) continue;
                    list.Add(user);
                }
                return Json(new {count = list.Count, members = list}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {count = 0, members = list}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateWork(Plan item)
        {
            if (item.StartDate == null)
            {
                item.StartDate = DateTime.Now.Date;
            }
            if (item.EndDate == null)
            {
                item.EndDate = DateTime.Now.AddDays(1).Date;
            }
            if (item.ManagerPerson == null)
            {
                item.ManagerPerson = "";
            }
            item.CreateDate = DateTime.Now;
            item.Creator = LoginUserService.ssoUserID;
            item.Guid = Guid.NewGuid().ToString();
            item.Completing = 0;
            item.CompleteBZ = "";
            item.Adress = "";
            item.isTop = 0;
            item.isdel = 0;
            //item.GroupID = _groupid;
            if (string.IsNullOrEmpty(item.ParentID))
                item.ParentID = "0"; //非子任务 parentid都设置为0
            item.Level = GetLevel(item.ParentID);
            item.Fid = GetFid(item.ParentID);
            unitOfWork.DPlan.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new {r = true}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 编辑任务 只有是任务的创建者 才可以编辑
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ActionResult Edit(string uid)
        {
            ViewBag.uid = uid;
            return View();
        }

        public ActionResult EditWork(Plan plan)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == plan.Guid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                var list = new List<string>();
                list.AddRange(GetProperties(plan));
                unitOfWork.DPlan.Update(query, plan, list);
                
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success|| result.ResultType == OperationResultType.NoChanged)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public static List<string> GetProperties(object O)
        {
            List<string> list = new List<string>();
            try
            {
                PropertyInfo[] propertyInfo = O.GetType().GetProperties();
                for (int i = 0; i < propertyInfo.Length; i++)
                {
                    var name = propertyInfo[i].Name;
                    list.Add(name);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return list;
        }



        /// <summary>
        /// 获取抄送人
        /// </summary>
        /// <param name="uids"></param>
        /// <returns></returns>
        public JsonResult GetCopyToUsers(string uids)
        {
            var copytousers = new List<CopyToUser>();
            var copytouserlist = uids.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in copytouserlist)
            {
                var ssoUser = ssoUserOfWork.GetUserByID(s);
                if (ssoUser == null) continue;
                var copytouser = new CopyToUser();
                copytouser.uid = s;
                copytouser.name = ssoUser.RealName;
                copytouser.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + s;
                copytousers.Add(copytouser);
            }
            return Json(new {data = copytousers}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDoUsers(string uid)
        {
            var ssoUser = ssoUserOfWork.GetUserByID(uid);
            
            var douser = new CopyToUser();
            douser.uid = uid;
            douser.name = ssoUser==null?"":ssoUser.RealName;
            douser.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + uid;
            return Json(new {data = douser}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Finish(string id)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == id && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                query.Completing = 100;
                query.CompleteDate = DateTime.Now;
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == id && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                query.isdel = 1;
                //删除子元素
                var querysub = unitOfWork.DPlan.Get(p => p.Fid.StartsWith(query.Fid) && p.isdel != 1);
                foreach (var plan in querysub)
                {
                    plan.isdel = 1;
                    unitOfWork.DPlan.Update(plan);
                }
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }





        public JsonResult GetPlanDetail(string uid)
        {
            var canEdit = true;
            var canCreateWorkLog= false;
            ViewBag.uid = uid;
            var creatorPhoto = "";
            var creatorName = "";
            ;
            var fatherTitle = "无";
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                creatorPhoto = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + query.Creator;
                creatorName = query.CreatorTrueName;
                var queryfather = unitOfWork.DPlan.Get(p => p.Guid == query.ParentID).FirstOrDefault();
                if (queryfather != null)
                {
                    fatherTitle = queryfather.Title;
                }
                if (query.Creator == LoginUserService.ssoUserID) canEdit = false;
                if (query.Creator == LoginUserService.ssoUserID||query.ExecutivesPerson==LoginUserService.ssoUserID) canCreateWorkLog = true;
                if (query.ManagerPerson == null)
                {
                    query.ManagerPerson = "";
                }


                return Json(new
                {
                    r = true,
                    data = query,
                    creatorPhoto = creatorPhoto,
                    creatorName = creatorName,
                    fatherTitle = fatherTitle,
                    canEdit = canEdit,
                    canCreateWorkLog = canCreateWorkLog,
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPlanLogs(string uid)
        {
            var list = new List<PlanLogView>();
            var query =
                unitOfWork.DPlanProgress.Get(p => p.PlanGuid == uid && p.IsDel != 1)
                    .OrderByDescending(p => p.CreateDate);
            if (query != null)
            {
                foreach (var planProgress in query)
                {
                    var item = new PlanLogView();
                    item.Planlog = planProgress;
                    if (!string.IsNullOrEmpty(planProgress.Files))
                    {
                        var fileidslist = planProgress.Files.Split(new char[] {';'},
                            StringSplitOptions.RemoveEmptyEntries);
                        foreach (var fileid in fileidslist)
                        {
                            var id = Convert.ToInt32(fileid);
                            var queryplanfile = unitOfWork.DPlanFile.Get(p => p.ID == id).FirstOrDefault();
                            if (queryplanfile != null)
                            {
                                var hfsfile = new HfsFileView();
                                hfsfile.ID = queryplanfile.ID.ToString();
                                hfsfile.Title = queryplanfile.FileName;
                                hfsfile.LiteratureId = queryplanfile.NewFileName;
                                hfsfile.FileExtension = queryplanfile.FileExtension;
                                item.PlanHfsFiles.Add(hfsfile);
                                item.PlanFiles.Add(queryplanfile);
                            }
                        }
                    }
                    item.Planlog.Content = item.Planlog.Content.Replace("\r", "<br />");
                    item.Planlog.Content = item.Planlog.Content.Replace("\n", "<br />");
                    list.Add(item);
                }
                return Json(new
                {
                    r = true,
                    data = list
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AddPlanDiscuss(string uid, string planid, string title = "")
        {
            var item = new PlanDiscuss();
            item.CreateTime = DateTime.Now;
            item.Creator = LoginUserService.ssoUserID;
            item.DiscussID = uid;
            item.DiscussTitle = title;
            item.Guid = Guid.NewGuid().ToString();
            item.PlanID = planid;
            item.Url = "/discuss/DiscussAdd/Index?did=" + uid + "&from=todolist";
            item.Type = "协同研讨";
            unitOfWork.DPlanDiscuss.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new {r = true}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);

        }

        public JsonResult AddPlanCooperation(string uid, string planid, string title = "")
        {
            var item = new PlanDiscuss();
            item.CreateTime = DateTime.Now;
            item.Creator = LoginUserService.ssoUserID;
            item.DiscussID = uid;
            item.DiscussTitle = title;
            item.Guid = Guid.NewGuid().ToString();
            item.PlanID = planid;
            item.Url = "/Creation#/cooperative/CreationInfo?id=" + uid;
            item.Type = "协作文档";
            unitOfWork.DPlanDiscuss.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new {r = true}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddPlanProject(string uid, string planid, string title = "")
        {
            var item = new PlanDiscuss();
            item.CreateTime = DateTime.Now;
            item.Creator = LoginUserService.ssoUserID;
            item.DiscussID = uid;
            item.DiscussTitle = title;
            item.Guid = Guid.NewGuid().ToString();
            item.PlanID = planid;
            item.Url = "/rpms#/project/" + uid;
            item.Type = "协同研究";
            unitOfWork.DPlanDiscuss.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new {r = true}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPlanDiscusses(string planid)
        {
            var query = unitOfWork.DPlanDiscuss.Get(p => p.IsDel != 1 && p.PlanID == planid).ToList();
            var client = new RpmsServiceClient();
            //ProjectView_PMC result = null;
            //try
            //{
            //    result = client.GetProjectInfo("userId", "projectId");
            //    client.Close();
            //}
            //catch (Exception ex)
            //{
            //    client.Abort();
            //}
            return Json(new {data = query}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPlanProject(string projectid)
        {
            var client = new RpmsServiceClient();
            ProjectView_PMC result = null;
            var errorMsg = "";
            try
            {
                result = client.GetProjectInfo(LoginUserService.ssoUserID, projectid);
                client.Close();
            }
            catch (Exception ex)
            {
                errorMsg = ex.ToString();
                client.Abort();
            }
            return Json(new {data = result, ErrorMsg = errorMsg}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteWorkLog(string uid)
        {
            var planGuid = "";
            var query = unitOfWork.DPlanProgress.Get(p => p.Guid == uid && p.IsDel != 1).FirstOrDefault();
            if (query != null)
            {
                planGuid = query.PlanGuid;
                query.IsDel = 1;
                unitOfWork.DPlanProgress.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    //获取这条记录之前的最新的工作日志 修改齐对应的工作任务的完成度的百分比
                    var queryplan = unitOfWork.DPlan.Get(p => p.Guid == planGuid && p.isdel != 1).FirstOrDefault();
                    if (queryplan != null)
                    {
                        var querylogs =
                            unitOfWork.DPlanProgress.Get(p => p.PlanGuid == planGuid && p.IsDel != 1)
                                .OrderByDescending(p => p.CreateDate);
                        if (querylogs != null && querylogs.Any())
                        {
                            queryplan.Completing = querylogs.ToList()[0].CurProgress;
                        }
                        else
                        {
                            queryplan.Completing = 0;
                        }
                        unitOfWork.DPlan.Update(queryplan);
                        result = unitOfWork.Save();
                        if (result.ResultType == OperationResultType.Success)
                        {
                            return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                        }
                        return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
                    }
                }
                return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "未查询到对应的数据"}, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 根据parentID获取level
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public int GetLevel(string id)
        {
            if (id == "0") return 0;
            var query = unitOfWork.DPlan.Get(p => p.Guid == id).FirstOrDefault();
            if (query != null) return (query.Level + 1);
            return 0;
        }

        public string GetFid(string id)
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
            count = queryParent.Count();
            if (count < 9)
            {
                return queryParent.Fid + "0" + (count + 1) + ".";
            }
            return queryParent.Fid + (count + 1) + ".";
        }

        public PlanView Plan2PlanView(Plan plan)
        {
            var uid = LoginUserService.ssoUserID;
            var ssoUser = ssoUserOfWork.GetUserByID(plan.ExecutivesPerson);
            var planview = new PlanView();
            planview.fid = plan.Fid;
            planview.level = plan.Level;
            planview.id = plan.ID;
            planview.uid = plan.Guid;
            planview.title = plan.Title;
            planview.timespan = plan.StartDate.Value.ToString("MM/dd") + "-" +
                                plan.EndDate.Value.ToString("MM/dd");
            planview.completing = plan.Completing + "%";
            planview.priority = plan.Priority.Value.ToString();
            planview.truename0 = plan.CreatorTrueName;
            planview.truename1 = ssoUser==null?"": ssoUser.RealName;
            if (plan.Creator == uid)
            {
                planview.relationship = "<i class=\"skdb-txt-fb\"></i>";
            }
            if (plan.ExecutivesPerson == uid)
            {
                planview.relationship = "<i class=\"skdb-txt-zx\"></i>";
            }
            if (plan.ManagerPerson != null && plan.ManagerPerson.Contains(uid))
            {
                planview.relationship = "<i class=\"skdb-txt-cs\"></i>";
            }
            return planview;
        }



        public ActionResult UploadNew(HttpPostedFileBase file, string uploadFileType = "ZK-700")
        {
            string fileCode;
            var fileName = Path.GetFileName(file.FileName);
            var result = _networkDiskService.UploadFile(fileName, uploadFileType, file.InputStream.Length,
                file.InputStream);
            fileCode = ((UploadRetMsg) result).FileCode;
            if (fileCode == string.Empty)
            {
                return Json(new
                {
                    Code = HttpStatusCode.InternalServerError,
                    Data = new
                    {
                        Result = false,
                        FileCode = string.Empty,
                        ImagePath = ""
                    },
                    Error = "文件上传失败！"

                }, JsonRequestBehavior.AllowGet);
            }
            string convertTaskId;
            result = _fileConvertService.FileConvert(fileCode, out convertTaskId);
            return Json(new
            {
                Code = HttpStatusCode.OK,
                Data = new
                {
                    Result = result,
                    FileCode = fileCode,
                    ImagePath =
                        string.IsNullOrEmpty(fileCode)
                            ? ""
                            : string.Format(ConfigHelper.GetConfigString("HfsUrl"), fileCode),
                    ConvertTaskId = string.IsNullOrEmpty(convertTaskId) ? 0 : Convert.ToInt32(convertTaskId)
                }
            });
        }



        /// <summary>
        /// 暂时没用到
        /// </summary>
        /// <param name="fileCode"></param>
        /// <param name="tableName"></param>
        /// <param name="kType"></param>
        /// <param name="dbCode"></param>
        /// <returns></returns>
        public ActionResult PdfRead(string fileCode, string tableName = "local", string kType = "local",
            string dbCode = "local")
        {
            if (string.IsNullOrEmpty(fileCode))
            {
                throw new ArgumentNullException("fileCode");
            }
            //非cnki的改后缀，cnki的保持原样
            if (!"cnki".Equals(kType, StringComparison.OrdinalIgnoreCase))
            {
                fileCode = ((Path.GetExtension(fileCode) == ".caj" || Path.GetExtension(fileCode) == ".nh")
                    ? fileCode
                    : Path.GetFileNameWithoutExtension(fileCode) + ".pdf");
            }

            if ("CJFQ".Equals(dbCode, StringComparison.OrdinalIgnoreCase))
            {
                dbCode = "CJFQ";
            }

            var taskType = "cnki".Equals(kType, StringComparison.OrdinalIgnoreCase) ? 0 : 1;

            dbCode = "cnki".Equals(kType, StringComparison.OrdinalIgnoreCase) ? dbCode : "local";
            tableName = "cnki".Equals(kType, StringComparison.OrdinalIgnoreCase) ? tableName : "local";

            //本地文档需要传文件名 中心网站的文档不需要
            var hfsFileCode = taskType == 0 ? "" : fileCode;
            var userIp = WebUtils.GetUserIPAddress();
            var returnValue = _okmsProxyServiceService.GetHfsFileInfo(dbCode, fileCode, tableName, userIp,
                LoginUserService.User.UnitID, hfsFileCode, taskType);
            if (returnValue.Status > 0)
            {
                return
                    Json(new HandlerResult(HttpStatusCode.OK,
                        new {dbCode = dbCode, tableName = tableName, status = returnValue.Status}));
            }

            if (returnValue.Status == -1)
            {
                throw new ArgumentException(returnValue.Msg);
            }

            var fileCacheKey = Path.GetFileNameWithoutExtension(returnValue.Info.HfsFileCode);

            var chapterList = new List<PdfChapter>(); //returnValue.Info.Chapters;
            for (int i = 0; i < returnValue.Info.PageCount; i++)
            {
                var imageBasePath = returnValue.Info.HfsFileCode.Substring(0,
                    returnValue.Info.HfsFileCode.LastIndexOf('.'));
                PdfChapter chapter = new PdfChapter()
                {
                    ImageName = string.Format("{0}{1}{2}{3}", imageBasePath, "_", i + 1, ".jpg")
                };
                chapterList.Add(chapter);
            }
            var pageIndex = 1;
            var pageCount = returnValue.Info.PageCount;

            if (pageIndex < 1 || pageCount < pageIndex)
            {
                pageIndex = 1;
            }
            return
                Json(new HandlerResult(HttpStatusCode.OK,
                    new
                    {
                        dbCode = dbCode,
                        tableName = tableName,
                        status = returnValue.Status,
                        chpter = chapterList,
                        hfsFileCode = hfsFileCode,
                        taskType = taskType,
                        pageIndex = pageIndex,
                        pageMax = pageCount
                    }));
        }


        /// <summary>
        /// 等待下载页面循环调用，取得下载的状态和进度 暂时没用到
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dbCode"></param>
        /// <param name="tableName"></param>
        /// <param name="taskType"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult HfsFileCode(string dbCode, string tableName, string fileName, int taskType = 0)
        {
            int status = -1;
            float percentage = 0;
            string msg = string.Empty;

            var userIp = WebUtils.GetUserIPAddress();
            var downloadStatus = _okmsProxyServiceService.GetDownloadStatusForProxy(dbCode, fileName, userIp,
                LoginUserService.User.UnitID, taskType);
            status = downloadStatus.Status;
            msg = downloadStatus.Msg;
            if (downloadStatus.TotalLength == 0)
            {
                percentage = 0;
            }
            else
            {
                percentage = (float) downloadStatus.DownloadedLength/downloadStatus.TotalLength;
            }
            return
                Json(new HandlerResult(HttpStatusCode.OK,
                    new {status = status, msg = msg, percentage = ((int) (percentage*100)).ToString() + "%"}));
        }

        public ActionResult GetKnowledgeContent(string fileCode)
        {
            var content = string.Empty;
            fileCode = Path.GetFileNameWithoutExtension(fileCode) + ".xml";
            var url = string.Format("{0}/static/resource/", ConfigHelper.GetConfigString("Discuss"));
            content = _fileConvertService.GetFileContentWithCustomUrl(fileCode, url);
            return Json(new HandlerResult(HttpStatusCode.OK, content));
        }

        [ValidateInput(false)]
        public ActionResult ReadOnline(string filecode, int fileType)
        {
            ViewBag.filecode = filecode;
            ViewBag.fileType = fileType;
            return View();
        }

        static DateTime ConvertTime(long time)
        {
            DateTime timeStamp = new DateTime(1970, 1, 1); //得到1970年的时间戳
            long t = (time + 8*60*60*1000)*10000 + timeStamp.Ticks;
            DateTime dt = new DateTime(t);
            return dt;
        }


        /// <summary>
        /// 更新三类项目的标题内容
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public JsonResult UpdateWorkDiscuss(string uid, string title)
        {
            var query = unitOfWork.DPlanDiscuss.Get(p => p.DiscussID == uid).FirstOrDefault();
            if (query != null)
            {
                query.DiscussTitle = title;
                unitOfWork.DPlanDiscuss.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new {r = true}, JsonRequestBehavior.AllowGet);
                }
                return Json(new {r = false, m = "操作失败！\n" + result.Message}, JsonRequestBehavior.AllowGet);
            }
            return Json(new {r = false, m = "操作失败！未找到对应的任务数据"}, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 根据UID删除三类项目
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public JsonResult DeleteWorkDiscuss(string uid)
        {
            var query = unitOfWork.DPlanDiscuss.Get(p => p.Guid == uid).FirstOrDefault();
            if (query != null)
            {
                query.IsDel = 1;
                unitOfWork.DPlanDiscuss.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }
    }
}


