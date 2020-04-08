using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models.Msg
{
    /// <summary>
    /// 群组视频会议中右侧的讨论Model
    /// </summary>
    public class ConferenceChatModel
    {
        /// <summary>
        /// 会议ID
        /// </summary>
        public string conferenceId { get; set; }
        /// <summary>
        /// 消息发送者的userid
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 消息发送者的真实姓名
        /// </summary>
        public string trueName { get; set; }
        /// <summary>
        /// 消息发送者的头像
        /// </summary>
        public string photo { get; set; }
        /// <summary>
        /// 消息主题内容
        /// </summary>
        public string msgContent { get; set; }
        /// <summary>
        /// 消息类型，默认为0 表示文本字符串
        /// </summary>
        public int? msgType { get; set; } = 0;
        /// <summary>
        /// 消息发送的时间
        /// </summary>
        public string dateTime { get; set; }

        /// <summary>
        /// 其他(备用，待扩展)
        /// </summary>
        public string ext { get; set; }
    }
}
