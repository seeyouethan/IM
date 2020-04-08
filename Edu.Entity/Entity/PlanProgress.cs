namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PlanProgress")]
    public partial class PlanProgress
    {
        public int ID { get; set; }

        [StringLength(50)]
        public string Guid { get; set; }

        [StringLength(255)]
        public string Creator { get; set; }

        public DateTime? CreateDate { get; set; }

        [Column(TypeName = "text")]
        public string Content { get; set; }

        public int? PlanID { get; set; }

        [StringLength(50)]
        public string PlanGuid { get; set; }

        public int CurProgress { get; set; }
        public string Files { get; set; }
        public int? IsDel { get; set; }
    }
}
