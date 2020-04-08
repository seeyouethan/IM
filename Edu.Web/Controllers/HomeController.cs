using Edu.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Edu.Service.Service;

namespace Edu.Web.Controllers
{
    public class HomeController : BaseControl
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WebRtcTest()
        {
            return View();
        }

        public ActionResult WebRtcTestiframe()
        {
            return View();
        }
        public ActionResult ToDoList()
        {
            return View();
        }

        public ActionResult IMTest(string groupid,string groupname)
        {
            ViewBag.groupid = groupid;
            ViewBag.groupname = groupname;
            return View();
        }
        

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult GetMain()
        {
            return PartialView("_Main");
        }

        public ActionResult WebApiTest()
        {
            return View();
        }

        public ActionResult UploadPage()
        {
            return View();
        }
        /// <summary>
        /// 上传文件页面
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadAA()
        {
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 },
                        JsonRequestBehavior.AllowGet);
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            
            
            try
            {
                string virtualPath = Server.MapPath("~/upload/");
                if (!Directory.Exists(virtualPath))
                {
                    Directory.CreateDirectory(virtualPath);
                }
                if (file.ContentLength > 1048576000)
                {
                    return Json(new { Success = false, Content = "", Error = "文件大小过大，请上传1GB以内的文件", Message = "操作失败", Count = 0, Total = 0 },
                        JsonRequestBehavior.AllowGet);
                }
                if (file.ContentLength < 1)
                {
                    return Json(new { Success = false, Content = "", Error = "文件大小为0，上传失败", Message = "操作失败", Count = 0, Total = 0 },
                        JsonRequestBehavior.AllowGet);
                }
                string fileName = file.FileName;
                string DtFromat = DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
                string fullFileName = fileName;
                if (fileName.LastIndexOf('.') > 0)
                {
                    fullFileName = fileName.Substring(0, fileName.LastIndexOf('.')) + "_" + DtFromat +
                                   fileName.Substring(fileName.LastIndexOf('.'));
                }
                if (fileName != null && (fileName.LastIndexOf('\\') > 0))
                    fullFileName = fileName.Substring(fileName.LastIndexOf('\\') + 1);
                string fileSavePath = string.Format("{0}{1}", virtualPath, fullFileName);
                file.SaveAs(fileSavePath);

                return Json(new { Success = true, Content = "/im/upload/"+ fullFileName, Error = "", Message = "操作成功", Count = 0, Total = 0 },
                        JsonRequestBehavior.AllowGet);


            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex, Message = "操作失败", Count = 0, Total = 0 },
                        JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult WebRtcA()
        {
            return View();
        }
        public ActionResult WebRtcB()
        {
            return View();
        }
    }
}