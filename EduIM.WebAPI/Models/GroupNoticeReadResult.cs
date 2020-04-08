using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EduIM.WebAPI.Models
{
    public class GroupNoticeReadResult
    {
        

        /// <summary>
        /// 消息ID
        /// </summary>
        public string msgID { get; set; }

        /// <summary>
        /// 应该读该消息的人数
        /// </summary>
        public string count { get; set; }

        /// <summary>
        /// 已经读了消息的人数
        /// </summary>
        public int readCount { get; set; }

        /// <summary>
        /// 未读该消息的人数
        /// </summary>
        public int unreadCount { get; set; }

        public List<ReadNoticePerson> readPersonList { get; set; }
    }

    public class ReadNoticePerson
    {
        /// <summary>
        /// 用户id
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string realName { get; set; }
        /// <summary>
        /// 读取该通知的时间
        /// </summary>
        public string readDate { get; set; }
    }
}