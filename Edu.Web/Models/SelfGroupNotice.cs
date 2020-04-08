using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    /// <summary>
    /// 用来向消息队列中添加通知，这个Model仅仅指创建自建群的通知
    /// </summary>
    public class SelfGroupNotice
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
        /// 创建者
        /// </summary>
        public string creator { get; set; }
        /// <summary>
        /// 群组头像
        /// </summary>
        public string photo { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 群组成员
        /// </summary>
        public List<string> membersList { get; set; }
        /// <summary>
        /// 群组描述
        /// </summary>
        public string describe { get; set; }
    }
}