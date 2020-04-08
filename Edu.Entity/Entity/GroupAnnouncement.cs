namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GroupAnnouncement")]
    public partial class GroupAnnouncement
    {
        public int id { get; set; }

        public int groupid { get; set; }

        [StringLength(255)]
        public string creator { get; set; }
        public string title { get; set; }
        public string creatorname { get; set; }

        public DateTime? datetime { get; set; }

        public string content { get; set; }
        [StringLength(255)]
        public string remark { get; set; }
    }
}
