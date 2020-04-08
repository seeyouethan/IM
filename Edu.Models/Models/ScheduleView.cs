using System.Collections.Generic;

namespace Edu.Models.Models
{
    public class ScheduleView
    {
        /// <summary>
        /// 工作日程ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 工作日程Fid
        /// </summary>
        public string Fid { get; set; }
        /// <summary>
        /// 任务级别（一级任务、子任务（2级））
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
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
        /// 紧急范围 三个值：一般、紧急、特急 
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// 完成度百分比 20%
        /// </summary>
        public string Completing { get; set; }

        /// <summary>
        /// 所包含的子任务子集
        /// </summary>
        public List<ScheduleView> SubSchedules = new List<ScheduleView>();
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
        /// 与当前用户的关系 执行者、发起者、抄送者
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 是否可以编辑
        /// </summary>
        public bool CanEdit { get; set; }
        /// <summary>
        /// 是否可以创建工作反馈（日志）
        /// </summary>
        public bool CanCreateWorkLog { get; set; }
        /// <summary>
        /// 是否可以创建子任务
        /// </summary>
        public bool CanCreateSub { get; set; }
        /// <summary>
        /// 是否可以删除
        /// </summary>
        public bool CanDelete { get; set; }
        /// <summary>
        /// 是否可以置顶
        /// </summary>
        public bool CanTop { get; set; }
        /// <summary>
        /// 置顶时间
        /// </summary>
        public string TopDateTime { get; set; }

        public bool IsTop { get; set; }
        /// <summary>
        /// 抄送人
        /// </summary>
        public List<ManagerPersonView> ManagerPerson { get; set; }
        /// <summary>
        /// 详细内容/描述
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 提醒
        /// </summary>
        public string CallDate { get; set; }

        public string Address { get; set; }

    }
}