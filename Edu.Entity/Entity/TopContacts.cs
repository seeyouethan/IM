namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TopContacts
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string uID { get; set; }

        [StringLength(255)]
        public string ContactID { get; set; }

        public DateTime? ContactDate { get; set; }

        public int? GpID { get; set; }
    }
}
