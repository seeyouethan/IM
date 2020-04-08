using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.Web.Models
{
    public class HfsFileView
    {
        /// <summary>
        /// ID
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// LiteratureId
        /// </summary>
        public string LiteratureId { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// TableName
        /// </summary>
        public string TableName = "local";

        /// <summary>
        ///机构ID
        /// </summary>
        public string DBCode = "local";
        /// <summary>
        ///类型
        /// </summary>
        public string FileExtension { get; set; }
    }
}