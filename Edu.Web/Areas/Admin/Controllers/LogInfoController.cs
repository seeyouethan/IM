using Edu.Entity;
using Edu.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Edu.Tools;
using Edu.Models;

namespace Edu.Web.Areas.Admin.Controllers
{
    public class LogInfoController : AdminBaseController
    {
        #region 列表
        // GET: Admin/LogInfo
        public ActionResult Index(DateTime? startT, DateTime? endT, int? OpType, string sn = "", int pageNo = 1)
        {
            Paging paging = new Paging();
            paging.PageNumber = pageNo;
            var query = unitOfWork.DLogInfo.GetIQueryable(p => p.Name.Contains(sn) || p.TableName.Contains(sn) || p.Url.Contains(sn));
            if (startT != null)
            {
                query = query.Where(p => p.CreateDate >= startT);
            }
            if (endT != null)
            {
                query = query.Where(p => p.CreateDate <= endT);
            }
            if (OpType != null)
            {
                query = query.Where(p => p.OpType == OpType);
            }
            paging.Amount = query.Count();
            paging.EntityList = query.OrderByDescending(p => p.CreateDate).Skip(paging.PageSiz * paging.PageNumber).Take(paging.PageSiz).ToList();
            ViewBag.sn = sn;
            ViewBag.startT = startT;
            ViewBag.endT = endT;
            ViewBag.PageNo = pageNo;//页码
            ViewBag.PageCount = paging.PageCount;//总页数
            ViewBag.OpType = OpType;
            return View(paging.EntityList);
        }
        #endregion


        #region 查询
        public ActionResult LoadSearch(DateTime? startT, DateTime? endT, int? OpType, string sn = "")
        {
            ViewBag.sn = sn;
            ViewBag.startT = startT;
            ViewBag.endT = endT;
            ViewBag.OpType = OpType;
            return PartialView("_Search");
        }
        #endregion


    }

}
