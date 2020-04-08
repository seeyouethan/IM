using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    public class GroupView
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
        /// 简介
        /// </summary>
        public string Summary { get; set; }
        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime PostTime { get; set; }
        /// <summary>
        ///机构ID
        /// </summary>
        public string UnitID { get; set; }
        /// <summary>
        /// 负责人
        /// </summary>
        public Object Leader { get; set; }
        /// <summary>
        /// 成员
        /// </summary>
        public List<Object> Members { get; set; }
        //public UserView[] Members;
        public int MembersCount { get; set; }
        /// <summary>
        /// 是否能够退出
        /// </summary>
        public bool CanExit { get; set; }
        /// <summary>
        /// 是否能够管理群组
        /// </summary>
        public bool CanManage { get; set; }
        /// <summary>
        /// 文档数
        /// </summary>
        public int DocumentCount { get; set; }
        /// <summary>
        /// 研讨数
        /// </summary>
        public int DiscussCount { get; set; }
        /// <summary>
        /// 创作数
        /// </summary>
        public int CreationCount { get; set; }
        /// <summary>
        /// 项目数
        /// </summary>
        public int ProjectCount { get; set; }
    }



}