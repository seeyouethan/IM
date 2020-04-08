
using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Edu.Entity;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.Tools;
using Exceptionless.Json;
using Edu.Service.Service.Message;

namespace Edu.Web.Controllers
{

    /// <summary>
    /// 群组管理(电厂项目 单独使用)
    /// </summary>
    public class GroupController : BaseControl
    {
        public ActionResult Index()
        {
            var uid = LoginUserService.ssoUserID;
            var uRealName = LoginUserService.realName;
            ViewBag.uid = uid;
            ViewBag.uRealName = uRealName;
            return View();
        }

        public ActionResult OkcsDemo()
        {
            return View();
        }
    }

}