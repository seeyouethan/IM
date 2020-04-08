using Edu.Entity;
using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Edu.Tools;
using Edu.Models;

namespace Edu.Web.Areas.Admin.Controllers
{
	 public class UserRoleController : AdminBaseController
    {
        #region 列表
        // GET: Admin/UserRole
        public ActionResult Index(DateTime? startT, DateTime? endT, string sn = "", int pageNo = 1)
        {
		    Paging paging = new Paging();
            paging.PageNumber = pageNo;
            var query = unitOfWork.DUserRole.GetIQueryable(p => p.Name.Contains(sn) && p.States == (int)EnumState.Normal);
            if (startT != null)
            {
                query = query.Where(p => p.CreateDate >= startT);
            }
            if (endT != null)
            {
                query = query.Where(p => p.CreateDate <= endT);
            }
            paging.Amount = query.Count();
            paging.EntityList = query.OrderByDescending(p => p.CreateDate).Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).ToList();
            ViewBag.sn = sn;
            ViewBag.startT = startT;
            ViewBag.endT = endT;
            ViewBag.PageNo = pageNo;//页码
            ViewBag.PageCount = paging.PageCount;//总页数
            return View(paging.EntityList);
        }
        #endregion

        #region 增加修改
        public ActionResult GetOp(int? id)
        {
            var query = unitOfWork.DUserRole.GetByID(id);
            return PartialView("_Op", query);
        }
        [HttpPost]
        [ValidateInput(false)]
        [DisabledReSubmitAction]
        public ActionResult Mody(UserRole entity)
        {
            OperationResult result = null;
            var old = unitOfWork.DUserRole.GetByID(entity.ID);
            List<string> list = new List<string>();
            list.AddRange(this.Request.Form.AllKeys.AsEnumerable());
            if (old == null)
            {
                entity.CreateDate = DateTime.Now;
                entity.Creator = LoginUserService.UserID;
                entity.States = (int)EnumState.Normal;
                unitOfWork.DUserRole.Insert(entity);
                result = unitOfWork.Save();
                new DBLogService().insert(ActionClick.Add, entity);
            }
            else
            {
                unitOfWork.DUserRole.Update(old, entity, list);
                result = unitOfWork.Save();
                new DBLogService().insert(ActionClick.Mody, entity);
            }
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败\n" + result.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion

        #region 查询
        public ActionResult LoadSearch(DateTime? startT, DateTime? endT, string sn = "")
        {
            ViewBag.sn = sn;
            ViewBag.startT = startT;
            ViewBag.endT = endT;
            return PartialView("_Search");
        }
        #endregion 

        #region 删除
        public ActionResult DeleteOp(int id)
        {
            var old = unitOfWork.DUserRole.GetByID(id);
            unitOfWork.DUserRole.Delete(old);
            var result = unitOfWork.Save();

            if (result.ResultType == OperationResultType.Success)
            {
                new DBLogService().insert(ActionClick.Delete, old);
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败\n" + result.Message }, JsonRequestBehavior.AllowGet);
            } 
          
        }
        #endregion

    }

}
 