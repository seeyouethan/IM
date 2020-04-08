using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Edu.Models.Models.Msg;

namespace EduIM.WebAPI.Models
{
    public class RecentlyChatGroupSingle
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string groupid { get; set; }
        /// <summary>
        /// 群组名称
        /// </summary>
        public string groupname { get; set; }
        /// <summary>
        /// 最近一条的消息内容
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 最近一条的消息类型，0表示文本，1表示图片,2表示文件，3表示图文混合消息，4表示语音消息
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
        /// 未读消息个数
        /// </summary>
        public int unreadmsgcount { get; set; }
        /// <summary>
        /// 群组头像
        /// </summary>
        public string groupicon { get; set; }

        /// <summary>
        /// 如果有未读消息，这个字段中存放对应的未读消息
        /// </summary>
        public List<Msg> msglist { get; set; }
    }
}