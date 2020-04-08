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
        /// ���ֵҪ��Ϊ��������͵��豸ֵʹ�ã�ÿ���豸��ÿ�ΰ�װ����һ��Ψһ��ֵ��ͬ���豸ж���ٴΰ�װ����ֵ��仯��
        /// </summary>
        public string devicetoken { get; set; }
        public DateTime? createdate { get; set; }
        /// <summary>
        /// ���͵���Ϣ����
        /// </summary>
        public int msgcount { get; set; }
        public string devicetype { get; set; }
        /// <summary>
        /// ��ǰ�û��Ƿ�����
        /// </summary>
        public int isonline { get; set; }
    }
}
