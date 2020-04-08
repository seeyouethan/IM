using System;
using System.Collections.Generic;
using System.Web.Http;
using Edu.Models.IM;
using Edu.Models.Models;
using Edu.Service;
using Edu.Service.Service;
using Edu.Tools;
using EduIM.WebAPI.Filters;
using log4net;
using Newtonsoft.Json;

namespace EduIM.WebAPI.Controllers
{

    [NoAccessToken]
    public class PmcApiToImApiController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private readonly ssoUser _ssoUserOfWork = new ssoUser();

        readonly ILog _log = LogManager.GetLogger(typeof(MainApiController));

        /// <summary>
        /// 获取当前用户所在的群组
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupList(string userId)
        {
            try
            {
                var chatGroupList = new List<MyGroup>();
                var list = new List<MyGroup>();
                /*
                 * 1.发送请求，获取工作群
                 */              
                var unitid = "";
                var url = ConfigHelper.GetConfigString("GetMyGroups") + "?unitID=" + unitid + "&userID=" + userId + "&pageSize=999";
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultJsonContent result = JsonConvert.DeserializeObject<PmcJsonResultJsonContent>(resp);


                //获取所有
                return Json(
                    new
                    {
                        Success = true,
                        Content = result.Content,
                        Error = "",
                        Message = "查询成功",
                        Count = result.Count,
                        Total = result.Total,
                    });

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 获取工作群成员
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupMembers(string groupID)
        {
            try
            {
                /*
                 * 1.发送请求，获取工作群组成员
                 */
                var url = ConfigHelper.GetConfigString("GetMyGroupMembers") + "?groupID=" + groupID;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultWeb result = JsonConvert.DeserializeObject<PmcJsonResultWeb>(resp);


                //获取所有
                return Json(
                    new
                    {
                        Success = true,
                        Data = result.Data,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 获取工作群成员消息 GetGroupInfoWebApi
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupInfo(string groupID,string userID)
        {
            try
            {
                /*
                 * 1.发送请求，获取工作群组成员
                 */
                var url = ConfigHelper.GetConfigString("GetGroupInfoWebApi") + "?groupID=" + groupID+ "&userID="+userID;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultWebApi result = JsonConvert.DeserializeObject<PmcJsonResultWebApi>(resp);


                //获取所有
                return Json(
                    new
                    {
                        Success = result.Success,
                        Content = result.Content,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 群组消息通知接口1同意/拒绝入群申请 GroupMsg01
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult UpdateStatus(string id, string status)
        {
            try
            {
                /*
                 * 1.发送请求，获取工作群组成员
                 */
                var url = ConfigHelper.GetConfigString("GroupMsg01") + "?id=" + id + "&status=" + status;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultWebApi result = JsonConvert.DeserializeObject<PmcJsonResultWebApi>(resp);


                //获取所有
                return Json(
                    new
                    {
                        Success = result.Success,
                        Content = result.Content,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 群组消息通知接口2获取我收到的申请列表 GroupMsg02
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetUserGroupApply(string userId)
        {
            try
            {
                /*
                 * 1.发送请求，获取工作群组成员
                 */
                var url = ConfigHelper.GetConfigString("GroupMsg02") + "?userId=" + userId;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultWebApi result = JsonConvert.DeserializeObject<PmcJsonResultWebApi>(resp);


                //获取所有
                return Json(
                    new
                    {
                        Success = result.Success,
                        Content = result.Content,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 群组消息通知接口3获取我的申请列表 GroupMsg03
        /// </summary>
        /// <param name="userId">当前用户ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetUserApplyResult(string userId)
        {
            try
            {
                /*
                 * 1.发送请求，获取工作群组成员
                 */
                var url = ConfigHelper.GetConfigString("GroupMsg03") + "?userId=" + userId;
                var resp = HttpWebHelper.HttpGet(url);

                PmcJsonResultWebApi result = JsonConvert.DeserializeObject<PmcJsonResultWebApi>(resp);


                //获取所有
                return Json(
                    new
                    {
                        Success = result.Success,
                        Content = result.Content,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });

            }
            catch (Exception ex)
            {
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "查询失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



    }
}