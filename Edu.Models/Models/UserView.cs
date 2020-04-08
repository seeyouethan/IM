namespace Edu.Models.Models
{
    public class UserView
    {

        public string Id { get; set; }
        public string ParentId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 该用户是否是我的好友true表示是，false表示否
        /// </summary>
        public bool IsFriend { get; set; }
        /// <summary>
        /// 用户部门 2019年8月12日 新增
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 用户电话
        /// </summary>
        public string Phone { get; set; }

    }

    /// <summary>
    /// 新的用户Model包含了层级关系，移除了头像和isfriend字段
    /// </summary>
    public class UserViewNew
    {

        public string Id { get; set; }
        public string ParentId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserId { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string RealName { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户部门
        /// </summary>
        public string Department { get; set; }
        /// <summary>
        /// 用户电话
        /// </summary>
        public string Phone { get; set; }

    }



    public class UserViewGroupMember
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 真实姓名
        /// </summary>
        public string realName { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public string icon { get; set; }

        public bool isAdmin { get; set; }
        

    }
}