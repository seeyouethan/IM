namespace Edu.Models.Models
{
    /// <summary>
    /// 结果类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Knetoauthresult<T>
    {
        /// <summary>
        /// 结果/结果集
        /// </summary>
        public T Content { get; set; }

        /// <summary>
        /// 提示信息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 创建一个结果类型
        /// </summary>
        /// <param name="success"></param>
        /// <param name="content"></param>
        /// <param name="message"></param>
        public Knetoauthresult(bool success, T content, string message = "")
        {
            Success = success;
            Content = content;
            Message = message;
        }

        public Knetoauthresult(T content)
        {
            Success = true;
            Content = content;
            Message = "";
        }

        public Knetoauthresult() { }
    }
}