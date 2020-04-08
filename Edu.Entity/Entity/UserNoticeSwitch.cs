namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserNoticeSwitch")]
    public partial class UserNoticeSwitch
    {
        public int id { get; set; }
        public string uid { get; set; }
        /// <summary>
        /// IM新消息通知 0表示关闭，1表示开启
        /// </summary>
        public int type1 { get; set; }

        /// <summary>
        /// 通知公告消息（群组类）0表示关闭，1表示开启
        /// </summary>
        public int type2 { get; set; }
        /// <summary>
        /// OA待办消息通知 0表示关闭，1表示开启
        /// </summary>
        public int type3 { get; set; }
    }
}
