namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserInfo")]
    public partial class UserInfo
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string uID { get; set; }

        [StringLength(255)]
        public string UserName { get; set; }

        [StringLength(255)]
        public string Pwd { get; set; }

        public int? RoleID { get; set; }

        [StringLength(255)]
        public string TrueName { get; set; }

        [StringLength(10)]
        public string Sign { get; set; }

        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(255)]
        public string Phone { get; set; }
        
        public string Photo { get; set; }

        public DateTime? CreateDate { get; set; }

        [StringLength(255)]
        public string CreateUser { get; set; }

        public int? States { get; set; }

        [StringLength(255)]
        public string IPAdress { get; set; }
        
        public DateTime? StatesDate { get; set; }

        [StringLength(10)]
        public string uSystem { get; set; }

        public string UnitID { get; set; }
    }
}
