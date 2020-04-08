using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models
{
    /// <summary>
    /// 用户加入组
    /// </summary>
    public class IMGroup
    {
        /// <summary>
        /// 用户连接signalR的身份
        /// </summary>
        public string ConnectionID { get; set; }
        /// <summary>
        /// SSO中获取的UserID
        /// </summary>
        public string UserID { get; set; }
        /// <summary>
        /// 讨论组编号
        /// </summary>
        public string ID { get; set; }



    }
}
