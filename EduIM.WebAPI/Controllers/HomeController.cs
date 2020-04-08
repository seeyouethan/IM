using Edu.Models.Models;
using Edu.Service;
using Edu.Tools;
using EduIM.WebAPI.Filters;
using EduIM.WebAPI.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace EduIM.WebAPI.Controllers
{
    public class HomeController : BaseControl
    {
        /// <summary>
        /// 获取头像/群组Logo 这个接口不用验证
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [NoAccessToken]
        [HttpGet]
        public ActionResult Pic(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new ContentResult { Content = "" };
            try
            {
                var idInt = Convert.ToInt32(id);
                var query = unitOfWork.DImGroup.Get(p => p.ID == idInt).FirstOrDefault();
                if (query != null)
                {
                    if (!string.IsNullOrEmpty(query.Photo) && query.Photo.Contains("data:image/"))
                    {
                        var base64 = query.Photo.Replace("data:image/webp;base64,", "").Replace("data:image/png;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");//将base64头部信息替换
                        byte[] imgBytes = Convert.FromBase64String(base64);
                        return File(imgBytes, "application/octet-stream");
                    }
                    //如果为空，返回一个默认的
                    return GetDefaultGroupImg();
                }
            }
            catch (Exception ex)
            {
                return GetDefaultGroupImg();
            }
            return GetDefaultGroupImg();
        }

        /// <summary>
        /// 上传文件，如果是上传的图片文件，则访问图片的时候使用HFS获取图片地址+这个方法返回的Content即可获取，
        /// 若上传是其他文件，则需要使用下面的Download方法
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [NoAccessToken]
        public ActionResult Upload()
        {
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                return Json(new { Success = true, Content = result.Content, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        [HttpPost]
        [NoAccessToken]
        public ActionResult UploadImageAndThumbnail()
        {
            var fileList = System.Web.HttpContext.Current.Request.Files;

            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }

            var fileOriginal = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;

            var result = FileService.Upload(fileOriginal, "ZK-700");
            if (result.Success)
            {
                //上传缩略图
                var imgPic = System.Drawing.Image.FromStream(fileList[0].InputStream);
                var thumbnailStream = CreateThumbnailPic(imgPic, 200, 200);

                //重置指针的值，因为在转换为流的时候，指针会指到结尾  
                //将当前流位置设置为起始位置
                thumbnailStream.Seek(0, SeekOrigin.Begin);
                var fileCopyName = Guid.NewGuid().ToString()+"_thumbnail.jpg";
                var result2 = FileService.UploadImageThumbnail(fileCopyName, "ZK-700", thumbnailStream.Length, thumbnailStream);
                if (result.Success)
                {
                    return Json(new { Success = true, Content = new { oImage= result.Content, tImage=result2.Content}, Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = "原图上传成功，缩略图生成失败！", Error = result2.Error, Message = "操作失败", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 下载文件方法
        /// </summary>
        /// <param name="title">下载时候，显示的文件名</param>
        /// <param name="fileCode">Upload方法返回的Content</param>
        /// <returns></returns>
        [HttpGet]
        [NoAccessToken]
        public ActionResult Download(string title, string fileCode)
        {
            //如果title中含有后缀，则将后缀去掉
            var pos = title.LastIndexOf(".");
            if (pos >= 0)
            {
                title = title.Substring(0, pos);
            }

            var fileType = Path.GetExtension(fileCode);
            //获取用户浏览器类型，火狐下不需要编码
            bool isfirefox = Request.UserAgent.ToUpper(CultureInfo.InvariantCulture).Contains("FIREFOX");
            var downloadFileName = Path.Combine("", title + fileType);
            var result = FileService.Download(fileCode);
            if (result.Success)
            {
                var ContentType = "application/octet-stream";
                if(fileType==".ppt")
                {
                    ContentType = "application/vnd.ms-powerpoint";
                }
                else if (fileType == ".pptx")
                {
                    ContentType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                }
                else if (fileType == ".docx")
                {
                    ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }
                else if (fileType == ".doc")
                {
                    ContentType = "application/msword";
                }
                else if (fileType == ".xls" || fileType == ".xlt" || fileType == ".xla")
                {
                    ContentType = "application/vnd.ms-excel";
                }
                else if (fileType == ".rtf")
                {
                    ContentType = "application/rtf";
                }
                else if (fileType == ".txt")
                {
                    ContentType = "text/plain";
                }    
                



                var buffer = result.Content as byte[];
                var file = File(buffer, ContentType,
                   isfirefox ? HttpUtility.UrlDecode(downloadFileName) : downloadFileName);
                
                return file;
            }
            return Json(new
            {
                Success = false,
                Content = "",
                Error = result.Error,
                Message = "操作失败",
                Count = 0,
                Total = 0
            }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        [NoAccessToken]
        public ActionResult DownloadVideo(string title, string fileCode)
        {
            //如果title中含有后缀，则将后缀去掉
            var pos = title.LastIndexOf(".");
            if (pos >= 0)
            {
                title = title.Substring(0, pos);
            }

            var fileType = Path.GetExtension(fileCode);
            //获取用户浏览器类型，火狐下不需要编码
            bool isfirefox = Request.UserAgent.ToUpper(CultureInfo.InvariantCulture).Contains("FIREFOX");
            var downloadFileName = Path.Combine("", title + fileType);
            var result = FileService.Download(fileCode);
            if (result.Success)
            {
                var buffer = result.Content as byte[];
                var file = File(buffer, "video/mp4",
                   isfirefox ? HttpUtility.UrlDecode(downloadFileName) : downloadFileName);
                return file;
            }
            return Json(new
            {
                Success = false,
                Content = "",
                Error = result.Error,
                Message = "操作失败",
                Count = 0,
                Total = 0
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [NoAccessToken]
        public ActionResult GetHfsImage(string filecode)
        {
            if (string.IsNullOrEmpty(filecode))
                return GetDefaultGroupImg();
            try
            {
                var result = FileService.Download(filecode);
                if (result.Success)
                {
                    var buffer = result.Content as byte[];
                    return File(buffer, "application/octet-stream");
                }
                return GetDefaultGroupImg();
            }
            catch (Exception ex)
            {
                return GetDefaultGroupImg();
            }
            return GetDefaultGroupImg();
        }

        [HttpGet]
        [NoAccessToken]
        public ActionResult GetHfsAudio(string filecode)
        {
            if (string.IsNullOrEmpty(filecode))
                return GetDefaultGroupImg();
            try
            {
                var result = FileService.Download(filecode);
                if (result.Success)
                {
                    var buffer = result.Content as byte[];
                    return File(buffer, "audio/mpeg");
                }
                return GetDefaultGroupImg();
            }
            catch (Exception ex)
            {
                return GetDefaultGroupImg();
            }
            return GetDefaultGroupImg();
        }

        [HttpGet]
        [NoAccessToken]
        public ActionResult DownloadDfile()
        {
            return Json(new
            {
                Success = false,
                Content = "",
                Error = "",
                Message = "该文件已经被删除",
                Count = 0,
                Total = 0,
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApiTest()
        {
            return Json(new
            {
                Success = true,
                Content = "测试接口获取成功",
                Error = "",
                Message = "查询成功",
                Count = 0,
                Total = 0
            }, JsonRequestBehavior.AllowGet);
        }

        //解析base64编码获取图片
        private Bitmap Base64ToImg(string base64Code)
        {
            MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64Code));
            return new Bitmap(stream);
        }

        /// <summary>
        /// 获取默认的群组头像
        /// </summary>
        /// <returns></returns>
        private FileContentResult GetDefaultGroupImg()
        {
            var base64 = ConfigHelper.GetConfigString("DefaultGroupLogo").Replace("data:image/png;base64,", "").Replace("data:image/jpg;base64,", "").Replace("data:image/jpeg;base64,", "");
            byte[] defaultimgBytes1 = Convert.FromBase64String(base64);
            return File(defaultimgBytes1, "application/octet-stream");
        }

        [HttpGet]
        [NoAccessToken]
        public ActionResult SearchUserNewPinYin(string keyword)
        {
            var memberList = new List<object>();
            var currentuser = ssoUserOfWork.GetUserByID("580f861f-d28e-40f0-880e-55e13f906f0e");
            memberList = RedisHelper.Hash_Get<List<object>>("AllMembers", currentuser.OrgId);
            if (memberList == null)
            {
                memberList = ssoUserOfWork.GetMembersOfUnit(currentuser.OrgId);
            }
            List<SUser> list = new List<SUser>();
            object objdate = new object();
            JavaScriptSerializer jsonser = new JavaScriptSerializer();
            string myJson = jsonser.Serialize(memberList);
            //到上面步骤已经完成了object转化json，接下来由json转化自定model
            list = JsonConvert.DeserializeObject<List<SUser>>(myJson);
            var listtemp = list.Where(p => p.type == 0);

            //排除掉当前自己的用户
            //查找到自己的常用联系人 获取其中的uid 组成一个List
            var queryTopContact = unitOfWork.DTopContacts.Get(p => p.uID == "b1aac979-c30d-4d6a-99db-9948a55c41e0");
            List<string> uidList = queryTopContact.Select(user => user.ContactID).ToList();
            ViewBag.contactIdList = uidList;
            return null;
        }

        public ActionResult MqttLog(string filename)
        {
            var path = ConfigHelper.GetConfigString("MqttLogFile") + filename;
            string name = Path.GetFileName(path);
            return File(path, "application/octet-stream", filename);
        }

        /// <summary>
        /// 创建缩略图
        /// </summary>
        /// <param name="FromImagePath"></param>
        /// <param name="ToImagePath"></param>
        /// <param name="MaxWidth"></param>
        /// <param name="MaxHeight"></param>
        public Stream CreateThumbnailPic(Image FromImage, double MaxWidth, double MaxHeight)
        {
            Bitmap tmp = null;
            Graphics g = null;
            //double Max_width = 110, Max_height = 110;//假设最大宽度以及最大高度
            int Width = 0;//框条和卡纸的宽度
            int Height = 0;//框条和卡纸的高度
            double wmp = 1, hmp = 1, default_pparm = 1;//默认宽的比例，默认高对应的比例，最大宽度比例，最大高度比例，默认实际比例，最大实际比例
            Width = FromImage.Width;
            Height = FromImage.Height;
            wmp = MaxWidth / Width;//最大宽度比例
            hmp = MaxHeight / Height;//最大高度比例
            default_pparm = wmp < hmp ? wmp : hmp;//默认实际比例

            if (default_pparm > 1)
            {
                default_pparm = 1;
            }
            Width = (int)(Width * default_pparm);
            Height = (int)(Height * default_pparm);

            tmp = new Bitmap((int)(Width), (int)(Height));//最大的容器
            g = Graphics.FromImage(tmp);
            //g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, tmp.Width, tmp.Height));
            Rectangle Boxs = new Rectangle(0, 0, FromImage.Width, FromImage.Height);
            Rectangle Boxd = new Rectangle(0, 0,
                                           (int)(Width), (int)(Height));//算图的起点
            g.DrawImage(FromImage, Boxd, Boxs, GraphicsUnit.Pixel);

            MemoryStream ms = new MemoryStream();
            tmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);

            return ms as Stream;
        }
    }
}