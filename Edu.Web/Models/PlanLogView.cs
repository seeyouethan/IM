using System.Collections.Generic;
using Edu.Entity;

namespace Edu.Web.Models
{
    /// <summary>
    /// 用于界面前台显示的日志
    /// </summary>
    public class PlanLogView
    {
        /// <summary>
        /// 数据库中的PlanProgress
        /// </summary>
        public PlanProgress Planlog { get; set; }
        /// <summary>
        /// 日志中包含的文件
        /// </summary>
        public List<PlanFile> PlanFiles=new List<PlanFile>();
        public List<HfsFileView> PlanHfsFiles = new List<HfsFileView>();


    }
}
