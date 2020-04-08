using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Edu.Entity;
using Edu.Models.Models;
using Edu.Service;
using Edu.Tools;
using KNet.AAMS.Foundation.Model;
using KNet.Data.Entity;

namespace Edu.Web.Controllers
{
    public class GroupToDoListController : BaseControl
    {
        // GET: GroupToDoList
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 根据用户id获得任务列表
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ActionResult GetListByUid(string uid,DateTime dt)
        {
            var list = new List<PlanView>();
            var query = unitOfWork.DPlan.Get(p => p.ExecutivesPerson == uid&&p.StartDate<=dt&&p.EndDate>=dt&&p.ParentID== "0" && p.isdel != 1);
            if (query != null && query.Any())
            {
                foreach (var plan in query)
                {
                    list.Add(Plan2PlanView(plan));
                }
            }
            return Json(new { list = list }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 根据主任务uid获取子任务
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public ActionResult GetSubtasksByUid(string uid)
        {
            var list = new List<PlanView>();
            var query = unitOfWork.DPlan.Get(p => p.ParentID == uid && p.isdel != 1);
            if (query != null && query.Any())
            {
                foreach (var plan in query)
                {
                    list.Add(Plan2PlanView(plan));
                }
            }
            return Json(new { list = list }, JsonRequestBehavior.AllowGet);
        }
        public string groupid = "0";
        /// <summary>
        /// 根据群组获得任务列表
        /// </summary>
        /// <param name="gid">群id</param>
        /// <param name="dt0">开始时间</param>
        /// <param name="dt1">结束时间</param>
        /// <returns></returns>
        public ActionResult GetListByGroupid(string gid,DateTime dt0,DateTime dt1)
        {
            gid = groupid;
            var list = new List<PlanView>();
            var query = unitOfWork.DPlan.Get(p => p.GroupID == gid && p.isdel != 1 && p.StartDate<=dt1 &&p.EndDate>=dt0 && p.ParentID=="0");


            if (query != null && query.Any())
            {
                foreach (var plan in query)
                {
                    var planview = Plan2PlanView(plan);
                    SetSubWork(planview);
                    list.Add(planview);
                }
            }
            return Json(new { list = list }, JsonRequestBehavior.AllowGet);
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
                query.CompleteDate=DateTime.Now;
                
                //添加一条任务日志
                var log=new PlanProgress();
                log.Guid = Guid.NewGuid().ToString();
                log.Creator = LoginUserService.ssoUserID;
                log.CreateDate = DateTime.Now;
                log.Content = "点击确认，完成任务";
                log.PlanID = query.ID;
                log.PlanGuid = query.Guid;
                log.CurProgress = 100;
                unitOfWork.DPlanProgress.Insert(log);
                //子任务
                var querysub= unitOfWork.DPlan.Get(p => p.ParentID == uid && p.isdel != 1);
                foreach (var plan in querysub)
                {
                    plan.Completing = 100;
                    plan.CompleteDate=DateTime.Now;
                    plan.CompleteBZ = "父任务完成->子任务完成";

                    var sublog = new PlanProgress();
                    sublog.Guid = Guid.NewGuid().ToString();
                    sublog.Creator = LoginUserService.ssoUserID;
                    sublog.CreateDate = DateTime.Now;
                    sublog.Content = "点击确认父任务完成->子任务完成";
                    sublog.PlanID = plan.ID;
                    sublog.PlanGuid = plan.Guid;
                    sublog.CurProgress = 100;
                    unitOfWork.DPlanProgress.Insert(sublog);

                    unitOfWork.DPlan.Update(plan);
                }
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetWorkUnfinished(string uid)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == uid && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                var querylog =
                    unitOfWork.DPlanProgress.Get(p => p.PlanGuid == query.Guid).OrderByDescending(p => p.CreateDate);
                var lastcompelting = 0;
                if (querylog.Any())
                {
                    lastcompelting = querylog.ToList()[0].CurProgress;
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
                //子任务
                var querysub = unitOfWork.DPlan.Get(p => p.ParentID == uid && p.isdel != 1);
                foreach (var plan in querysub)
                {
                    var querysublog =
                    unitOfWork.DPlanProgress.Get(p => p.PlanGuid == query.Guid).OrderByDescending(p => p.CreateDate);
                    var lastcompeltingsub = 0;
                    if (querysublog.Any())
                    {
                        lastcompeltingsub = querylog.ToList()[0].CurProgress;
                    }
                    //更新子任务的完成状态
                    plan.Completing = lastcompeltingsub;
                    plan.CompleteDate = null;
                    //记录子任务的完成记录
                    var sublog = new PlanProgress();
                    sublog.Guid = Guid.NewGuid().ToString();
                    sublog.Creator = LoginUserService.ssoUserID;
                    sublog.CreateDate = DateTime.Now;
                    sublog.Content = "点击取消父任务完成->子任务取消完成";
                    sublog.PlanID = plan.ID;
                    sublog.PlanGuid = plan.Guid;
                    sublog.CurProgress = lastcompeltingsub;
                    unitOfWork.DPlanProgress.Insert(sublog);
                    unitOfWork.DPlan.Update(plan);
                }
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);

            }

            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 创建项目视图
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return View();
        }
        /// <summary>
        /// 获取创建者的头像和真实姓名
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCreator()
        {
            return Json(
                new
                {   photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID,
                    realname = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName
                }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateSub(string fatherId)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == fatherId && p.isdel != 1).FirstOrDefault();
            ViewBag.fatherid = fatherId;
            ViewBag.fathertitle = query.Title;
            return View();
        }

        public ActionResult ChooseMember(string ids,int type)
        {
            ViewBag.ids = ids;
            ViewBag.type = type;
            return View();
        }

        public JsonResult GetChoosenMembers(string ids)
        {
            var list = new List<DeptUser>();
            if (!string.IsNullOrEmpty(ids))
            {
                var arr = ids.Split(';');
                foreach (var id in arr)
                {
                    var user = ssoUserOfWork.GetUserByID(id);
                    if (user != null)
                    {
                        list.Add(user);
                    }
                }
                return Json(new { count = list.Count, members = list }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { count = 0, members = list }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreateWork(Plan item)
        {
            item.CreateDate=DateTime.Now;
            item.Creator = LoginUserService.ssoUserID;
            item.Guid= Guid.NewGuid().ToString();
            item.Completing = 0;
            item.CompleteBZ = "";
            item.Adress = "";
            item.GroupID = "";
            item.isTop = 0;
            item.isdel = 0;
            item.GroupID = groupid;
            if(string.IsNullOrEmpty(item.ParentID))
                item.ParentID = "0";//非子任务 parentid都设置为0
            item.Level = GetLevel(item.ParentID);
            item.Fid = GetFid(item.ParentID);
            unitOfWork.DPlan.Insert(item);
            var result = unitOfWork.Save();
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
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
        public ActionResult EditWork(Plan plan,string planprogress)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == plan.Guid&& p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                var list = new List<string>();
                list.AddRange(GetProperties(plan));

                //添加日志记录
                var tempstr = "编辑操作,未添加日志记录。";
                if (!string.IsNullOrEmpty(planprogress))
                {
                    tempstr = planprogress;
                }
                var p = new PlanProgress();
                p.Guid = Guid.NewGuid().ToString();
                p.Creator = LoginUserService.ssoUserID;
                p.CreateDate = DateTime.Now;
                p.Content = tempstr;
                p.PlanID = plan.ID;
                p.PlanGuid = plan.Guid;
                p.CurProgress = plan.Completing;
                unitOfWork.DPlanProgress.Insert(p);


                unitOfWork.DPlan.Update(query,plan, list);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
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


       

        public JsonResult GetCopyToUsers(string uids)
        {
            var copytousers = new List<CopyToUser>();
            var copytouserlist = uids.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in copytouserlist)
            {
                var ssoUser = ssoUserOfWork.GetUserByID(s);
                if (ssoUser != null)
                {
                    var copytouser = new CopyToUser();
                    copytouser.uid = s;
                    copytouser.name = ssoUser.RealName;
                    copytouser.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + s;
                    copytousers.Add(copytouser);
                }
            }
            return Json( new { data = copytousers }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDoUsers(string uid)
        {
            var ssoUser = ssoUserOfWork.GetUserByID(uid);
            var name = "";
            if (ssoUser != null)
            {
                name = ssoUser.RealName;
            }
            var douser = new CopyToUser();
            douser.uid = uid;
            douser.name = name;
            douser.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + uid;
            return Json(new { data = douser }, JsonRequestBehavior.AllowGet);
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
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string id)
        {
            var query = unitOfWork.DPlan.Get(p => p.Guid == id && p.isdel != 1).FirstOrDefault();
            if (query != null)
            {
                query.isdel = 1;
                //删除子元素
                var querysub = unitOfWork.DPlan.Get(p => p.ParentID == id && p.isdel != 1);
                foreach (var plan in querysub)
                {
                    plan.isdel = 1;
                    unitOfWork.DPlan.Update(plan);
                }
                unitOfWork.DPlan.Update(query);
                var result = unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new { r = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { r = false, m = "操作失败！\n" + result.Message }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Detail(string uid)
        {
            ViewBag.uid = uid;
            return View();
        }


        public JsonResult GetPlanDetail(string uid)
        {
            ViewBag.uid = uid;
            var creatorPhoto = "";
            var creatorName = ""; ;
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
                return Json(new
                {
                    r = true,
                    data = query,
                    creatorPhoto = creatorPhoto,
                    creatorName = creatorName,
                    fatherTitle = fatherTitle
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetPlanLogs(string uid)
        {
            var query = unitOfWork.DPlanProgress.Get(p => p.PlanGuid == uid).OrderByDescending(p=>p.CreateDate);
            if (query != null)
            {
                return Json(new
                {
                    r = true,
                    data = query
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, m = "操作失败！未找到对应的任务数据" }, JsonRequestBehavior.AllowGet);
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
            var truename1 = "";
            var ssoUser = ssoUserOfWork.GetUserByID(plan.ExecutivesPerson);
            if (ssoUser != null)
            {
                truename1 = ssoUser.RealName;
            }
            var planview=new PlanView();
            planview.id = plan.ID;
            planview.uid = plan.Guid;
            planview.title = plan.Title;
            planview.timespan=plan.StartDate.Value.ToString("MM/dd") + "-"+
            plan.EndDate.Value.ToString("MM/dd");
            planview.completing = plan.Completing + "%";
            planview.priority = plan.Priority.Value.ToString();
            planview.truename0 = plan.CreatorTrueName;
            planview.truename1 = truename1;
            return planview;
        }

        
    }

    
}