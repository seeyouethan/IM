using Edu.Tools;
using Edu.Entity;
using Edu.Models;
using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Edu.Models.Models;


namespace Edu.Web.Areas.Admin.Controllers
{
    public class UserInfoController : AdminBaseController
    {
        public ActionResult Index(int? roleID, int? state, DateTime? startT, DateTime? endT, string sn = "", int pageNo = 1)
        {
            Paging paging = new Paging();
            paging.PageNumber = pageNo;
            var query = unitOfWork.DUserInfo.GetIQueryable(p => p.UserName.Contains(sn) || p.TrueName.Contains(sn) || p.Email.Contains(sn) || p.Phone.Contains(sn));

            if (roleID != null)
            {
                query = query.Where(p => p.RoleID == roleID);
            }
            if (state != null)
            {
                query = query.Where(p => p.States == state);
            }
            if (startT != null)
            {
                query = query.Where(p => p.CreateDate > startT);
            }
            if (endT != null)
            {
                query = query.Where(p => p.CreateDate < endT);
            }
            paging.Amount = query.Count();
            paging.EntityList = query.OrderByDescending(p => p.CreateDate).Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).ToList();
            ViewBag.sn = sn;
            ViewBag.roleID = roleID;
            ViewBag.PageNo = pageNo;//页码
            ViewBag.PageCount = paging.PageCount;//总页数

            var list = from m in paging.EntityList as IEnumerable<UserInfo>
                       join n in unitOfWork.DUserRole.GetAll() on m.RoleID equals n.ID into tempa
                       from a in tempa
                       select new UserInfoView
                       {
                           ID = m.ID,
                           GUID = m.uID,
                           Phone = m.Phone,
                           Photo = m.Photo,
                           Email = m.Email,
                           RoleID = m.RoleID.Value,
                           RoleName = a.Name,
                           States = m.States,
                           UserName = m.UserName,
                           TrueName = m.TrueName
                       };

            return View(list);
        }
        public ActionResult ModyOp(int? id)
        {
            var old = unitOfWork.DUserInfo.GetByID(id);
            List<SelectListItem> LsRole = new List<SelectListItem>();
            var userroles = unitOfWork.DUserRole.GetAll();
            if (old != null)
            {
                LsRole.AddRange(new SelectList(userroles, "ID", "NAME", old.RoleID));
            }
            else
            {
                LsRole.AddRange(new SelectList(userroles, "ID", "NAME"));
            }
            ViewBag.RoleID = LsRole;
            return View(old);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult User_Op(UserInfo user)
        {
            OperationResult result = null;
            var old = unitOfWork.DUserInfo.GetByID(user.ID);
            List<string> list = new List<string>();
            list.AddRange(this.Request.Form.AllKeys.AsEnumerable());
            string uploadsFolder = Request.ApplicationPath.TrimEnd('/') + "/File/User/" + user.ID + "/";//附件路径文件
            MFileInfo fl = new FileUploadService().FileUpload(uploadsFolder);
            if (fl != null)
            {
                user.Photo = fl.Path;
                list.Add("Photo");
            }
            string showpwd = user.Pwd;
            if (old == null)
            {
                user.uID = Guid.NewGuid().ToString();
                user.States = (int)Edu.Models.UserInfo_UserState.Normal;
                user.Pwd = SecureHelper.MD5(showpwd);
                user.CreateDate = DateTime.Now;
                user.CreateUser = Edu.Service.LoginUserService.UserID.ToString();
                user.IPAdress = Edu.Tools.StringHelper.GetIP();
                unitOfWork.DUserInfo.Insert(user);
                new DBLogService().insert(ActionClick.Add, user);
                result = unitOfWork.Save();
            }
            else
            {
                if (!string.IsNullOrEmpty(this.Request.Form["Pwd"]))
                {
                    user.Pwd = SecureHelper.MD5(showpwd);
                }
                new DBLogService().insert(ActionClick.Mody, user);
                unitOfWork.DUserInfo.Update(old, user, list);
                result = unitOfWork.Save();
                CacheHelper.RemoveAllCache("UserInfo" + old.ID);
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
        public ActionResult DeleteOp(int userid)
        {

            var old = unitOfWork.DUserInfo.GetByID(userid);
            new DBLogService().insert(ActionClick.Delete, old);
            unitOfWork.DUserInfo.Delete(old);
            var result = unitOfWork.Save();

            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败\n" + result.Message }, JsonRequestBehavior.AllowGet);

            }
        }
        public ActionResult GetIsUserName(string userName)
        {
            var user = unitOfWork.DUserInfo.Get(p => p.UserName == userName).FirstOrDefault();
            if (user == null)
            {
                return Json("False", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("True", JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public ActionResult GetModyPwd(int userid)
        {
            ViewBag.refUrl = Request.UrlReferrer;//修改后返回上层url
            var user = unitOfWork.DUserInfo.GetByID(userid);
            return PartialView("_ModyPwd", user);
        }

        public ActionResult ModyPwd(UserInfo user)
        {
            OperationResult result = null;
            var old = unitOfWork.DUserInfo.GetByID(user.ID);
            old.Pwd = SecureHelper.MD5(user.Pwd);
            unitOfWork.DUserInfo.Update(old);
            result = unitOfWork.Save();
            new DBLogService().insert(ActionClick.Other, user, "修改密码");
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败\n" + result.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult LoadSearch(int? roleID, int? state, DateTime? startT, DateTime? endT, string sn = "")
        {
            ViewBag.sn = sn;
            ViewBag.state = state;
            ViewBag.startT = startT;
            ViewBag.endT = endT;
            List<SelectListItem> LsRole = new List<SelectListItem>();
            var userroles = unitOfWork.DUserRole.GetAll();
            LsRole.AddRange(new SelectList(userroles, "ID", "NAME", roleID));
            ViewBag.roleID = LsRole;
            return PartialView("_Search");
        }

        public ActionResult ChangeStates(string ids, YesNo _state)
        {

            var Intid = StringHelper.GetListInt(ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            foreach (int item in Intid)
            {
                var query = unitOfWork.DUserInfo.GetByID(item);
                query.States = (int)_state;
                unitOfWork.DUserInfo.Update(query);
            }
            OperationResult result = unitOfWork.Save();
            new DBLogService().insert(ActionClick.Other, ids, "修改锁定状态");
            if (result.ResultType == OperationResultType.Success)
            {
                return Json(new { r = true }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { r = false, m = "操作失败\n" + result.Message }, JsonRequestBehavior.AllowGet);

            }
        }

    }
}