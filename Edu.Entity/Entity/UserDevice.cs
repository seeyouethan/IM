namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserDevice")]
    public partial class UserDevice
    {
        public int id { get; set; }
        public string uid { get; set; }
        public string deviceid { get; set; }
        /// <summary>
        /// 这个值要作为服务端推送的设备值使用，每个设备，每次安装后都有一个唯一的值，同样设备卸载再次安装，此值会变化。
        /// </summary>
        public string devicetoken { get; set; }
        public DateTime? createdate { get; set; }
        /// <summary>
        /// 推送的消息个数
        /// </summary>
        public int msgcount { get; set; }
        public string devicetype { get; set; }
        /// <summary>
        /// 当前用户是否在线
        /// </summary>
        public int isonline { get; set; }
    }
}
