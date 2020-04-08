using System;
using System.Web.Http;
using Edu.Service;
using Edu.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Edu.Models.Models.Msg;
using Edu.Service.Service.Message;
using EduIM.WebAPI.Filters;
using Edu.Entity;

namespace EduIM.WebAPI.Controllers
{

    [NoAccessToken]
    public class ImApiController : ApiController
    {
        private readonly ssoUser _ssoUserOfWork = new ssoUser();
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();

        /// <summary>
        /// 获取历史聊天消息记录
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="datetime"></param>
        /// <param name="isgroup"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetHistoryMessage(string fromuid, string touid, string datetime, string isgroup)
        {
            try
            {
                var pageSize = 5;
                var dt = Convert.ToDateTime(datetime);
                var list = new List<Msg>();
                var query= new List<IMMsg>();
                if (isgroup == "1")
                {
                    query =
                     _unitOfWork.DIMMsg.Get(
                         p => (p.TouID == touid && p.isgroup == 1 && p.CreateDate < dt && p.IsDel != 1)).OrderByDescending(p => p.ID).Take(pageSize).ToList();
                }
                else
                {
                    query = _unitOfWork.DIMMsg.Get(p => ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.CreateDate < dt && p.IsDel != 1).OrderByDescending(p => p.ID).Take(pageSize).ToList();                   
                }

                foreach (var msg in query)
                {
                    var singleMsg = MsgServices.ImMsg2Msg(msg);
                    list.Add(singleMsg);
                }
                return Json(new { Success = true, Content = list, Error = "", Message = "", Count = list.Count, Total = list.Count });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 获取未读的聊天消息
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="isgroup"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetUnreadMessage(string fromuid, string touid, string isgroup)
        {
            try
            {
                var list = new List<Msg>();
                if (isgroup == "1")
                {
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid + fromuid);
                    if (all != null && all.Any())
                    {
                        list = all;
                        /*从缓存中删除这些消息*/
                        Task.Run(() =>
                        {
                            RedisHelper.Hash_Remove("IMGroupMsg", touid + fromuid);
                        });
                    }
                }
                else
                {
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", fromuid);
                    if (all != null && all.Any())
                    {
                        list = all.Where(p => p.fromuid == touid).ToList();
                    }
                    if (list.Any())
                    {
                        /*从缓存中删除这些消息*/
                        foreach (var msg in list)
                        {
                            all.Remove(msg);
                        }
                        Task.Run(() =>
                        {
                            RedisHelper.Hash_Remove("IMMsg", fromuid);
                            MsgServices.ResetRedisKeyValue("IMMsg", fromuid, all);
                        });
                    }
                }
                return Json(new { Success = true, Content = list, Error = "", Message = "", Count = list.Count, Total = list.Count });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询失败", Count = 0, Total = 0 });
            }
        }


        /// <summary>
        /// 获取最近聊天记录/向上滚动刷新获取数据，
        /// </summary>
        /// <param name="fromuid"></param>
        /// <param name="touid"></param>
        /// <param name="isgroup"></param>
        /// <param name="datetime"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetRecentlyChatMessage(string fromuid, string touid,int isgroup,string datetime)
        {
            try
            {
                var listResult = new List<Msg>();
                if (string.IsNullOrEmpty(datetime)) {
                    if (isgroup == 1)
                    {
                        //先从缓存中取未读的消息，如果缓存中没有未读的消息，那么再从数据库中取最近的10条聊天记录
                        var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid + fromuid);
                        if (all != null && all.Any())
                        {
                            /*从缓存中删除这些消息*/
                            RedisHelper.Hash_Remove("IMGroupMsg", touid + fromuid);
                            listResult.AddRange(all);
                        }
                        else
                        {
                            var query =
                                     _unitOfWork.DIMMsg.Get(
                                         p => (p.TouID == touid && p.isgroup == 1 && p.IsDel != 1)).OrderByDescending(p => p.ID).Take(10);
                            foreach (var msg in query)
                            {
                                var singleMsg = MsgServices.ImMsg2Msg(msg);
                                listResult.Add(singleMsg);
                            }
                        }
                    }
                    else
                    {
                        // 先从缓存中取未读的消息，如果缓存中没有未读的消息，那么再从数据库中取最近的10条聊天记录
                        var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", fromuid);
                        if (all != null && all.Any())
                        {
                            var queryUnreadMsg =
                            all.Where(p => p.fromuid == touid);
                            var unreadMsg = queryUnreadMsg as Msg[] ?? queryUnreadMsg.ToArray();
                            if (unreadMsg.Any())
                            {
                                /*从缓存中删除这些消息*/
                                foreach (var msg in unreadMsg)
                                {
                                    all.Remove(msg);
                                }
                                RedisHelper.Hash_Remove("IMMsg", fromuid);
                                MsgServices.ResetRedisKeyValue<Msg>("IMMsg", fromuid, all);
                            }
                            listResult.AddRange(unreadMsg.ToList());
                        }
                        else
                        {
                            var query =
                            _unitOfWork.DIMMsg.Get(
                                p => ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid)) && p.IsDel != 1).OrderByDescending(p => p.ID).Take(10);

                            foreach (var msg in query)
                            {
                                var singleMsg = MsgServices.ImMsg2Msg(msg);
                                listResult.Add(singleMsg);
                            }
                        }
                    }
                }
                else
                {
                    var dt = Convert.ToDateTime(datetime);
                    if (isgroup == 1) {
                        var query =
                                     _unitOfWork.DIMMsg.Get(
                                         p => (p.TouID == touid && p.isgroup == 1 && p.CreateDate < dt && p.IsDel != 1)).OrderByDescending(p => p.ID).Take(10);
                        foreach (var msg in query)
                        {
                            var singleMsg = MsgServices.ImMsg2Msg(msg);
                            listResult.Add(singleMsg);
                        }
                    }
                    else
                    {
                        var query =
                              _unitOfWork.DIMMsg.Get(
                                  p => ((p.FromuID == fromuid && p.TouID == touid) || (p.FromuID == touid && p.TouID == fromuid))&& p.CreateDate < dt && p.IsDel != 1).OrderByDescending(p => p.ID).Take(10);
                        foreach (var msg in query)
                        {
                            var singleMsg = MsgServices.ImMsg2Msg(msg);
                            listResult.Add(singleMsg);
                        }
                    }
                }


                //listResult = listResult.OrderBy(p => Convert.ToInt32(p.id0)).ToList();

                
                
                
                return Json(new { Success = true, Content = listResult, Error = "", Message = "", Count = listResult.Count, Total = listResult.Count });
            }
            catch (Exception ex)
            {
                return Json(new { Success = false, Content = "", Error = ex.ToString(), Message = "查询失败", Count = 0, Total = 0 });
            }
        }





    }
}