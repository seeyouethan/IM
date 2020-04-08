namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PlanTop")]
    public partial class PlanTop
    {
        public int ID { get; set; }
        public string Guid { get; set; }
        public string PlanID { get; set; }
        public string Creator { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Remark { get; set; }
    }
}
