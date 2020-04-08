namespace Edu.Models.Models
{
    public class ImGroupView
    {
        /// <summary>
        /// Ⱥ��ID
        /// </summary>
        public string ID { get; set; }
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
        /// Ⱥ�鴴��ʱ��(yyyy/MM/dd HH:mm:ss)
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// ��ǰδ����Ϣ����
        /// </summary>
        public int UnreadMsgCount { get; set; }
        /// <summary>
        /// �Ƿ��ǹ���Ա������ǹ���Ա���б༭��Ȩ��
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}