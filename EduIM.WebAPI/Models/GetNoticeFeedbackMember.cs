using Edu.Models.Models.Msg;
using System.Collections.Generic;

namespace EduIM.WebAPI.Models
{
    public class NoticeFeedbackMember
    {
        public string userId { get; set; }
        public string realName { get; set; }
        //public string photo { get; set; }

            
        /// <summary>
        /// 如果已读，那么该字段表示读取的时间，用于排序
        /// 如果未读，该字段无用，为一个无限小的时间字符串
        /// </summary>
        public string readTime { get; set; }
    }

    public class NoticeFeedbackResult
    {

        public List<NoticeFeedbackMember> readList { get; set; }

        public List<NoticeFeedbackMember> unReadList { get; set; }
    }

    public class GroupNoticeMessage
    {
        public Msg msg { get; set; }
        public List<NoticeFeedbackMember> readList { get; set; }

        public List<NoticeFeedbackMember> unReadList { get; set; }
    }
}