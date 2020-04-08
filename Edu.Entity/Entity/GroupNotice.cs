namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GroupNotice")]
    public partial class GroupNotice
    {
        public int id { get; set; }

        public int msgid { get; set; }

        [StringLength(255)]
        public string groupid { get; set; }
        public string userid { get; set; }
        public string createdate { get; set; }
        
    }
}
