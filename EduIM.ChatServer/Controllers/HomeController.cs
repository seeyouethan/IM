using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Edu.Service;

namespace EduIM.ChatServer.Controllers
{
    public class HomeController : BaseControl
    {
        public ActionResult Index()
        {
            var query = unitOfWork.DUserRole.GetAll();
            ViewBag.Title = "Home Page";

            return View(query);
        }
    }
}
