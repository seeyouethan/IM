using System;
using System.ComponentModel.DataAnnotations;

namespace Edu.Web.Models
{
    public partial class ImGroupMemberView
    {
        /// <summary>
        /// �û�ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// ��Ⱥ���е��ǳ�
        /// </summary>
        public string NickName { get; set; }
        
        /// <summary>
        /// �Ƿ��ǹ���Ա true��ʾ�� false��ʾ��
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// ����Ⱥ���ʱ��(yyyy/MM/dd hh:mm:ss)
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// ͷ��
        /// </summary>
        public string Photo { get; set; }
    }
}
