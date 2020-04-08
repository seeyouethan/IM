namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GroupMsg")]
    public partial class GroupMsg
    {
        public int ID { get; set; }
        public string Guid { get; set; }
        public string Content { get; set; }
        public string MsgID { get; set; }
        public string JumpId { get; set; }
        public string Type { get; set; }
        public string GroupId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? PostTime { get; set; }
        public string Creator { get; set; }
        public int? IsRead { get; set; }
    }
}
