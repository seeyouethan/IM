using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Models.Models.Msg;

namespace Edu.Models.IM
{
    public class RecentlyChatGroupNew
    {
        public string groupname { get; set; }
        /// <summary>
        /// 群组ID
        /// </summary>
        public string groupid { get; set; }
        /// <summary>
        /// 未读消息个数
        /// </summary>
        public int unreadmsgcount { get; set; }
        /// <summary>
        /// 最近一条的未读消息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 最近一条的未读消息类型，0表示文本，1表示图片,2表示文件，3表示图文混合消息，4表示语音消息
        /// </summary>
        public int msgtype { get; set; }

        /// <summary>
        /// 最近一条消息的时间 
        /// </summary>
        public string msgtime { get; set; }
        /// <summary>
        /// 最近一条消息的发送者的真实姓名
        /// </summary>
        public string msgtruename { get; set; }
        /// <summary>
        /// 2018年12月26日新增，最近聊天的10条消息，只取10条
        /// </summary>
        public List<Msg> msglist { get; set; }
        public int msgcount { get; set; }


        /// <summary>
        /// 群组创建者id 2019年8月12日  新增
        /// </summary>
        public string creator { get; set; }
        /// <summary>
        /// 群组创建者真实姓名   2019年8月12日  新增
        /// </summary>
        public string creatorname { get; set; }
    }
}
