using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    /// <summary>
    /// 申请入群结果通知
    /// </summary>
    public class GroupApplyResult
    {
        /// <summary>
        /// 申请者真实姓名
        /// </summary>
        public string TrueName { get; set; }
        /// <summary>
        /// 申请结果
        /// </summary>
        public string ApplyResult { get; set; }
        /// <summary>
        /// 申请群组的名称
        /// </summary>
        public string GroupName { get; set; }
        /// <summary>
        /// 通知日期
        /// </summary>
        public string PostDate { get; set; }
        /// <summary>
        /// 附带信息（暂时没用到）
        /// </summary>
        public string Msg { get; set; }
    }
}