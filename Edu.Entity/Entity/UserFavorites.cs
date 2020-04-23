namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// 
    /// </summary>
    [Table("UserFavorites")]
    public partial class UserFavorites
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// 收藏者ID
        /// </summary>
        public string uid { get; set; }
        
        /// <summary>
        /// 创建收藏的时间
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 具体内容
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// 内容关键id
        /// </summary>
        public string cid { get; set; }

        /// <summary>
        /// 类型 1表示聊天消息
        /// </summary>
        public int type { get; set; }
        
        /// <summary>
        /// 备注，备用字段
        /// </summary>
        public string remark { get; set; }

    }
}
