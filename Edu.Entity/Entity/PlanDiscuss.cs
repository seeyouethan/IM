namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("PlanDiscuss")]
    public partial class PlanDiscuss
    {
        public int ID { get; set; }
        public string Guid { get; set; }
        public string Creator { get; set; }
        public DateTime? CreateTime { get; set; }

        public string DiscussID { get; set; }
        public string DiscussTitle { get; set; }
        public string PlanID { get; set; }
        public string Url { get; set; }
        public string Type { get; set; }

        public int IsDel { get; set; }


    }
}
