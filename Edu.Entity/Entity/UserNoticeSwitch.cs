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
        /// IM����Ϣ֪ͨ 0��ʾ�رգ�1��ʾ����
        /// </summary>
        public int type1 { get; set; }

        /// <summary>
        /// ֪ͨ������Ϣ��Ⱥ���ࣩ0��ʾ�رգ�1��ʾ����
        /// </summary>
        public int type2 { get; set; }
        /// <summary>
        /// OA������Ϣ֪ͨ 0��ʾ�رգ�1��ʾ����
        /// </summary>
        public int type3 { get; set; }
    }
}
