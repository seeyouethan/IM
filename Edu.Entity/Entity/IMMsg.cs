namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("IMMsg")]
    public partial class IMMsg
    {
        public int ID { get; set; }

        [StringLength(255)]
        public string FromuID { get; set; }

        [StringLength(255)]
        public string TouID { get; set; }
        /// <summary>
        /// ��Ϣ���ݣ�����text���ͣ���ֱ�Ӵ���ı�������img ���ŵ�Ϊһ��img��url, ��Ϊfile ���ŵ���file�����ƣ�����abc.docx
        /// </summary>
        [StringLength(2000)]
        public string Msg { get; set; }

        [StringLength(255)]
        public string CreateUser { get; set; }
        /// <summary>
        /// ����ʱ��
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// ����/�ظ��� ��Ϣ��id
        /// </summary>
        public int? QuoteId { get; set; }

        /// <summary>
        /// 2020��4��8��11:07:23 ���� ���þ�������
        /// </summary>
        public string QuoteContent { get; set; }

        /// <summary>
        /// ����������
        /// </summary>
        public string fromusername { get; set; }
        /// <summary>
        /// ����������
        /// </summary>
        public string tousername { get; set; }
        /// <summary>
        /// ��Ϣ���ͣ���0��1��2�ֱ��ʾ�ı���Ϣ��ͼƬ��Ϣ���ļ���Ϣ������type=3��ʾһ�����ϵ���Ϣ,ͬʱ�������ı��ַ�����ͼƬ,����type=4 ��ʾ��������Ϣ
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string FileType { get; set; }
        /// <summary>
        /// �Ƿ���Ⱥ����Ϣ 0��ʾ��1��ʾ��
        /// </summary>
        public int isgroup { get; set; }
        
        /// <summary>
        /// 2018��12��20�� �������ֶΣ���������ļ������ص�ַ  
        /// 2019��9��29�� ���� ��msgtype=1 ��ͼƬ��Ϣ��ʱ�򣬸��ֶδ������ͼ�ĵ�ַ
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// type=4 ��ʱ�򣬴�ŵ�������Ϣ�ĳ���
        /// </summary>
        public int Duration { get; set; }
        /// <summary>
        /// �������Ϊ3�����ʾ��ŵ�ͼƬ�ļ��ϣ��м��÷ֺŸ���
        /// </summary>
        public string ImgList { get; set; }

        /// <summary>
        /// ����ƶ��˵�id
        /// </summary>
        public string id1 { get; set; }
        /// <summary>
        /// �ϴ���HFS�����ɵ�filecode 
        /// </summary>
        public string NewFileName { get; set; }
        /// <summary>
        /// �Ƿ��Ѿ���ɾ��
        /// </summary>
        public int IsDel { get; set; }

        /// <summary>
        /// ����Ϣ�Ƿ���֪ͨ��Ϣ  2019��11��5������
        /// ��ʱ�����ˣ��жϻ�ִ����Ϣ��ʹ��Type=="8" ���ж�
        /// </summary>
        public bool IsNotice { get; set; }
        public string NoticeCreateData { get; set; }
        /// <summary>
        /// 2020��2��3������ ��Ϣ����ID ��Ϊ�����ʾΪ��ͨ������Ϣ
        /// </summary>
        public string SubjectId { get; set; }

        /// <summary>
        /// ������
        /// </summary>
        public int ThumbCount { get; set; }

    }
}