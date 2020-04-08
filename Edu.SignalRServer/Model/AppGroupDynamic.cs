using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.SignalRServer.Model
{
    /// <summary>
    /// 群组动态
    /// </summary>
    public class AppGroupDynamic
    {
        public string Content { get; set; }
        public string Creator { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string ID { get; set; }
        public string PostTime { get; set; }
        public List<string> Receiver { get; set; }
        public int Type { get; set; }
        public string CreatorId { get; set; }
    }

    /// <summary>
    /// AppGroupDynamic中Content字段包含的内容
    /// </summary>
    public class AppGroupDynamicContent
    {
        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public string Action { get; set; }
        public string Title { get; set; }
        public string Logo { get; set; }
    }



    /// <summary>
    /// 群组公告
    /// </summary>
    public class AppGroupNotice
    {

        public DateTime CreateTime { get; set; }
        public string Creator { get; set; }
        public string CreatorId { get; set; }
        public string GroupID { get; set; }
        public string ID { get; set; }
        public List<string> Receiver { get; set; }
        public string Title { get; set; }
    }

    /// <summary>
    /// 待办任务（吴雪飞负责发送）
    /// </summary>
    public class TodoTask
    {

        public string Name { get; set; }
        public TaskOperatinType OperationType { get; set; }
        public DateTime PostTime { get; set; }
        public string Submitter { get; set; }
        public string TaskID { get; set; }
        public TaskType TaskType { get; set; }
        public string ToUserId { get; set; }
    }

    public enum TaskOperatinType
    {
        Add = 1,
        Delete = 2,
        Update = 3
    }
    public enum TaskType
    {
        Workflow = 1,
        Discuss = 2,
        Creation = 3
    }
    /// <summary>
    /// 13 自建群创建通知（刘静杰）
    /// </summary>
    public class SelfGroupNotice
    {
        public string createtime { get; set; }
        public string creator { get; set; }
        public string describe { get; set; }
        public string groupid { get; set; }
        public string groupname { get; set; }
        public List<string> membersList { get; set; }
        public string photo { get; set; }
    }
    /// <summary>
    /// 14  -->>   新建直播的通知（申志云）
    /// </summary>
    public class LiveAPPInfo
    {      

        public string CourseID { get; set; }
        public string CreateDate { get; set; }
        public string Creator { get; set; }
        public string Des { get; set; }
        public string DiscussTotal { get; set; }
        public string EndDate { get; set; }
        public string Host { get; set; }
        public int ID { get; set; }
        public string ImagePath { get; set; }
        public int? IsPublic { get; set; }
        public int JType { get; set; }
        public string LengthTime { get; set; }
        public virtual ICollection<string> ListUserID { get; set; }
        public string Livepath { get; set; }
        public string LiveUserpath { get; set; }
        public string Name { get; set; }
        public string RecordTotal { get; set; }
        public string StartDate { get; set; }
        public int Status { get; set; }
        public string Steam { get; set; }
        public string UpdateTime { get; set; }
        public string UserID { get; set; }
        public string UserTotal { get; set; }
    }

    /// <summary>
    /// 15  -->>   自建群编辑通知（刘静杰）
    /// </summary>
    public class SelfGroupNoticeMore
    {

        public string groupid { get; set; }
        public string groupname { get; set; }
        public string membername { get; set; }
        public List<string> membersList { get; set; }
        public int type { get; set; }
    }

    /// <summary>
    ///  16  -->>   公司OA平台发出的消息（杨立旭）
    /// </summary>
    public class MessageModel
    {

        public string Content { get; set; }
        public string DetailUrl { get; set; }
        public string ID { get; set; }
        public string SchemeName { get; set; }
        public object SchemeTypeName { get; set; }
        public int StatusCode { get; set; }
        public string TaskDate { get; set; }
        public string ThreadDate { get; set; }
        public List<string> ToUids { get; set; }
        public string Type { get; set; }
    }
}