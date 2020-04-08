namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PlanFile")]
    public partial class PlanFile
    {
        public int ID { get; set; }
        
        public string Guid { get; set; }
        public string Creator { get; set; }
        public DateTime? CreateTime { get; set; }
        [Column(TypeName = "text")]
        public string FileUrl { get; set; }
        public string FileExtension { get; set; }
        public string FileName { get; set; }
        public string NewFileName { get; set; }
    }
}
