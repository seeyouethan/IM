using System;
using System.Collections.Generic;

namespace Edu.Models.Models
{
    /// <summary>
    /// 用于界面前台显示的工作计划
    /// </summary>
    public class PlanView
    {
        /// <summary>
        /// 自增id
        /// </summary>
        public int id { get; set; }
        /// <summary>
        /// Guid
        /// </summary>
        public string uid { get; set; }
        public string title { get; set; }
        /// <summary>
        /// 创建者真实姓名
        /// </summary>
        public string truename0 { get; set; }
        /// <summary>
        /// 执行者真实姓名
        /// </summary>
        public string truename1 { get; set; }
        /// <summary>
        /// 时间跨度范围 例如：07/12-07/18
        /// </summary>
        public string timespan { get; set; }

        /// <summary>
        /// 紧急范围 三个值：""、紧急、特急  分别用0 1 2 表示
        /// </summary>
        public string priority { get; set; }

        /// <summary>
        /// 完成度百分比 20%
        /// </summary>
        public string completing { get; set; }

        /// <summary>
        /// 所包含的子任务子集
        /// </summary>
        public List<PlanView> subworks =new List<PlanView>();

        public string relationship = "";
        public int level { get; set; }
        public string fid { get; set; }

    }
}
