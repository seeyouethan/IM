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
    public class PersonInfoController : AdminBaseController
    {
        // GET: Admin/PersonInfo
        public ActionResult Index()
        {

            var query = unitOfWork.DUserInfo.GetByID(LoginUserService.UserID);
            return View(query);
        }
        public ActionResult Mody()
        {
            var query = unitOfWork.DUserInfo.GetByID(LoginUserService.UserID);
            return View(query);
        }
        [HttpPost]
        public ActionResult Mody(UserInfo user)
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
            new DBLogService().insert(ActionClick.Mody, user);
            unitOfWork.DUserInfo.Update(old, user, list);
            result = unitOfWork.Save();
            CacheHelper.RemoveAllCache("UserInfo" + user.ID);
            var isSave = unitOfWork.SaveRMsg();
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