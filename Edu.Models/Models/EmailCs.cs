namespace Edu.Models.Models
{
    /// <summary>
    /// 邮件服务
    /// </summary>
    public class EmailCs
    {
        public string Email { get; set; }
        public string EmailUser { get; set; }
        public string EmailPassword { get; set; }
        public string EmailSmtp { get; set; }
        /// <summary>
        /// 收件人地址
        /// </summary>
        public string ReceiveEmail { get; set; }
        /// <summary>
        /// 收件人用户名
        /// </summary>
        public string ReceiveName { get; set; }
        /// <summary>
        /// 发送邮件内容
        /// </summary>
        public string SendBody { get; set; }
        /// <summary>
        /// 邮件标题
        /// </summary>
        public string Subject { get; set; }
    }
}
