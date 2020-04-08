namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// 记录个推日志的数据库
    /// </summary>
    [Table("GetuiLog")]
    public partial class GetuiLog
    {
        public int id { get; set; }

        public string fromuid { get; set; }

        public string touid { get; set; }

        /// <summary>
        /// 消息时间
        /// </summary>
        public string msgtime { get; set; }
        
        /// <summary>
        /// 收到回执时间
        /// </summary>
        public string resulttime { get; set; }

        /// <summary>
        /// 具体内容
        /// </summary>
        public string content
        {
            get; set;
        }
       
        /// <summary>
        /// 类型 0表示聊天消息
        /// </summary>
        public int type
        {
            get; set;
        }
        /// <summary>
        /// 其他消息id
        /// </summary>
        public string msgid
        {
            get; set;
        }
        /// <summary>
        /// 设备类型
        /// </summary>
        public string devicetype
        {
            get; set;
        }
        /// <summary>
        /// 设备id
        /// </summary>
        public string deviceid
        {
            get; set;
        }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark
        {
            get; set;
        }

    }
}
