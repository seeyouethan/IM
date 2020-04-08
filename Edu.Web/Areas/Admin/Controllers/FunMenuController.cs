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
    public class FunMenuController : AdminBaseController
    {
        // GET: Admin/FunMenu
        public ActionResult Index()
        {
            var query = unitOfWork.DMenu.Get(p => p.States==(int)EnumState.Normal, q => q.OrderBy(p => p.OrderNo)).OrderBy(p => p.FuncID);

            return View(query);
        }
        public ActionResult GetOp(int? rid)
        {
            var old = unitOfWork.DMenu.GetByID(rid);
            if (old == null)
            {
                return PartialView("_Op");
            }
            else
            {
                return PartialView("_Op", old);
            }

        }
        public ActionResult Mody_Op(Menu menu)
        {
            var old = unitOfWork.DMenu.GetByID(menu.ID);
            if (old == null)
            {

                new DBLogService().insert(ActionClick.Add, menu);
                menu.States = (int)EnumState.Normal;
                unitOfWork.DMenu.Insert(menu);
            }
            else
            {
                new DBLogService().insert(ActionClick.Mody, menu);
                unitOfWork.DMenu.Update(old, menu, new List<string>(this.Request.Form.AllKeys.AsEnumerable()));
            }
            var isSave = unitOfWork.SaveRMsg();
            if (isSave == "True")
            {
                ViewBag.alert = "操作成功";
            }
            else
            {
                ViewBag.alert = "操作失败!!" + isSave;
            }
            ViewBag.Url = "/Admin/FunMenu/index";
            return PartialView("_JsAlertUrl");
        }

        public ActionResult DeleteOp(int rid)
        {
   
            var old = unitOfWork.DMenu.GetByID(rid);
            new DBLogService().insert(ActionClick.Delete, old);
            unitOfWork.DMenu.Delete(old);
            var isDel = unitOfWork.SaveRMsg();
            return Content(isDel);
        }


        /// <summary>
        /// 设置权限
        /// </summary>
        /// <returns></returns>
        public ActionResult SetAuthory(string roleid, string checkvaule)
        {
            var checkids = "," + checkvaule;
            var MenuTable = unitOfWork.DMenu.GetAll();
            foreach (var item in MenuTable)
            {
                if (item.FuncType == "菜单")
                {
                    continue;
                }
                if (checkids.Contains("," + item.ID + ","))
                {
                    if (("," + item.RoleIDs).Contains("," + roleid + ","))
                    {
                    }
                    else
                    {
                        item.RoleIDs += roleid + ",";
                    }
                }
                else
                {
                    if (item.RoleIDs != null)
                    {
                        item.RoleIDs = ("," + item.RoleIDs).Replace("," + roleid + ",", ",").TrimStart(',');
                    }
                }
                unitOfWork.DMenu.Update(item);
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add(roleid, checkvaule);
            new DBLogService().insert(ActionClick.Other, dic, "修改权限");
            unitOfWork.Save();
            return Content("True");
        }
        public ActionResult SetAuthority(int rid)
        {
            ViewBag.rid = rid;
            return View();
        }
        public ActionResult LoadCategory(int rid)
        {
            string rids = "," + rid.ToString() + ",";
            var query = from m in unitOfWork.DMenu.GetAll().OrderBy(p => p.OrderNo)
                        select new ZNodeFun
                        {
                            oid = m.ID,
                            id = m.FuncID,
                            name = m.FuncName,
                            pId = m.ParentID,
                            open = true,
                            chk = m.RoleIDs == null ? false : ("," + m.RoleIDs).Contains(rids)
                        };
            return Content(JsonHelper.Instance.Serialize(query).Replace("chk", "checked"));
        }
    }
}