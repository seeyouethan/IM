using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.SignalRServer.Models
{
    public class DisconnetMsg
    {
        /// <summary>
        /// 0表示出现链接超时断开，1表示客户端关闭连接
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// 消息描述
        /// </summary>
        public string msg { get; set; }
    }
}
