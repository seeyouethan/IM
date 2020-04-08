using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.IM
{
    public class ContactFile
    {
        /// <summary>
        /// 文件id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// 文件全名 包括后缀名
        /// </summary>
        public string filename { get; set; }
        /// <summary>
        /// 文件下载链接
        /// </summary>
        public string fileurl { get; set; }
        /// <summary>
        /// 文件大小 Byte单位
        /// </summary>
        public int filesize { get; set; }
        /// <summary>
        /// 发送者的id
        /// </summary>
        public string fromuid { get; set; }
        /// <summary>
        /// 接收者的id
        /// </summary>
        public string touid { get; set; }
        /// <summary>
        /// 发送者的真实姓名
        /// </summary>
        public string fromrealname { get; set; }
        /// <summary>
        /// 如果是群组中的文件，那么该字段表示来源/接收群组的群组名称，否则表示为空字符串
        /// </summary>
        public string groupname { get; set; }
        /// <summary>
        /// 接收者的真实姓名
        /// </summary>
        public string torealname { get; set; }
        /// <summary>
        /// 发送文件的时间，字符串格式，24小时格式，精确到秒 yyyy/mm/dd HH:mm:ss
        /// </summary>
        public string createtime { get; set; }
    }
}
