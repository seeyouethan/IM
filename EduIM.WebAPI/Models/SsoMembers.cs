namespace EduIM.WebAPI.Models
{
    public class SsoMembers
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string realName { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string userId { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string pId { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string logo { get; set; }


        public string department { get; set; }
        public int type { get; set; }

    }
}