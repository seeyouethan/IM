namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImApply")]
    public partial class ImApply
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string uID { get; set; }

        [StringLength(255)]
        public string TouID { get; set; }

        [StringLength(255)]
        public string Msg { get; set; }

        [StringLength(255)]
        public string CreateUser { get; set; }

        public DateTime? CreateDate { get; set; }

        public int? Result { get; set; }

        [StringLength(255)]
        public string ResultMsg { get; set; }

        public DateTime? ResultDate { get; set; }
    }
}
