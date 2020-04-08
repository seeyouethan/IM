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
using System.Threading.Tasks;

namespace EduIM.WebAPI.Controllers
{
    
    //[WebApiTracker]
    public class SignalRApiController : ApiController
    {
        /// <summary>
        /// 查询redis中的在线用户用户
        /// </summary>
        /// <param name="terminal">终端类型</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetOnlineUser(string terminal)
        {
            var list = new List<UserOnLine>();
            try
            {
                if (terminal == "oaokcs")
                {
                    list = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine");
                    if(list!= null && list.Any())
                    {
                        list = list.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
                    }
                }
                else if (terminal == "oa")
                {
                    list = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLine_OA");
                    if (list != null && list.Any())
                    {
                        list = list.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
                    }
                }
                else if (terminal == "app")
                {
                    list = RedisHelper.Hash_GetAll<UserOnLine>("IMUserOnLineApp");
                    if (list != null && list.Any())
                    {
                        list = list.OrderByDescending(p => Convert.ToDateTime(p.CreateDate)).ToList();
                    }
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = list,
                        Error = "",
                        Message = "查询成功",
                        Count = list.Count,
                        Total = list.Count,
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