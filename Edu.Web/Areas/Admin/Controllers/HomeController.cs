using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Edu.Web.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        // GET: Admin/Home
        public ActionResult Index()
        {

            return View();

        }
        public ActionResult Right()
        {
            return View();
        }
        public ActionResult GetLeft()
        {

            if (LoginUserService.UserID == 0)
            {
                return PartialView("_Left", null);
            }
            var user_info = unitOfWork.DUserInfo.GetByID(LoginUserService.UserID);
            ViewBag.user_info = user_info;
            string rid = user_info.RoleID.ToString();
            var query = unitOfWork.DMenu.Get(p => ("," + p.RoleIDs).Contains("," + rid + ",") && p.FuncLevel == 2, q => q.OrderBy(p => p.OrderNo)).GroupBy(p => p.ParentID);
            return PartialView("_Left", query);
        }
    }
}