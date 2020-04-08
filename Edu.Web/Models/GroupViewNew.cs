using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    /// <summary>
    /// /pmcwebapi/api/Group/GetGroupList接口返回来的群组数据结构
    /// </summary>
    public class GroupViewNew
    {
        public bool CanExit { get; set; }
        public bool CanManage { get; set; }
        /// <summary>
        /// 创建者ID
        /// </summary>
        public string CreateUserID { get; set; }
        /// <summary>
        /// 群组ID
        /// </summary>
        public string ID { get; set; }
        public object Leader { get; set; }
        public object Members { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string PostTime { get; set; }
        public string Summary { get; set; }
        public string UnitID { get; set; }
        /// <summary>
        /// 群组默认图片
        /// </summary>
        public string Logo { get; set; }
    }
}