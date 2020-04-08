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
    public class HomeController : Controller
    {
        readonly ILog _log = LogManager.GetLogger(typeof(HomeController));
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Online()
        {
            var list0 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine");
            var list1 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine_OA");
            var list2 = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLineApp");

            ViewBag.list0 = new List<UserOnLine>();
            ViewBag.list1 = new List<UserOnLine>();
            ViewBag.list2 = new List<UserOnLine>();

            if (list0 != null && list0.Any())
            {
                ViewBag.list0 = list0.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
            }
            if (list1 != null && list1.Any())
            {
                ViewBag.list1 = list1.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
            }
            if (list2 != null && list2.Any())
            {
                ViewBag.list2 = list2.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
            }
            return View();
        }

        public ActionResult NewOnline()
        {
            
            return View();
        }

        /// <summary>
        /// 服务端日志查看页面
        /// </summary>
        /// <returns></returns>
        public ActionResult ServiceLog()
        {
            var filename = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";

            ViewBag.filename = filename;

            return View();
        }





        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult OAPushMessageLog()
        {
            return View();
        }
    }
}