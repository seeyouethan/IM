using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using Edu.Tools;

namespace EduIM.ChatServer
{

    public class ServerHub : Hub
    {
        private static readonly char[] Constant =
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V',
            'W', 'X', 'Y', 'Z'
        };
        /// <summary>
        /// 产生随机用户名函数
        /// </summary>
        /// <param name="length">用户名长度</param>
        /// <returns></returns>
        public static string GenerateRandomName(int length)
        {
            var newRandom = new System.Text.StringBuilder(62);
            var rd = new Random();
            for (var i = 0; i < length; i++)
            {
                newRandom.Append(Constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }
        /// <summary>
        /// 供客户端调用的服务器端代码
        /// </summary>
        /// <param name="message"></param>
        public void Send(string message)
        {
            var name = GenerateRandomName(4);

            // 调用所有客户端的sendMessage方法
            Clients.All.sendMessage(name, message);
        }

        /// <summary>
        /// 用户连接时触发事件
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            Trace.WriteLine("客户端连接成功");
            // 在这添加你的代码.
            // 例如:在一个聊天程序中,记录当前连接的用户ID和名称,并标记用户在线.
            // 在该方法中的代码完成后,通知客户端建立连接,客户端代码
            // start().done(function(){//你的代码});
            LogHelper.Info(string.Format("OnConnected:当前连接用户:{0},ConnectionId:{1}", Edu.Service.LoginUserService.userName, Context.ConnectionId));
            return base.OnConnected();
        }

        public override Task OnDisconnected(bool iscon)
        {

            LogHelper.Info(string.Format("OnDisconnected:当前下线用户:{0},ConnectionId:{1}", Edu.Service.LoginUserService.userName, Context.ConnectionId));
            // 在这添加你的代码.
            // 例如: 标记用户离线
            // 删除连接ID与用户的关联.
            return base.OnDisconnected(iscon);
        }

        public override Task OnReconnected()
        {
            LogHelper.Info(string.Format("OnReconnected:当前从新连接用户:{0},ConnectionId:{1}", Edu.Service.LoginUserService.userName, Context.ConnectionId));
            // 在这添加你的代码.
            // 例如:你可以标记用户离线后重新连接,标记为在线
            return base.OnReconnected();
        }



    }
}