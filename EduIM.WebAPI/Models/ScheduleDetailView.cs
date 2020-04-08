using System.Collections.Generic;

namespace EduIM.WebAPI.Models
{
    public class ScheduleDetailView
    {
        /// <summary>
        /// 工作日程ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 任务描述
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 父任务标题
        /// </summary>
        public string FatherTitle = "无";
        /// <summary>
        /// 创建时间
        /// </summary>
        public string CreateDate { get; set; }
        /// <summary>
        /// 开始时间
        /// </summary>
        public string StartDate { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// 紧急范围 三个值：一般、紧急、特急  分别用0 1 2 表示
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// 完成度百分比 20%
        /// </summary>
        public string Completing { get; set; }
        /// <summary>
        /// 执行者真实姓名
        /// </summary>
        public string DoUserTrueName { get; set; }
        /// <summary>
        /// 执行者ID
        /// </summary>
        public string DoUser { get; set; }
        /// <summary>
        /// 创建者ID
        /// </summary>
        public string Creator { get; set; }
        /// <summary>
        /// 创建者真实姓名
        /// </summary>
        public string CreatorTrueName { get; set; }
        /// <summary>
        /// 创建者头像
        /// </summary>
        public string CreatorPhoto { get; set; }
        /// <summary>
        /// 执行者头像
        /// </summary>
        public string DoUserPhoto { get; set; }
        /// <summary>
        /// 抄送人
        /// </summary>
        public List<ManagerPersonView> ManagerPerson { get; set; }
        /// <summary>
        /// 与当前用户的关系 执行者、发起者、抄送者
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 提醒时间 0表示不提醒，1表示提前5分钟，2表示提前10分钟，3表示提前15分钟，4表示提前30分钟，5表示提前1小时
        /// 这里已经转换为字符串了
        /// </summary>
        public string CallDate { get; set; }

        /// <summary>
        /// 工作日志，工作日志中还包含了工作日志中提交的文件
        /// </summary>
        public List<ScheduleLogView> PlanLogs = new List<ScheduleLogView>();
    }
}