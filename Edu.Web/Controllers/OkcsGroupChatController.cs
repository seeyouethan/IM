using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Edu.Entity;
using Edu.Models;
using Edu.Models.Models;
using Edu.Service.Service;
using Edu.Tools;
using KNet.AAMS.Utility.Extensions;

namespace Edu.Web.Controllers
{
    public class OkcsGroupChatController : BaseControl
    {
        // GET: OkcsGroupChat
        public ActionResult Index(string groupid, string groupname)
        {
            var selfuid = LoginUserService.ssoUserID;
            /*再从redis中查找一下connid*/
            ViewBag.sefconnid = "";
            var self = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", selfuid);
            if (self != null)
            {
                ViewBag.sefconnid = self.ConnectionId;
            }
            //发送给群组的消息的touid存的为群组的groupid
            ViewBag.touid = groupid;
            ViewBag.fromusername = ssoUserOfWork.GetUserByID(LoginUserService.ssoUserID).RealName;
            ViewBag.tousername = groupname;
            ViewBag.photo = ConfigHelper.GetConfigString("sso_host_name") + "pic/" + LoginUserService.ssoUserID;
            return View();
        }


        public ActionResult CheckLogin()
        {
            var selfuid = LoginUserService.ssoUserID;
            /*再从redis中查找一下connid*/
            ViewBag.sefconnid = "";
            var self = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", selfuid);
            if (self != null)
            {
                return Json(new { r = true ,uid= selfuid ,connid= self.ConnectionId }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { r = false, uid = selfuid}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取最近聊天记录 5条 显示在聊天窗口中 这是群聊的最近聊天记录
        /// </summary>
        /// <param name="groupid"></param>
        /// <returns></returns>
        public ActionResult RecentlyChat(string groupid)
        {
            var query =
                     unitOfWork.DIMMsg.Get(
                         p => (p.TouID == groupid)).OrderByDescending(p => p.CreateDate).Take(5).OrderBy(p => p.CreateDate);
            return PartialView("_RecentlyChat", query);
        }
    }
}