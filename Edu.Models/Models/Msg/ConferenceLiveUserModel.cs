using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models.Msg
{
    /// <summary>
    /// 群组视频会议中正在分享摄像头/桌面的Model
    /// </summary>
    public class ConferenceLiveUserModel
    {
        /// <summary>
        /// 会议ID
        /// </summary>
        public string conferenceid { get; set; }
        /// <summary>
        /// 消息发送者的userid
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 摄像头/桌面
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 帧率
        /// </summary>
        public string framerate { get; set; }
        /// <summary>
        /// 分辨率
        /// </summary>
        public string resolution { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public string datetime { get; set; }

        /// <summary>
        /// 其他(备用，待扩展) online 表示开始分享时间， offline表示结束分享时间
        /// </summary>
        public string ext { get; set; }
    }
}
