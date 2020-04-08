namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Plan")]
    public partial class Plan
    {
        public int ID { get; set; }

        [StringLength(50)]
        public string Guid { get; set; }

        [StringLength(500)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Creator { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string Adress { get; set; }

        [Column(TypeName = "text")]
        public string Content { get; set; }

        public int CallDate { get; set; }
        /// <summary>
        /// 完成度 100表示完成 
        /// </summary>
        public int Completing { get; set; }

        [StringLength(2000)]
        public string CompleteBZ { get; set; }
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompleteDate { get; set; }

        public string ParentID { get; set; }

        
        public string ManagerPerson { get; set; }

        [StringLength(255)]
        public string ExecutivesPerson { get; set; }

        /// <summary>
        /// 群组id
        /// </summary>
        [StringLength(50)]
        public string GroupID { get; set; }

        public int? Priority { get; set; }

        public int? isTop { get; set; }

        public DateTime? TopDate { get; set; }
        /// <summary>
        /// 创建者真实姓名
        /// </summary>
        public string CreatorTrueName { get; set; }
        /// <summary>
        /// 删除标志 0表示未删除 1表示删除
        /// </summary>
        public int isdel { get; set; }
        /// <summary>
        /// 是否已读，如果不是已读，那么在用户下次登录的时候，需要通知一下
        /// </summary>
        public int isread { get; set; }

        public string Fid { get; set; }
        public int Level { get; set; }
        /// <summary>
        /// 地址，2019年9月24日 新增，仅移动端使用
        /// </summary>
        public string Address { get; set; }
    }
}
