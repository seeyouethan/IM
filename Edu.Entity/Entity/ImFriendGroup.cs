namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImFriendGroup")]
    public partial class ImFriendGroup
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string TouID { get; set; }

        [StringLength(255)]
        public string CreateUser { get; set; }

        public DateTime? CreateDate { get; set; }

        public double? OrderNo { get; set; }

        public int? IsSystem { get; set; }
    }
}
