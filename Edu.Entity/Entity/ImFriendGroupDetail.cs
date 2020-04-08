namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImFriendGroupDetail")]
    public partial class ImFriendGroupDetail
    {
        public int ID { get; set; }

        public int? GroupID { get; set; }

        [StringLength(255)]
        public string UserID { get; set; }

        [StringLength(255)]
        public string FriendID { get; set; }

        [StringLength(255)]
        public string Creator { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
