namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("ImGroup")]
    public partial class ImGroup
    {
        /// <summary>
        /// Ⱥ��ID
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Ⱥ������
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ⱥ��ͷ����Ƭ
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// Ⱥ������
        /// </summary>
        public string Des { get; set; }
        /// <summary>
        /// Ⱥ�鴴����uid
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// Ⱥ�鴴��ʱ��
        /// </summary>
        public DateTime? CreateDate { get; set; }
        /// <summary>
        /// TypeID=0��ʾ����Ⱥ�� TypeID=1��ʾ�Լ�����������Ⱥ��
        /// </summary>
        public int? TypeID { get; set; }
    }
}