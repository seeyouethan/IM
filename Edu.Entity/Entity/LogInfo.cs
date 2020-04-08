namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("LogInfo")]
    public partial class LogInfo
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string TableName { get; set; }

        public int OpType { get; set; }

        [Column(TypeName = "text")]
        public string Remark { get; set; }

        [StringLength(255)]
        public string Url { get; set; }

        public DateTime? CreateDate { get; set; }

        [StringLength(255)]
        public string UserName { get; set; }
        public int UserID { get; set; }

        [StringLength(50)]
        public string IP { get; set; }
    }
}
