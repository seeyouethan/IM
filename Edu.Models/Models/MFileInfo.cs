using System;

namespace Edu.Models.Models
{
    /// <summary>
    /// 文件描述类
    /// </summary>
    [Serializable]
    public class MFileInfo
    {
        /// <summary>
        /// 上传名称名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 路径
        /// </summary>
        public string Path { get; set; }
        /// <summary>
        /// 后缀
        /// </summary>
        public string ExtraName { get; set; }
        /// <summary>
        /// 文件大小
        /// </summary>
        public int Size { get; set; }
    }
}
