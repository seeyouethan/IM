namespace Edu.Models.Models
{
    public class ImGroupView
    {
        /// <summary>
        /// 群组ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 群组名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 群组头像照片
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 群组描述
        /// </summary>
        public string Des { get; set; }
        /// <summary>
        /// 群组创建者uid
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 群组创建时间(yyyy/MM/dd HH:mm:ss)
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// 当前未读消息个数
        /// </summary>
        public int UnreadMsgCount { get; set; }
        /// <summary>
        /// 是否是管理员，如果是管理员则有编辑的权限
        /// </summary>
        public bool IsAdmin { get; set; }
    }
}