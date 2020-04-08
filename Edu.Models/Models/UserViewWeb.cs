using System;

namespace Edu.Models.Models
{
    public class UserViewWeb
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 该字段表示发生最近一次发生聊天记录的时间(如果有未读消息的话)
        /// </summary>
        public DateTime LatestMsgtime { get; set; }
        /// <summary>
        /// 未读消息个数
        /// </summary>
        public int UnreadMsgCount { get; set; }

        /// <summary>
        /// 如果已经连接上signalr那么这个值表示connid （一般用来处理离线的时候用到）
        /// </summary>
        public string Connid { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string Logo { get; set; }

        public string Department { get; set; }

    }
}