namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImGroup")]
    public partial class ImGroup
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// 群组名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 群组头像照片
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 群组描述
        /// </summary>
        public string Des { get; set; }
        /// <summary>
        /// 群组创建者uid
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 群组创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// TypeID=0表示工作群组 TypeID=1表示自己建立的讨论群组
        /// </summary>
        public int? TypeID { get; set; }
    }
}