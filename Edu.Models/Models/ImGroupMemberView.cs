namespace Edu.Models.Models
{
    public class ImGroupMemberView
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
        /// ����Ⱥ���ʱ��(yyyy/MM/dd HH:mm:ss)
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// ͷ��
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// �ó�Ա���ڵĲ���  2019��8��12�� ����
        /// </summary>
        public string Department { get; set; }
    }
}
