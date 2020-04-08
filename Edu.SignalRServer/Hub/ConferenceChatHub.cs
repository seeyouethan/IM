using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using Microsoft.AspNet.SignalR.Hubs;
using Edu.Tools;
using Edu.Models.Models;
using Edu.Models.Models.Msg;
using Edu.SignalRServer.Service;

namespace Edu.SignalRServer.Hub
{
    [HubName("conferenceChatHub")]
    [EnableCors("*", "*", "*")]
    public class ConferenceChatHub : Microsoft.AspNet.SignalR.Hub
    {

        public override Task OnConnected()
        {
            var conferenceid = Context.QueryString["conferenceid"];
            var uid = Context.QueryString["uid"];

            var onlineUsers = RedisHelper.Hash_Get<List<UserOnLine>>("A_IM_Conference_OnlineUser", conferenceid) ?? new List<UserOnLine>();
            var model = onlineUsers.Find(p => p.uid == uid);
            if (model != null)
            {
                //更新时间
                model.CreateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff");
            }
            else
            {
                //新建一条数据
                var item = new UserOnLine()
                {
                    ConnectionId = Context.ConnectionId,
                    CreateDate = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"),
                    deviceid = string.Empty,
                    devicetype = "web",
                    deviceToken = string.Empty,
                    uid = uid,
                };
                onlineUsers.Add(item);
            }

            RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser", conferenceid);
            RedisHelper.Hash_Set("A_IM_Conference_OnlineUser", conferenceid, onlineUsers);

            foreach (var onlineUser in onlineUsers)
            {
                if (onlineUser.uid != uid)
                {
                    Clients.Client(onlineUser.ConnectionId).notifyOtherUserOnline(uid);
                }
                //Clients.Group()
            }

            return Clients.Caller.notifySelfUserOnline();
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            var conferenceid = Context.QueryString["conferenceid"];
            var uid = Context.QueryString["uid"];
            //移除缓存记录
            var onlineUsers = RedisHelper.Hash_Get<List<UserOnLine>>("A_IM_Conference_OnlineUser", conferenceid) ?? new List<UserOnLine>();
            var model = onlineUsers.Find(p => p.uid == uid);

            if (model != null)
            {
                onlineUsers.Remove(model);
            }

            RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser", conferenceid);
            RedisHelper.Hash_Set("A_IM_Conference_OnlineUser", conferenceid, onlineUsers);

            //查看自己是否正在分享，若在分享，则删除对应的缓存记录
            RemoveUserFromConferenceLiveUserList(conferenceid, uid);

            foreach (var onlineUser in onlineUsers)
            {
                Clients.Client(onlineUser.ConnectionId).notifyOtherUserOffline(uid);
            }

            //告诉自己
            return base.OnDisconnected(stopCalled);
        }
        public override Task OnReconnected()
        {
            return Clients.Caller.notifyMessage("User Reconnect:"+Context.ConnectionId);
        }


        public void Chat(ConferenceChatModel model)
        {

        }


        public Task OnLive()
        {
            return null;
        }

        public IEnumerable<ConferenceLiveUserModel> GetOnLiveUsers()
        {
            var conferenceid = Context.QueryString["conferenceid"];
            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_OnlineUser_OnLive", conferenceid) ?? new List<ConferenceLiveUserModel>(); 
            return list;
        }

        public IEnumerable<ConferenceLiveUserModel> SetOnLiveUser(string type)
        {
            var conferenceid = Context.QueryString["conferenceid"];
            var uid = Context.QueryString["uid"];
            var list = CreateOrUpdateConferenceLiveUserList(conferenceid, uid, type);
            return list;
        }

        public IEnumerable<ConferenceLiveUserModel> RemoveOnLiveUser()
        {
            var conferenceid = Context.QueryString["conferenceid"];
            var uid = Context.QueryString["uid"];
            var list = RemoveUserFromConferenceLiveUserList(conferenceid, uid);

            return list;
        }


        /// <summary>
        /// 开始分享
        /// </summary>
        /// <param name="conferenceid"></param>
        /// <param name="uid"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IEnumerable<ConferenceLiveUserModel> CreateOrUpdateConferenceLiveUserList(string conferenceid,string uid,string type)
        {
            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_OnlineUser_OnLive",
                           conferenceid) ?? new List<ConferenceLiveUserModel>();
            
            var model = list.Find(p => p.uid == uid);
            if (model != null)
            {
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
                    uid=uid,
                };
                list.Add(newModel);
            }
            RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser_OnLive", conferenceid);
            RedisHelper.Hash_Set("A_IM_Conference_OnlineUser_OnLive", conferenceid, list);
            //发通知给群里所有人，当前正在分享的人数
            Clients.OthersInGroup(conferenceid).notifyOnLiveUsers(list);
            //记录
            RecordUserLive(conferenceid, uid, type, "online");

            return list;
        }


        
        public IEnumerable<ConferenceLiveUserModel> RemoveUserFromConferenceLiveUserList(string conferenceid, string uid)
        {
            var list = RedisHelper.Hash_Get<List<ConferenceLiveUserModel>>("A_IM_Conference_OnlineUser_OnLive", conferenceid) ?? new List<ConferenceLiveUserModel>();

            var model = list.Find(p => p.uid == uid);
            if (model != null)
            {
                list.Remove(model);
                RedisHelper.Hash_Remove("A_IM_Conference_OnlineUser_OnLive", conferenceid);
                RedisHelper.Hash_Set("A_IM_Conference_OnlineUser_OnLive", conferenceid, list);
            }

            //发通知给群里所有人，当前正在分享的人数
            Clients.OthersInGroup(conferenceid).notifyOnLiveUsers(list);
            //记录
            RecordUserLive(conferenceid, uid, "", "offline");

            return list;
        }



        public void RecordUserLive(string conferenceid, string uid,string type,string ext)
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
    }


}