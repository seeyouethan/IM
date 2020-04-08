namespace Edu.Models.Models
{
    public class ImGroupMemberView
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 在群组中的昵称
        /// </summary>
        public string NickName { get; set; }
        
        /// <summary>
        /// 是否是管理员 true表示是 false表示否
        /// </summary>
        public bool IsAdmin { get; set; }

        /// <summary>
        /// 加入群组的时间(yyyy/MM/dd HH:mm:ss)
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 该成员所在的部门  2019年8月12日 新增
        /// </summary>
        public string Department { get; set; }
    }
}
