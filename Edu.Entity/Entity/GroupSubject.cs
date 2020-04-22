namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GroupSubject")]
    public partial class GroupSubject
    {
        public int id { get; set; }
        public string groupid { get; set; }
        /// <summary>
        /// 主题名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 创建者ID
        /// </summary>
        public string creator { get; set; }
        /// <summary>
        /// 删除标记
        /// </summary>
        public int isdel { get; set; }
        /// <summary>
        /// 结束标记（已经弃用）
        /// </summary>
        public int isend { get; set; }
        /// <summary>
        /// 结束时间（作为删除时间使用）
        /// </summary>
        public string endtime { get; set; }
        /// <summary>
        /// 其他(备用)
        /// </summary>
        public string remark { get; set; }
    }
}
