using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.IM
{
    /// <summary>
    /// 用于反馈发送的消息
    /// </summary>
    public class MessageFeedback
    {
        public int id { get; set; }
        public string content { get; set; }
        public string createtime { get; set; }
        public string touid { get; set; }
        public string fromuid { get; set; }
    }
}
