using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using Edu.Entity;
using Edu.Models.IM;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.Service;
using Edu.Service.Service;
using Edu.Tools;
using EduIM.WebAPI.Filters;
using MassTransitConfig = EduIM.WebAPI.Service.MassTransitConfig;
using Edu.Web.Models;
using System.Web.Script.Serialization;
using Edu.Service.Service.Message;
using EduIM.WebAPI.Models;
using EduIM.WebAPI.Service;
using log4net;
using Newtonsoft.Json;

namespace EduIM.WebAPI.Controllers
{
    
    //[WebApiTracker]
    [NoAccessToken]
    public class VideoConferenceApiController : ApiController
    {
        /// <summary>
        /// 获取正在分享屏幕的用户
        /// </summary>
        /// <param name="conferenceid">当前会议ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetOnLiveUser(string conferenceid)
        {
            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_OnlineUser_OnLive", conferenceid) ?? new List<ConferenceLiveUserModel>();
            return Json(
                new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "",
                    Count = list.Count,
                    Total = list.Count
                });
        }

        /// <summary>
        /// 设置当前用户正在分享 摄像头 / 桌面
        /// </summary>
        /// <param name="conferenceid">当前会议ID</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SetOnLiveUser()
        {

            string conferenceid = HttpContext.Current.Request.Form["conferenceid"];
            string uid = HttpContext.Current.Request.Form["uid"];
            string type = HttpContext.Current.Request.Form["type"];


            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_OnlineUser_OnLive",
                          conferenceid) ?? new List<ConferenceLiveUserModel>();
            //if (list.Count >= 4)
            //{
            //    var model = list.Where(p => p.uid == uid).FirstOrDefault();
            //    if (model != null)
            //    {
            //        //已经存在该用户，修改时间
            //        model.type = type;
            //        model.datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");

            //        //更新数据
            //        RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser_OnLive", conferenceid);
            //        RedisHelper.Hash_Set("A_IM_Conference_OnlineUser_OnLive", conferenceid, list);

            //        //存在该用户，返回false
            //        return Json(new
            //        {
            //            Success = true,
            //            Content = list,
            //            Error = "",
            //            Message = "",
            //            Count = list.Count(),
            //            Total = list.Count()
            //        });
            //    }
            //    else
            //    {
            //        //不存在该用户，返回false
            //        return Json(new
            //        {
            //           Success = false,
            //           Content = list,
            //           Error = "",
            //           Message = "",
            //           Count = list.Count(),
            //           Total = list.Count()
            //       });
            //    }
            //}
            //else
            //{
                var model = list.Where(p => p.uid == uid).FirstOrDefault();
                if (model != null)
                {
                    LoggerHelper.Info($"model != null： " + model.uid);
                    model.type = type;
                    model.datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
                }
                else
                {
                    var newModel = new ConferenceLiveUserModel
                    {
                        conferenceid = conferenceid,
                        datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                        ext = "",
                        framerate = "",
                        resolution = "",
                        type = type,
                        uid = uid,
                    };
                    list.Add(newModel);
                }
                //更新数据
                RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser_OnLive", conferenceid);
                RedisHelper.Hash_Set("A_IM_Conference_OnlineUser_OnLive", conferenceid, list);

                return Json(
                new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "",
                    Count = list.Count(),
                    Total = list.Count()
                });
           // }            
        }


        /// <summary>
        /// 设置当前用户正在分享 摄像头 / 桌面
        /// </summary>
        /// <param name="conferenceid">当前会议ID</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult RemoveOnLiveUser()
        {

            string conferenceid = HttpContext.Current.Request.Form["conferenceid"];
            string uid = HttpContext.Current.Request.Form["uid"];

            var list = RemoveUserFromConferenceLiveUserList(conferenceid, uid);

            return Json(
                new
                {
                    Success = true,
                    Content = list,
                    Error = "",
                    Message = "",
                    Count = list.Count(),
                    Total = list.Count()
                });
        }







   
        public IEnumerable<ConferenceLiveUserModel> RemoveUserFromConferenceLiveUserList(string conferenceid, string uid)
        {
            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_OnlineUser_OnLive", conferenceid) ?? new List<ConferenceLiveUserModel>();

            var model = list.Where(p => p.uid == uid).FirstOrDefault();
            if (model != null)
            {
                list.Remove(model);
                RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser_OnLive", conferenceid);
                RedisHelper.Hash_Set("A_IM_Conference_OnlineUser_OnLive", conferenceid, list);
            }
            
            //记录
            //RecordUserLive(conferenceid, uid, "", "offline");

            return list;
        }


        public void RecordUserLive(string conferenceid, string uid, string type, string ext)
        {
            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_UserLive_Record", conferenceid) ?? new List<ConferenceLiveUserModel>();

            var newModel = new ConferenceLiveUserModel
            {
                conferenceid = conferenceid,
                datetime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                ext = ext,
                framerate = "",
                resolution = "",
                type = type,
                uid = uid,

            };
            list.Add(newModel);

            RedisHelper.Hash_Remove("A_IM_Conference_UserLive_Record", conferenceid);
            RedisHelper.Hash_Set("A_IM_Conference_UserLive_Record", conferenceid, list);
        }




        /// <summary>
        /// 直播界面，发送消息给移动端（暂时未开始使用）
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult SendMessage()
        {
            try
            {
                string uid = HttpContext.Current.Request.Form["uid"];
                string touid = HttpContext.Current.Request.Form["touid"];
                string msg = HttpContext.Current.Request.Form["msg"];
                if (string.IsNullOrEmpty(uid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！uid参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(touid))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！touid参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                if (string.IsNullOrEmpty(msg))
                {
                    return Json(new { Success = false, Content = "", Error = "操作失败！msg参数为空！", Message = "操作失败", Count = 0, Total = 0 });
                }
                
                //向MQ中发通知，向移动的推送一条消息 新增成员 的消息
                var liveNotice = new LiveNotice
                {
                    uid = uid,
                    msg = msg,
                };
                var queueAddress = MassTransitConfig.Bus.GetSendEndpoint(MassTransitConfig.GetAppQueueHostAddress()).Result;
                queueAddress.Send<LiveNotice>(liveNotice);
                return Json(new { Success = true, Content = "", Error = "", Message = "操作成功", Count = 0, Total = 0 });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "操作失败", Count = 0, Total = 0 });
            }
        }
    }
}