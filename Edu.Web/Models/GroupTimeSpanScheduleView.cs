using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    public class GroupTimeSpanScheduleView
    {
        /// <summary>
        /// 日期（字符串类型 yyyy-MM-dd）
        /// </summary>
        public string datetime { get; set; }
        /// <summary>
        /// 是否有日程安排 true表示有 false表示无
        /// </summary>
        public bool haswork { get; set; }
        

    }
}