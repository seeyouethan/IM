namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    /// <summary>
    /// ��¼������־�����ݿ�
    /// </summary>
    [Table("GetuiLog")]
    public partial class GetuiLog
    {
        public int id { get; set; }

        public string fromuid { get; set; }

        public string touid { get; set; }

        /// <summary>
        /// ��Ϣʱ��
        /// </summary>
        public string msgtime { get; set; }
        
        /// <summary>
        /// �յ���ִʱ��
        /// </summary>
        public string resulttime { get; set; }

        /// <summary>
        /// ��������
        /// </summary>
        public string content
        {
            get; set;
        }
       
        /// <summary>
        /// ���� 0��ʾ������Ϣ
        /// </summary>
        public int type
        {
            get; set;
        }
        /// <summary>
        /// ������Ϣid
        /// </summary>
        public string msgid
        {
            get; set;
        }
        /// <summary>
        /// �豸����
        /// </summary>
        public string devicetype
        {
            get; set;
        }
        /// <summary>
        /// �豸id
        /// </summary>
        public string deviceid
        {
            get; set;
        }
        /// <summary>
        /// ��ע
        /// </summary>
        public string remark
        {
            get; set;
        }

    }
}
