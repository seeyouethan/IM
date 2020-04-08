namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserRole")]
    public partial class UserRole
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(255)]
        public string Code { get; set; }

        [StringLength(255)]
        public string IsAdmin { get; set; }

        public int? States { get; set; }

        public DateTime? StatesDate { get; set; }
    }
}
