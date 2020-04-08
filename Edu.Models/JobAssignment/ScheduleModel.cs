namespace Edu.Models.JobAssignment
{
    public class ScheduleModel
    {
        
        public int id { get; set; }
        public string title { get; set; }
        public string startDate { get; set; }
        /// <summary>
        /// 0 不提醒
        /// 1 提前5分钟
        /// 2 提前10分钟
        /// 3 提前15分钟
        /// 4 提前30分钟
        /// 5 提前1小时
        /// 6 提前1天
        /// </summary>
        public int callDate { get; set; }
        public string endDate { get; set; }
        public string groupId { get; set; }
        public string content { get; set; }
        public string groupName { get; set; }

        public string exePerson { get; set; }

        public string noticeDate { get; set; }
    }
}