namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Menu")]
    public partial class Menu
    {
        public int ID { get; set; }

        [Required]
        [StringLength(255)]
        public string FuncID { get; set; }

        [Required]
        [StringLength(255)]
        public string ParentID { get; set; }

        [Required]
        [StringLength(255)]
        public string FuncName { get; set; }

        [Required]
        [StringLength(255)]
        public string FuncDes { get; set; }

        [Required]
        [StringLength(255)]
        public string FuncType { get; set; }

        public int FuncLevel { get; set; }

        public int OrderNo { get; set; }

        [Required]
        [StringLength(255)]
        public string Target { get; set; }

        [Required]
        [StringLength(255)]
        public string TargetUrl { get; set; }

        [Required]
        [StringLength(255)]
        public string SysLogo { get; set; }

        [StringLength(255)]
        public string RoleIDs { get; set; }

        public int? States { get; set; }

        public DateTime? StatesDate { get; set; }
    }
}
