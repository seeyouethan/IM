namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ConferenceDiscuss")]
    public partial class ConferenceDiscuss
    {
        public int id { get; set; }
        public string groupid { get; set; }
        public string discussid { get; set; }
        public string creator { get; set; }
        public DateTime? createdate { get; set; }
        public string remark { get; set; }
    }
}
