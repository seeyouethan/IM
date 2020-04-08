using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    /// <summary>
    /// 抄送人类
    /// </summary>
    public class ManagerPersonView
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
        /// 用户头像
        /// </summary>
        public string Photo { get; set; }
    }
}