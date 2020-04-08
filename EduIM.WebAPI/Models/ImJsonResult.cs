using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EduIM.WebAPI.Models
{
    public class EduJsonResult
    {
        public bool Success { get; set; }
        public object Content { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
    }

    /// <summary>
    /// 图文消息混合的消息Model
    /// </summary>
    public class TextAndImageMessage
    {
        public string msg { get; set; }
        public int type { get; set; }
        public List<string> imgList { get; set; }
    }
}