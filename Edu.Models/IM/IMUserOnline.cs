using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models
{
    public class IMUserOnline
    {
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string TrueName { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string HeadPhoto { get; set; }
        
        /// <summary>
        /// 用户连接signalr的身份
        /// </summary>
        public string ConnectionID { get; set; }
        /// <summary>
        /// 用户SSO中userID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 用户是否在线 0：下线 1：在线
        /// </summary>
        public int IsOnline { get; set; }

    }
}
