using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Edu.Models.Models;
using Edu.SignalRServer.Hub;
using Edu.Tools;
using log4net;

namespace Edu.SignalRServer.Controllers
{
    public class MainController : Controller
    {
        readonly ILog _log = LogManager.GetLogger(typeof(MainController));
        public ActionResult Index()
        {
            var list0 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine");
            var list1 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLineApp");

            ViewBag.list0 = new List<UserOnLine>();
            ViewBag.list1 = new List<UserOnLine>();

            if (list0 != null && list0.Any())
            {
                ViewBag.list0 = list0.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
            }

            if (list1 != null && list1.Any())
            {
                ViewBag.list1 = list1.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
            }



            return View();
        }

        public ActionResult Log()
        {
            return Log();
        }

    }
}