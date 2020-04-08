using Edu.Tools;
using Edu.Entity;
using Edu.Models;
using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Edu.Web.Areas.Admin.Controllers
{
    public class PersonPwdController : AdminBaseController
    {
        // GET: Admin/PersonPwd
        public ActionResult Index()
        {
            var query = unitOfWork.DUserInfo.GetByID(LoginUserService.UserID);
            return View(query);
        }
        public ActionResult ModyPwd(UserInfo user)
        {
            OperationResult result = null;
            string newpwd = this.Request.Form["newpwd"];
            if (string.IsNullOrEmpty(newpwd))
            {
                return Json(new { r = false, m = "密码不能为空" }, JsonRequestBehavior.AllowGet);
            }

            string oldpwd = this.Request.Form["opwd"];

            var old = unitOfWork.DUserInfo.GetByID(user.ID);
            if (SecureHelper.MD5(oldpwd) != old.Pwd)
            {
                return Json(new { r = false, m = "密码不正确" }, JsonRequestBehavior.AllowGet);
            }

            old.Pwd = SecureHelper.MD5(newpwd);
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
    }
}