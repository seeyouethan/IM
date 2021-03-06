﻿namespace Edu.Models.Models
{
    /// <summary>
    /// 聊天历史记录
    /// </summary>
    public class ChatHistoryView
    {
        /// <summary>
        /// 标识ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 发送者ID
        /// </summary>
        public string FromID { get; set; }
        /// <summary>
        /// 接收者ID
        /// </summary>
        public string ToID { get; set; }
        /// <summary>
        /// 聊天内容
        /// </summary>
        public string Msg { get; set; }
        /// <summary>
        /// 创建时间/发送时间
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// 发送者真实姓名
        /// </summary>
        public string FromUserTrueName { get; set; }
        /// <summary>
        /// 接收者真实姓名
        /// </summary>
        public string ToUserTrueName { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 新增
        /// </summary>
        public string Ext { get; set; }
        
    }
}