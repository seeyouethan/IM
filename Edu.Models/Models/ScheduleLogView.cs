using System.Collections.Generic;

namespace Edu.Models.Models
{
    /// <summary>
    /// 用于界面前台显示的工作计划中包含的日志
    /// </summary>
    public class ScheduleLogView
    {
        /// <summary>
        /// 数据库中的PlanProgress的Guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 创建者的Guid
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateDate { get; set; } 
        /// <summary>
        /// 工作日志内容 注意换行符
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 当前完成进度
        /// </summary>
        public string CurProgress { get; set; }
        /// <summary>
        /// 包含的文件
        /// </summary>
        public List<ScheduleLogFileView> LogFiles=new List<ScheduleLogFileView>();

    }
}
