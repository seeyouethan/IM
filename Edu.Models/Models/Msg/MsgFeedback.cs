using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Models.Models.Msg
{
    /// <summary>
    /// SignalR消息回执的实体
    /// </summary>
    public class MsgFeedback
    {
        public bool Success { get; set; }
        public string Content { get; set; }
        public string Error { get; set; }
    }
}