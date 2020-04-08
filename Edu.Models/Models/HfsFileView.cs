namespace Edu.Models.Models
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
        ///类型 例如：mp4  xml doc等等
        /// </summary> 
        public string FileExtension { get; set; }

        /// <summary>
        /// 下载地址
        /// </summary>
        public string DownUrl { get; set; }
    }
}