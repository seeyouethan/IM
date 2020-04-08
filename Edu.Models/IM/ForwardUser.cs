using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Models.Models.Msg;

namespace Edu.Models.IM
{
    /// <summary>
    /// 最近联系人
    /// </summary>
    public class ForwardUser
    {
        /// <summary>
        /// 用户id   若为群组，则表示群组的id(头像根据id去拼接)
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 真实姓名  若为群组，则表示群组的名称
        /// </summary>
        public string realname { get; set; }
        /// <summary>
        /// 是否是群组  0表示否，1表示是
        /// </summary>
        public int isgroup { get; set; }
        /// <summary>
        /// 最近一次消息聊天的时间
        /// </summary>
        public string msgtime { get; set; }
    }
}
