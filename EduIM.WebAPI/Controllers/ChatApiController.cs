using System;
using System.Web;
using System.Web.Http;
using Edu.Entity;
using Edu.Service;
using Edu.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Edu.Models.Models.Msg;
using Edu.Service.Service.Message;
using EduIM.WebAPI.Filters;
using EduIM.WebAPI.Models;
using EduIM.WebAPI.Service;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Drawing;

namespace EduIM.WebAPI.Controllers
{
    
    
    public class ChatApiController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private readonly ssoUser _ssoUserOfWork = new ssoUser();
        



        

        /// <summary>
        /// 移动端发送图片的时候，调用这个接口（不带缩略图）
        /// </summary>
        /// <returns></returns>
        [NoAccessToken]
        public IHttpActionResult SendImgMsg()
        {
            string userId = HttpContext.Current.Request.Form["userId"];
            string touid = HttpContext.Current.Request.Form["touid"];
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "userId为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var uid = userId;
            string strPostfix = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            if (strPostfix.ToLower() != "gif" && strPostfix.ToLower() != "jpg" && strPostfix.ToLower() != "pic" && strPostfix.ToLower() != "bmp" && strPostfix.ToLower() != "jpeg" && strPostfix.ToLower() != "png")
            {
                return Json(new { Success = false, Content = "", Error = "请上传图片格式" + "（bmp;jpg;jpeg;png;tif;pic;)", Message = "操作失败", Count = 0, Total = 0 });
            }
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                return Json(new { Success = true, Content = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/GetHfsImage?filecode=" + result.Content, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }



        /// <summary>
        /// 移动端发送图片的时候，调用这个接口（带缩略图）
        /// </summary>
        /// <returns></returns>
        [NoAccessToken]
        public IHttpActionResult SendImgMsgAndThumbnail()
        {
            string userId = HttpContext.Current.Request.Form["userId"];
            string touid = HttpContext.Current.Request.Form["touid"];
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "userId为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var uid = userId;
            string strPostfix = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            if (strPostfix.ToLower() != "gif" && strPostfix.ToLower() != "jpg" && strPostfix.ToLower() != "pic" && strPostfix.ToLower() != "bmp" && strPostfix.ToLower() != "jpeg" && strPostfix.ToLower() != "png")
            {
                return Json(new { Success = false, Content = "", Error = "请上传图片格式" + "（bmp;jpg;jpeg;png;tif;pic;)", Message = "操作失败", Count = 0, Total = 0 });
            }
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                //开始生成缩略图，并上传

                var imgPic = System.Drawing.Image.FromStream(file.InputStream);
                var thumbnailStream = CreateThumbnailPic(imgPic, 200, 200);
                thumbnailStream.Seek(0, SeekOrigin.Begin);
                var fileCopyName = Guid.NewGuid().ToString() + "_thumbnail.jpg";
                var result2 = FileService.UploadImageThumbnail(fileCopyName, "ZK-700", thumbnailStream.Length, thumbnailStream);

                if (result.Success)
                {
                    return Json(new { Success = true, Content = new { oImage = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/GetHfsImage?filecode=" + result.Content, tImage = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/GetHfsImage?filecode=" + result2.Content }, Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = true, Content = "", Error = "原图上传成功，缩略图生成失败" +result2.Error, Message = "操作失败", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 发送文件消息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SendFileMsg()
        {
            string userId = HttpContext.Current.Request.Form["userId"];
            string touid = HttpContext.Current.Request.Form["touid"];
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "userId为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var uid = userId;
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                return Json(new { Success = true, Content = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/Download?title=" + file.FileName + "&fileCode=" + result.Content, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 最新的发送聊天消息(上传图片消息）群组聊天发送图片也有这个接口，需要注意的是群组发送图片的时候，数据表IMMsg中存的记录是没有存放tousername这个字段的
        /// 修改了接口返回的数据格式
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="touid"></param>
        /// <param name="isgroup"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SendImgMsgNew()
        {
            string userId = HttpContext.Current.Request.Form["userId"];
            string touid = HttpContext.Current.Request.Form["touid"];
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "userId为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var uid = userId;
            string strPostfix = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            if (strPostfix.ToLower() != "gif" && strPostfix.ToLower() != "jpg" && strPostfix.ToLower() != "pic" && strPostfix.ToLower() != "bmp" && strPostfix.ToLower() != "jpeg" && strPostfix.ToLower() != "png")
            {
                return Json(new { Success = false, Content = "", Error = "请上传图片格式" + "（bmp;jpg;jpeg;png;tif;pic;)", Message = "操作失败", Count = 0, Total = 0 });
            }
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                //图片不需要转换
                var fileCode = (string)result.Content;
                var fileuploadresult = new UploadFileResult();
                fileuploadresult.fileCode = fileCode;
                fileuploadresult.convertID = 0;
                fileuploadresult.fileUrl = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/Download?title=" +
                                           file.FileName + "&fileCode=" + result.Content;
                return Json(new { Success = true, Content = fileuploadresult, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 最新的发送文件消息，2019年1月18日修改，新增了碎片化文件功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SendFileMsgNew()
        {
            string userId = HttpContext.Current.Request.Form["userId"];
            string touid = HttpContext.Current.Request.Form["touid"];
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "userId为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                var fileCode = (string)result.Content;
                string convertTaskId;
                var b = FileService.FileConvert(fileCode, out convertTaskId);
                var fileuploadresult = new UploadFileResult();
                fileuploadresult.fileCode = fileCode;
                fileuploadresult.convertID = string.IsNullOrEmpty(convertTaskId) ? 0 : Convert.ToInt32(convertTaskId);
                fileuploadresult.fileUrl = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/Download?title=" +
                                           file.FileName + "&fileCode=" + result.Content;
                return Json(new { Success = true, Content = fileuploadresult, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 发送语音消息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SendAudioMsg()
        {
            string userId = HttpContext.Current.Request.Form["userId"];
            string touid = HttpContext.Current.Request.Form["touid"];
            var fileList = System.Web.HttpContext.Current.Request.Files;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "userId为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (fileList.Count == 0)
            {
                return Json(new { Success = false, Content = "", Error = "文件读取失败", Message = "操作失败", Count = 0, Total = 0 });
            }
            var file = new HttpPostedFileWrapper(fileList[0]) as HttpPostedFileBase;
            string strPostfix = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
            if (strPostfix.ToLower() != "mp3" )
            {
                return Json(new { Success = false, Content = "", Error = "请发送mp3格式文件", Message = "操作失败", Count = 0, Total = 0 });
            }
            var result = FileService.Upload(file, "ZK-700");
            if (result.Success)
            {
                return Json(new { Success = true, Content = ConfigHelper.GetConfigString("OkcsServer") + "/imwebapi/Home/GetHfsAudio?fileCode=" + result.Content, Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            return Json(new { Success = false, Content = "", Error = result.Error, Message = "操作失败", Count = 0, Total = 0 });
        }

        /// <summary>
        /// 获取未读的聊天记录
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="touid">聊天对象用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetUnreadPersonalMsg(string userId, string touid)
        {
            try
            {
                var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", userId);
                var list = new List<Msg>();
                if (all != null && all.Any())
                {
                    var queryUnreadMsg =
                      all.Where(p => p.fromuid == touid);
                    var unreadMsg = queryUnreadMsg as Msg[] ?? queryUnreadMsg.ToArray();
                    if (unreadMsg.Any())
                    {
                        list.AddRange(unreadMsg);
                        /*从缓存中删除这些消息*/
                        foreach (var msg in unreadMsg)
                        {
                            all.Remove(msg);
                        }
                        RedisHelper.Hash_Remove("IMMsg", userId);
                        MsgServices.ResetRedisKeyValue("IMMsg", userId, all);
                        return Json(new { Success = true, Content = list, Error = "", Message = "查询成功", Count = 0, Total = 0 });
                    }
                    return Json(new { Success = false, Content = list, Error = "", Message = "未找到未读消息", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = list, Error = "", Message = "未找到未读消息", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 获取未读的群组消息
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <param name="groupid">聊天用户对象的群组ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetUnreadGroupMsg(string userId, string groupid)
        {
            try
            {
                var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", groupid  + userId);
                
                var list = new List<Msg>();
                if (all.Any())
                {
                    list.AddRange(all);
                    RedisHelper.Hash_Remove("IMGroupMsg", groupid  + userId);
                    return Json(new { Success = true, Content = list, Error = "", Message = "查询成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = false, Content = list, Error = "", Message = "未找到未读消息", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询出错", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 清除未读的聊天记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult ClearUnreadMsg()
        {
            string uid = HttpContext.Current.Request.Form["uid"];
            string touid = HttpContext.Current.Request.Form["touid"];
            string isgroup = HttpContext.Current.Request.Form["isgroup"];
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "uid",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "touid为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }

            if (string.IsNullOrEmpty(isgroup))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "isgroup为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (isgroup == "1")
            {
                var unreadgroupmsg = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", uid) ?? new List<String>();
                if (unreadgroupmsg.Any() && unreadgroupmsg.Contains(touid))
                {
                    unreadgroupmsg.Remove(touid);
                    RedisHelper.Hash_Remove("IMUnreadGroupMsg", uid);
                    RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", uid, unreadgroupmsg);
                }
                var unreadmsg = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid + uid);
                if (unreadmsg != null && unreadmsg.Any())
                {
                    RedisHelper.Hash_Remove("IMGroupMsg", touid + uid);
                }
                return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            else if(isgroup == "0")
            {
                var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", uid);
                if (all != null && all.Any())
                {
                    var queryUnreadMsg =
                  all.Where(p => p.fromuid == touid);
                    var unreadMsg = queryUnreadMsg as Msg[] ?? queryUnreadMsg.ToArray();
                    if (unreadMsg.Any())
                    {
                        foreach (var msg in unreadMsg)
                        {
                            all.Remove(msg);
                        }
                        RedisHelper.Hash_Remove("IMMsg", uid);
                        MsgServices.ResetRedisKeyValue<Msg>("IMMsg", uid, all);
                    }
                    return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
                }
                return Json(new { Success = true, Content = "没有未读消息", Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            else
            {
                return Json(new { Success = false, Content = "", Error = "isgroup参数不对", Message = "操作失败", Count = 0, Total = 0 });
            }
        }

        /// <summary>
        /// 删除对应的聊天消息，传入的值为用户id和消息id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IHttpActionResult>  DelMsg()
        {
            string uid = HttpContext.Current.Request.Form["uid"];
            string msgid = HttpContext.Current.Request.Form["msgid"];
            string touid = HttpContext.Current.Request.Form["touid"];
            string isgroup = HttpContext.Current.Request.Form["isgroup"];
            if (string.IsNullOrEmpty(uid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "用户id为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(msgid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "消息id为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(touid))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "接收人id为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            if (string.IsNullOrEmpty(isgroup))
            {
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "isgroup为空",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            try
            {
                var msgidInt = Convert.ToInt32(msgid);
                var query = _unitOfWork.DIMMsg.Get(p => p.ID == msgidInt && p.CreateUser== uid && p.TouID== touid && p.IsDel != 1).FirstOrDefault();
                if (query != null)
                {
                    if (query.Type == "2")
                    {
                        //如果是图片消息，则执行软删除
                        query.IsDel = 1;
                        //更新下载链接
                        query.FileUrl = ConfigHelper.GetConfigString("DownloadDfile");
                        _unitOfWork.DIMMsg.Update(query);
                    }
                    else
                    {
                        //如果是其他类型消息，则直接删除
                        _unitOfWork.DIMMsg.Delete(query);
                    }
                    var result = _unitOfWork.Save();
                    if (result.ResultType == OperationResultType.Success)
                    {
                        Task.Run(() =>
                        {
                            //查询缓存中的消息
                            if (isgroup == "1")
                            {
                                var touids = new List<string>();
                                touids = touid.Length == 36 ? HttpRequestService.GetWorkGroupMembers(touid) : HttpRequestService.GetSelfGroupMembers(touid);
                                foreach (var id in touids)
                                {
                                    var msgUnread = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid  + id);
                                    if (msgUnread != null && msgUnread.Any())
                                    {
                                        var newmsgUnread = msgUnread.Where(p => p.id0!= msgid).ToList();
                                        RedisHelper.Hash_Remove("IMGroupMsg", touid  + id);
                                        if (newmsgUnread.Any())
                                        {
                                            MsgServices.ResetRedisKeyValue<Msg>("IMGroupMsg", touid  + id, newmsgUnread);
                                        }
                                        else
                                        {
                                            RedisHelper.Hash_Remove("IMGroupMsg", touid  + id);
                                            var groups = RedisHelper.Hash_Get<List<String>>("IMUnreadGroupMsg", id);
                                            if (groups != null && groups.Any() && groups.Contains(touid))
                                            {
                                                groups.Remove(touid);
                                                //重新存储未读消息群组的Redis变量
                                                RedisHelper.Hash_Remove("IMUnreadGroupMsg", id);
                                                RedisHelper.Hash_Set<List<String>>("IMUnreadGroupMsg", id, groups);
                                            }
                                        }
                                    }
                                    
                                }
                            }
                            else if (isgroup == "0")
                            {
                                var msgUnread = MsgServices.GetRedisKeyValue<Msg>("IMMsg", touid);
                                if (msgUnread != null && msgUnread.Any())
                                {
                                    var newmsgUnread = msgUnread.Where(p => p.id0 != msgid).ToList();
                                    RedisHelper.Hash_Remove("IMMsg", touid);
                                    if (newmsgUnread.Any())
                                    {
                                        MsgServices.ResetRedisKeyValue<Msg>("IMMsg", touid, newmsgUnread);
                                    }
                                    else
                                    {
                                        RedisHelper.Hash_Remove("IMMsg", touid);
                                    }
                                }
                            }
                        });
                        
                        return Json(new
                        {
                            Success = true,
                            Content = "",
                            Error = "",
                            Message = "操作成功",
                            Count = 0,
                            Total = 0
                        });
                    }
                    return Json(new
                    {
                        Success = false,
                        Content = "",
                        Error = result.Message,
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
                }
                return Json(new
                {
                    Success = false,
                    Content = "",
                    Error = "未查询到对应的消息",
                    Message = "操作失败",
                    Count = 0,
                    Total = 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "操作失败", Count = 0, Total = 0 });
            }
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

            return ms;
        }
    }

}