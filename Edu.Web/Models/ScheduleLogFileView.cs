using System;
using System.Collections.Generic;
using Edu.Entity;

namespace Edu.Web.Models
{
    /// <summary>
    /// 用于界面前台显示的工作计划中包含的日志中的上传的文件
    /// </summary>
    public class ScheduleLogFileView
    {
        /// <summary>
        /// Guid
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
        /// 文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 文件后缀名 比如 doc\docx\ppt
        /// </summary>
        public string FileExtension { get; set; }
        /// <summary>
        /// 文件下载地址
        /// </summary>
        public string FileUrl { get; set; }
        /// <summary>
        /// 新的文件名，留做备用(这个是上传到hfs后生产的新的文件名)
        /// </summary>
        public string NewFileName { get; set; }

    }
}
