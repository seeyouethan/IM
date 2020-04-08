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
    public class RecentlyChatUserNew
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string userid { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string username { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string photo { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string realname { get; set; }
        /// <summary>
        /// 未读消息个数
        /// </summary>
        public int unreadmsgcount { get; set; }
        /// <summary>
        /// 最近一条的未读消息
        /// </summary>
        public string msg { get; set; }

        /// <summary>
        /// 最近一条消息的时间 
        /// </summary>
        public string msgtime { get; set; }
        /// <summary>
        /// 最近一条的未读消息类型，0表示文本，1表示图片，2表示文件，3表示图文混合消息，4表示语音消息
        /// </summary>
        public int msgtype { get; set; }
        /// <summary>
        /// 2018年12月26日新增，最近聊天的10条消息，只取10条，如果时间不为空，则取从该时间到现在的数据条数
        /// </summary>
        public List<Msg>  msglist { get; set; }
        public int msgcount { get; set; } 
    }
}
