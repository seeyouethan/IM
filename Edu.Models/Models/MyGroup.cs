using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models
{
    /// <summary>
    /// 我的群组，包含了工作群和聊天群
    /// </summary>
    public class MyGroup
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
        /// 群组描述
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// 群组创建者uid
        /// </summary>
        public string CreateUserID { get; set; }
        /// <summary>
        /// 群组创建时间(yyyy/MM/dd HH:mm:ss)
        /// </summary>
        public string PostTime { get; set; }
        /// <summary>
        /// 群组头像照片
        /// </summary>
        public string Logo { get; set; }
        /// <summary>
        /// 群组类型：0表示工作群，1表示自建群
        /// </summary>
        public int GroupType { get; set; }
        /// <summary>
        /// 是否是管理员
        /// </summary>
        public bool CanManage { get; set; }

        public string UnitID { get; set; }
    }
}
