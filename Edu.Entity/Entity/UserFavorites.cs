namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// 
    /// </summary>
    [Table("UserFavorites")]
    public partial class UserFavorites
    {
        /// <summary>
        /// ����
        /// </summary>
        public int id { get; set; }

        /// <summary>
        /// �ղ���ID
        /// </summary>
        public string uid { get; set; }
        
        /// <summary>
        /// �����ղص�ʱ��
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        public string content { get; set; }

        /// <summary>
        /// ���ݹؼ�id
        /// </summary>
        public string cid { get; set; }

        /// <summary>
        /// ���� 1��ʾ������Ϣ
        /// </summary>
        public int type { get; set; }
        
        /// <summary>
        /// ��ע�������ֶ�
        /// </summary>
        public string remark { get; set; }

    }
}
