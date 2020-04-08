using System;

namespace Edu.Models.Models
{
    public class UserOnLine
    {
        public string uid { get; set; }
        public string ConnectionId { get; set; }
        public string CreateDate { get; set; }
        /// <summary>
        /// 移动端使用，应用程序的唯一ID，手机UDID+APP的BundleID，用户发送远程通知
        /// </summary>
        public string deviceToken { get; set; }
        /// <summary>
        /// 移动端使用，移动端设备的ID
        /// </summary>
        public string deviceid { get; set; }
        /// <summary>
        /// 终端 移动端有ios/android  Web端有oaokcs/oa
        /// </summary>
        public string devicetype { get; set; }

    }
}
