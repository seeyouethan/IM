namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("GroupSubject")]
    public partial class GroupSubject
    {
        public int id { get; set; }
        public string groupid { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// ������ID
        /// </summary>
        public string creator { get; set; }
        /// <summary>
        /// ɾ�����
        /// </summary>
        public int isdel { get; set; }
        /// <summary>
        /// ������ǣ��Ѿ����ã�
        /// </summary>
        public int isend { get; set; }
        /// <summary>
        /// ����ʱ�䣨��Ϊɾ��ʱ��ʹ�ã�
        /// </summary>
        public string endtime { get; set; }
        /// <summary>
        /// ����(����)
        /// </summary>
        public string remark { get; set; }
    }
}
