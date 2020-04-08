using Edu.Entity;
using System;
using System.Collections.Generic;

namespace Edu.Models.Models
{
    /// <summary>
    /// 用于界面前台显示的日志 FullCalendar中的
    /// </summary>
    public class PlanFullCalendarView
    {
        public string title { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string id { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int order { get; set; }
    }
}
