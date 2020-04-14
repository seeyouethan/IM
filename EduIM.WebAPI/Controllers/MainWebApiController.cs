using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Edu.Models.Models.Msg;
using Edu.Service;
using Edu.Tools;
using Edu.Service.Service.Message;
using System.Threading.Tasks;
using System;
using EduIM.WebAPI.Service;
using EduIM.WebAPI.Models;
using Edu.Entity;
using System.Web;
using Edu.Models.Models;

namespace EduIM.WebAPI.Controllers
{

    //[WebApiTracker]
    public class MainWebApiController : ApiController
    {
        private readonly UnitOfWork _unitOfWork = new UnitOfWork();
        private readonly ssoUser _ssoUserOfWork = new ssoUser();


        /// <summary>
        /// 初始加载时的最近聊天记录,不分主题
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="touid"></param>
        /// <param name="isgroup"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult RecentlyChat(string uid, string touid, int isgroup)
        {
            try
            {
                //先从缓存中取未读的消息，如果有缓存消息，则返回所有的缓存消息，
                //如果缓存中没有未读的消息，那么再从数据库中取最近的10条聊天记录
                var result = new List<Msg>() { };
                if (isgroup == 1)
                {
                    //------群组消息--start------
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMGroupMsg", touid + uid);
                    if (all != null && all.Any())
                    {
                        Task.Run(() =>
                        {
                            //异步执行
                            //从缓存中删除这些消息
                            RedisHelper.Hash_Remove("IMGroupMsg", touid + uid);
                        });
                        //result = all;
                    }
                    else
                    {
                        var query = _unitOfWork.DIMMsg.Get(
                                    p => (p.TouID == touid && p.IsDel != 1)).OrderByDescending(p => p.ID);
                        if(query!=null && query.Any())
                        {
                            result= MsgServices.ImMsg2Msg(query.Take(10).ToList());
                        }
                    }
                    //------群组消息--end------
                }
                else
                {
                    //------单人聊天消息--start------
                    var all = MsgServices.GetRedisKeyValue<Msg>("IMMsg", uid);
                    if (all != null && all.Any())
                    {
                        var queryUnreadMsg = all.Where(p => p.fromuid == touid);
                        if (queryUnreadMsg != null && queryUnreadMsg.Any())
                        {
                            result = queryUnreadMsg.ToList();
                        }
                    }
                    if (result.Any())
                    {
                        //1.从缓存中取
                        Task.Run(() =>
                        {
                            //异步执行
                            //从缓存中删除这些消息
                            foreach (var msg in result)
                            {
                                all.Remove(msg);
                            }
                            RedisHelper.Hash_Remove("IMMsg", uid);
                            MsgServices.ResetRedisKeyValue<Msg>("IMMsg", uid, all);
                        });
                    }
                    else
                    {
                        //2.从数据库中取
                        var query = _unitOfWork.DIMMsg.Get(
                        p => ((p.FromuID == uid && p.TouID == touid) || (p.FromuID == touid && p.TouID == uid)) && p.IsDel != 1).OrderByDescending(p => p.ID);
                        if (query != null && query.Any())
                        {
                            result = MsgServices.ImMsg2Msg(query.Take(10).OrderBy(p=>p.ID).ToList());
                        }
                    }

                    //------单人聊天消息--end------
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = result,
                        Error = "",
                        Message = "查询成功",
                        Count = result.Count,
                        Total = result.Count
                    });
            }
            catch(Exception ex)
            {
                LoggerHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0,
                    });
            }
            
        }

        /// <summary>
        /// 获取聊天历史记录
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="touid"></param>
        /// <param name="keywords"></param>
        /// <param name="pageSize"></param>
        /// <param name="lastid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetChatHistory(string uid, string touid, string keywords, int pageSize, int lastid,string subjectId)
        {
            try
            { 
                var result = new List<Msg>() { };
                var totalCount = 0;

                if (!string.IsNullOrEmpty(keywords))
                {
                    //关键字查询,模糊查询
                    
                }
                else
                {
                    //没有关键字查询，查询全部
                    var query = _unitOfWork.DIMMsg.GetIQueryable(p => ((p.FromuID == uid && p.TouID == touid && p.isgroup == 0) || (p.FromuID == touid && p.TouID == uid && p.isgroup == 0) || (p.TouID == touid && p.isgroup == 1)) && p.IsDel != 1 && (p.ID < lastid)).OrderByDescending(p => p.ID);
                    if (subjectId != "0")
                    {
                        query = query.Where(p => p.SubjectId == subjectId).OrderByDescending(p => p.ID);
                    }
                    

                    
                    if(query!=null && query.Any())
                    {
                        totalCount = query.Count();
                        result = MsgServices.ImMsg2Msg(query.Take(pageSize).ToList());
                    }
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = result,
                        Error = "",
                        Message = "查询成功",
                        Count = result.Count,
                        Total = totalCount,
                    });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0,
                    });
            }
        }
        
        /// <summary>
        /// 获取在线状态
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetOnlineStatus(string uid)
        {
            try
            {
                var result = false;
                /*1.oaokcs端在线*/
                var isonline = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine", uid);
                /*2.oa端在线*/
                var isonline_oa = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLine_OA", uid);
                /*3.app端在线*/
                var isonline_app = RedisHelper.Hash_Get<UserOnLine>("IMUserOnLineApp", uid);

                if (isonline == null && isonline_oa == null && isonline_app == null)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
                return Json(
                    new
                    {
                        Success = true,
                        Content = result,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0,
                    });
            }
        }

        /// <summary>
        /// web页面聊天输入窗口输入文本 ，通过这个方法，返回对应的类型
        /// 0 纯文本 1单张图片 3 图文混合
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetMessageType(string message)
        {
            try
            {
                var result = new TextAndImageMessage { imgList = new List<string>(), msg = string.Empty, type = 0, };
                var msgContent = string.Empty;
                var msgType = 0;
                var imgList = new List<string>();
                MsgServices.GetMessageType(message, out msgContent, out msgType, out imgList);

                //去掉html标签
                string strText = System.Text.RegularExpressions.Regex.Replace(msgContent, @"<[^>]*>", "");
                strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

                result.imgList = imgList;
                result.msg = msgContent;
                result.type = msgType;

                return Json(
                    new
                    {
                        Success = true,
                        Content = result,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0,
                    });
            }
        }



        public IHttpActionResult GetWorkGroup(string message)
        {
            try
            {
                var result = new TextAndImageMessage { imgList = new List<string>(), msg = string.Empty, type = 0, };
                var msgContent = string.Empty;
                var msgType = 0;
                var imgList = new List<string>();
                MsgServices.GetMessageType(message, out msgContent, out msgType, out imgList);

                //去掉html标签
                string strText = System.Text.RegularExpressions.Regex.Replace(msgContent, @"<[^>]*>", "");
                strText = System.Text.RegularExpressions.Regex.Replace(strText, "&[^;]+;", "");

                result.imgList = imgList;
                result.msg = msgContent;
                result.type = msgType;

                return Json(
                    new
                    {
                        Success = true,
                        Content = result,
                        Error = "",
                        Message = "查询成功",
                        Count = 0,
                        Total = 0,
                    });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(System.Reflection.MethodBase.GetCurrentMethod().Name, ex);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.Message,
                        Message = "查询失败",
                        Count = 0,
                        Total = 0,
                    });
            }
        }



        /// <summary>
        /// 群组中新建主题
        /// </summary>       
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult AddGroupSubject([FromBody]GroupSubject subject)
        {
            try
            {



                var model = new GroupSubject();
                model.groupid = subject.groupid;
                model.name = subject.name;
                model.createtime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                model.isdel = 0;
                model.isend = 0;
                model.creator = subject.creator;
                model.remark = string.Empty;
                model.endtime = string.Empty;
                _unitOfWork.DGroupSubject.Insert(model);

                var result = _unitOfWork.Save();
                if (result.ResultType == OperationResultType.Success)
                {
                    return Json(new
                    {
                        Success = true,
                        Content = model,
                        Error = "",
                        Message = "操作成功",
                        Count = 0,
                        Total = 0
                    });
                }
                
                LoggerHelper.Error(result.ToString());
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
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }


        /// <summary>
        /// 查询群组主题（未结束的，有效的主题）
        /// </summary>       
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetGroupSubject(string goupId)
        {
            try
            {
                var result = new List<GroupSubject>() { };
                if (string.IsNullOrEmpty(goupId))
                {
                    return Json(new
                    {
                        Success = false,
                        Content = result,
                        Error = "",
                        Message = "群组id不能为空",
                        Count = 0,
                        Total = 0
                    });
                }
                var query = _unitOfWork.DGroupSubject.Get(p => p.isdel == 0 && p.groupid == goupId && p.isend != 1).OrderByDescending(p => p.id); ;
                if(query!=null && query.Any())
                {
                    result = query.ToList();
                }
               

                return Json(new
                {
                    Success = true,
                    Content = result,
                    Error = "",
                    Message = "查询成功",
                    Count = result.Count,
                    Total = result.Count
                });
            }
            catch (Exception ex)
            {
                LoggerHelper.Error(ex.Message);
                return Json(
                    new
                    {
                        Success = false,
                        Content = "",
                        Error = ex.ToString(),
                        Message = "操作失败",
                        Count = 0,
                        Total = 0
                    });
            }
        }



        }
    }