namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImGroupDetail")]
    public partial class ImGroupDetail
    {
        public int ID { get; set; }
        
        public string GroupID { get; set; }
        
        public string UserID { get; set; }
        
        public string NickName { get; set; }
        
        /// <summary>
        /// 是否是管理员 1表示是 0表示否
        /// </summary>
        public int isAdmin { get; set; }

        [StringLength(255)]
        public string Creator { get; set; }

        public DateTime? CreateDate { get; set; }

        public int? Result { get; set; }

        [StringLength(255)]
        public string ResultMsg { get; set; }

        public DateTime? ResultDate { get; set; }

        public string photo { get; set; }
    }
}
