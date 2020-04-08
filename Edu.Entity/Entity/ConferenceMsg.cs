namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ConferenceMsg")]
    public partial class ConferenceMsg
    {
        public int id { get; set; }
        public string conferenceid { get; set; }
        public string uid { get; set; }
        public string truename { get; set; }
        public string photo { get; set; }
        public string msgContent { get; set; }
        public int? msgtype { get; set; }
        public DateTime? datetime { get; set; }
        public string ext { get; set; }
    }
}
