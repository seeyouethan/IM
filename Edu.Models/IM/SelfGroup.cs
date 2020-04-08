using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Models.Models;
using Edu.Models.Models.Msg;

namespace Edu.Models.IM
{
    /// <summary>
    /// 自建群群组信息
    /// </summary>
    public class SelfGroup
    {
        /// <summary>
        /// 群组id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 群组名称
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string des { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string createtime { get; set; }
        /// <summary>
        /// 群组头像
        /// </summary>
        public string photo { get; set; }
        /// <summary>
        /// 创建者id
        /// </summary>
        public string creatorid { get; set; }
        /// <summary>
        /// 创建者真实姓名
        /// </summary>
        public string creatorrealname { get; set; }
        /// <summary>
        /// 群组成员id集合
        /// </summary>
        public List<UserView> members { get; set; }
    }
}
