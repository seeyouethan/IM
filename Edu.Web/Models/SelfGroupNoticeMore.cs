using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    /// <summary>
    /// 用来向消息队列中添加通知，这个Model包含了新增成员，移除成员，xxx退出群组，xxx解散了群组
    /// </summary>
    public class SelfGroupNoticeMore
    { /// <summary>
      /// 群组ID
      /// </summary>
        public string groupid { get; set; }
        /// <summary>
        /// 群组名称
        /// </summary>
        public string groupname { get; set; }
        
        /// <summary>
        /// 群组成员（要通知的成员）
        /// </summary>
        public List<string> membersList { get; set; }
        /// <summary>
        /// 成员名称（新增成员xxx，移除成员xxx，xxx退出群组，xxx解散了群组）
        /// </summary>
        public string membername { get; set; }
        /// <summary>
        /// 0表示新增成员，1表示移除成员，2退出群组，3表示解散群组 4表示修改群组信息
        /// </summary>
        public int type { get; set; }
    }
}