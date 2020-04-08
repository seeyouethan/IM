namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("TopContactsGroup")]
    public partial class TopContactsGroup
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Creator { get; set; }

        public DateTime? CreateDate { get; set; }

        public DateTime? OrderNo { get; set; }

        [StringLength(255)]
        public string Des { get; set; }
    }
}
