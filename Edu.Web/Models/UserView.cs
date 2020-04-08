using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    public class UserView
    {
        /// <summary>
        /// 该用户的ID
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
        /// 用户头像
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// 该用户是否是我的好友true表示是，false表示否
        /// </summary>
        public bool IsFriend { get; set; }

    }
}