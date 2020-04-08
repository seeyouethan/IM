using Edu.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Edu.Service.Service;
using Edu.Tools;
using System.Net;
using Cnki.NetworkDisk.Client;

namespace Edu.Web.Controllers
{
    public class CommonController : BaseControl
    {
        /// <summary>
        /// 上传图片到HFS
        /// </summary>
        /// <param name="file"></param>
        /// <param name="touid"></param>
        /// <param name="uploadFileType"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadImgToHfs(HttpPostedFileBase file, string touid, string uploadFileType = "ZK-700")
        {
            string fileCode;
            var fileName = Path.GetFileName(file.FileName);
            var result = _networkDiskService.UploadFile(fileName, uploadFileType, file.InputStream.Length,
                file.InputStream);
            fileCode = ((UploadRetMsg) result).FileCode;
            if (fileCode == string.Empty)
            {
                return Json(new
                {
                    Code = HttpStatusCode.InternalServerError,
                    Data = new
                    {
                        Result = false,
                        FileCode = string.Empty,
                        ImagePath = ""
                    },
                    Error = "文件上传失败！"

                }, JsonRequestBehavior.AllowGet);
            }
            string convertTaskId;
            result = _fileConvertService.FileConvert(fileCode, out convertTaskId);
            return Json(new
            {
                Code = HttpStatusCode.OK,
                Data = new
                {
                    Result = result,
                    FileCode = fileCode,
                    ImagePath =
                        string.IsNullOrEmpty(fileCode)
                            ? ""
                            : string.Format(ConfigHelper.GetConfigString("HfsUrl"), fileCode),
                    ConvertTaskId = string.IsNullOrEmpty(convertTaskId) ? 0 : Convert.ToInt32(convertTaskId)
                }
            });
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="uploadimgname"></param>
        /// <param name="touid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadImg(HttpPostedFileBase uploadimgname, string touid)
        {
            string strPostfix = uploadimgname.FileName.Substring(uploadimgname.FileName.LastIndexOf(".") + 1);
            if (strPostfix.ToLower() != "gif" && strPostfix.ToLower() != "jpg" && strPostfix.ToLower() != "pic" &&
                strPostfix.ToLower() != "bmp" && strPostfix.ToLower() != "jpeg" && strPostfix.ToLower() != "png")
            {
                return Json(JsonResultHelper.CreateJson(null, false, "请上传图片格式" + "（bmp;jpg;jpeg;png;tif;pic;)"),
                    JsonRequestBehavior.DenyGet);
            }

            KNet.AAMS.Web.Security.ApplicationIdentity identity =
                this.User.Identity as KNet.AAMS.Web.Security.ApplicationIdentity;
            var upload = FileUploadHelper.Upload(uploadimgname, Server.MapPath("~/upload/img"), identity.Id, touid, true);


            /*作者原来这样写 可能是有些浏览器前端不能够识别json吧*/
            //var result = Json(upload, "text/html", JsonRequestBehavior.DenyGet);
            var result = Json(upload, JsonRequestBehavior.DenyGet);
            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="uploadfilename"></param>
        /// <param name="touid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadFile(HttpPostedFileBase uploadfilename, string touid)
        {
            KNet.AAMS.Web.Security.ApplicationIdentity identity =
                this.User.Identity as KNet.AAMS.Web.Security.ApplicationIdentity;
            var upload = FileUploadHelper.Upload(uploadfilename, Server.MapPath("~/upload/file"), identity.Id, touid,
                false);

            var result = Json(upload, JsonRequestBehavior.DenyGet);
            return result;
        }


        /// <summary>
        /// 上传文件 工作日志文件
        /// </summary>
        /// <param name="uploadfilename"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadWorkLogFile()
        {
            var uploadfilename = Request.Files[0];
            var upload = FileUploadHelper.UploadFile(uploadfilename,
                Server.MapPath("~/upload/worklogfile" + "/" + LoginUserService.ssoUserID));
            var result = Json(upload, JsonRequestBehavior.AllowGet);
            return result;
        }

        public JsonResult UploadWorkLogFileHfs()
        {
            var uploadfilename = Request.Files[0];
            var upload = FileUploadHelper.UploadFile(uploadfilename,
                Server.MapPath("~/upload/worklogfile" + "/" + LoginUserService.ssoUserID));
            var result = Json(upload, JsonRequestBehavior.AllowGet);
            return result;
        }


        public ActionResult ShowImg(string imgurl)
        {
            ViewBag.imgurl = imgurl;
            return View();
        }

        public JsonResult GetImg(string imgurl)
        {
            var list = new List<ImgModel>();
            list.Add(new ImgModel()
            {
                alt = "",
                pid = "",
                src = imgurl,
                thumb = ""

            });
            return Json(new {status = 1, msg = "", title = "", id = 1, start = 0, data = list},
                JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDirectoryImgJsonResult(string fromuid, string touid, int pageNo = 1, int pageSize = 10)
        {
            var list = GetDirectoryFile(fromuid, touid, "img");
            list = list.Skip((pageNo - 1)*pageSize).Take(pageSize).ToList();
            return Json(new {list}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDirectoryFileJsonResult(string fromuid, string touid, int pageNo = 1, int pageSize = 10)
        {
            var list = GetDirectoryFile(fromuid, touid, "file");
            list = list.Skip((pageNo - 1)*pageSize).Take(pageSize).ToList();
            return Json(new {list}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDirectoryImgJsonResultGroup(string fromuid, string touid, List<string> touids,
            int pageNo = 1, int pageSize = 10)
        {
            List<string> touidlist;
            var list = new List<MyFileInfoString>();
            if (touid.Length == 36)
            {
                //工作群组 从touids获取用户list
                touidlist = touids.Where(p => p.Length != 0).ToList(); //去空
                touidlist.Add(fromuid);
            }
            else
            {
                //自建群组 查找mysql获取用户list
                touidlist = unitOfWork.DImGroupDetail.Get(p => p.GroupID == touid).Select(p => p.UserID).ToList();
            }
            foreach (var id in touidlist)
            {
                var list0 = GetSingleDirectoryFile(id, touid, "img");
                list.AddRange(list0);
            }
            list = list.Skip((pageNo - 1)*pageSize).Take(pageSize).ToList();
            return Json(new {list}, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDirectoryFileJsonResultGroup(string fromuid, string touid, List<string> touids,
            int pageNo = 1, int pageSize = 10)
        {
            List<string> touidlist;
            var list = new List<MyFileInfoString>();
            if (touid.Length == 36)
            {
                //工作群组 从touids获取用户list

                touidlist = touids.Where(p => p.Length != 0).ToList(); //去空
                touidlist.Add(fromuid);

            }
            else
            {
                //自建群组 查找mysql获取用户list
                touidlist = unitOfWork.DImGroupDetail.Get(p => p.GroupID == touid).Select(p => p.UserID).ToList();

            }
            foreach (var id in touidlist)
            {
                var list0 = GetSingleDirectoryFile(id, touid, "file");
                list.AddRange(list0);
            }
            list = list.Skip((pageNo - 1)*pageSize).Take(pageSize).ToList();
            return Json(new {list}, JsonRequestBehavior.AllowGet);
        }

        public List<MyFileInfoString> GetDirectoryFile(string fromuid, string touid, string fileType)
        {
            var path0 = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + fileType + "\\" + fromuid + "\\" + touid;
            var path1 = AppDomain.CurrentDomain.BaseDirectory + "upload\\" + fileType + "\\" + touid + "\\" + fromuid;
            var list = new List<MyFileInfo>();
            if (string.IsNullOrEmpty(fromuid) || string.IsNullOrEmpty(touid)) return null;
            if (!Directory.Exists(path0))
            {
                FileHelper.CreateDirectory(path0);
            }
            if (!Directory.Exists(path1))
            {
                FileHelper.CreateDirectory(path1);
            }
            //这里得到的目录是网站目录 而不是网站下应用程序的目录
            DirectoryInfo d0 = new DirectoryInfo(path0);
            DirectoryInfo d1 = new DirectoryInfo(path1);
            FileInfo[] arrFi0 = d0.GetFiles("*.*");
            FileInfo[] arrFi1 = d1.GetFiles("*.*");

            var myusername = LoginUserService.User.TrueName;
            var ssoUser = ssoUserOfWork.GetUserByID(touid);
            var otherusername = "";
            if (ssoUser != null)
            {
                otherusername = ssoUser.RealName;
            }


            list = arrFi0.Select(fileinfo => new MyFileInfo
            {
                fileInfo = fileinfo,
                username = myusername,
                path = "/im/upload/" + fileType + "/" + fromuid + "/" + touid + "/" + fileinfo.Name,
            }).ToList();

            list.AddRange(arrFi1.Select(item => new MyFileInfo
            {
                fileInfo = item,
                username = otherusername,
                path = "/im/upload/" + fileType + "/" + touid + "/" + fromuid + "/" + item.Name,
            }));


            SortAsFileCreationTime(ref list);

            var listResult = new List<MyFileInfoString>();

            for (int i = 0; i < list.Count; i++)
            {
                var filestring = new MyFileInfoString
                {
                    filename = list[i].fileInfo.Name,
                    filepath = list[i].path,
                    username = list[i].username,
                    day = list[i].fileInfo.LastWriteTime.ToString("yyyy-MM-dd"),
                    dtime = list[i].fileInfo.LastWriteTime.ToString("HH:mm:ss")
                };
                listResult.Add(filestring);
            }
            return listResult;
        }

        public List<MyFileInfoString> GetSingleDirectoryFile(string fromuid, string touid, string fileType)
        {
            var path0 = System.AppDomain.CurrentDomain.BaseDirectory + "upload\\" + fileType + "\\" + fromuid + "\\" +
                        touid;
            var list = new List<MyFileInfo>();
            if (string.IsNullOrEmpty(fromuid) || string.IsNullOrEmpty(touid)) return null;
            if (!Directory.Exists(path0))
            {
                FileHelper.CreateDirectory(path0);
            }
            DirectoryInfo d0 = new DirectoryInfo(path0);
            FileInfo[] arrFi0 = d0.GetFiles("*.*");

            var ssoUser = ssoUserOfWork.GetUserByID(fromuid);
            var myusername = "";
            if (ssoUser != null)
            {
                myusername = ssoUser.RealName;
            }
            list = arrFi0.Select(fileinfo => new MyFileInfo
            {
                fileInfo = fileinfo,
                username = myusername,
                path = "/im/upload/" + fileType + "/" + fromuid + "/" + touid + "/" + fileinfo.Name,
            }).ToList();
            SortAsFileCreationTime(ref list);
            var listResult = new List<MyFileInfoString>();
            for (int i = 0; i < list.Count; i++)
            {
                var filestring = new MyFileInfoString
                {
                    filename = list[i].fileInfo.Name,
                    filepath = list[i].path,
                    username = list[i].username,
                    day = list[i].fileInfo.LastWriteTime.ToString("yyyy-MM-dd"),
                    dtime = list[i].fileInfo.LastWriteTime.ToString("HH:mm:ss")
                };
                listResult.Add(filestring);
            }
            return listResult;
        }

        /// <summary>
        /// 按照时间排序 从大到小
        /// </summary>
        /// <param name="arrFi"></param>
        private void SortAsFileCreationTime(ref List<MyFileInfo> arrFi)
        {
            arrFi.Sort((y, x) => x.fileInfo.CreationTime.CompareTo(y.fileInfo.LastWriteTime));
        }






        public class ImgModel
        {
            public String alt;
            public string pid;
            public string src;
            public string thumb;
        }

        public class MyFileInfo
        {
            /// <summary>
            /// 文件信息
            /// </summary>
            public FileInfo fileInfo;

            /// <summary>
            /// 是否是自己上传的文件
            /// </summary>
            public string username;

            public string path;
        }

        public class MyFileInfoString
        {
            public string filename;
            public string day;
            public string dtime;

            /// <summary>
            /// 文件信息
            /// </summary>
            public string filepath;

            /// <summary>
            /// 是否是自己上传的文件
            /// </summary>
            public string username;
        }




    }
}