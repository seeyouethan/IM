namespace Edu.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Plan")]
    public partial class Plan
    {
        public int ID { get; set; }

        [StringLength(50)]
        public string Guid { get; set; }

        [StringLength(500)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Creator { get; set; }

        public DateTime? CreateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(500)]
        public string Adress { get; set; }

        [Column(TypeName = "text")]
        public string Content { get; set; }

        public int CallDate { get; set; }
        /// <summary>
        /// ��ɶ� 100��ʾ��� 
        /// </summary>
        public int Completing { get; set; }

        [StringLength(2000)]
        public string CompleteBZ { get; set; }
        /// <summary>
        /// ���ʱ��
        /// </summary>
        public DateTime? CompleteDate { get; set; }

        public string ParentID { get; set; }

        
        public string ManagerPerson { get; set; }

        [StringLength(255)]
        public string ExecutivesPerson { get; set; }

        /// <summary>
        /// Ⱥ��id
        /// </summary>
        [StringLength(50)]
        public string GroupID { get; set; }

        public int? Priority { get; set; }

        public int? isTop { get; set; }

        public DateTime? TopDate { get; set; }
        /// <summary>
        /// ��������ʵ����
        /// </summary>
        public string CreatorTrueName { get; set; }
        /// <summary>
        /// ɾ����־ 0��ʾδɾ�� 1��ʾɾ��
        /// </summary>
        public int isdel { get; set; }
        /// <summary>
        /// �Ƿ��Ѷ�����������Ѷ�����ô���û��´ε�¼��ʱ����Ҫ֪ͨһ��
        /// </summary>
        public int isread { get; set; }

        public string Fid { get; set; }
        public int Level { get; set; }
        /// <summary>
        /// ��ַ��2019��9��24�� ���������ƶ���ʹ��
        /// </summary>
        public string Address { get; set; }
    }
}
