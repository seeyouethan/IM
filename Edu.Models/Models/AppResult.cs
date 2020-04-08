using System;

namespace Edu.Models.Models
{/// <summary>
 /// 请求返回结果模型
 /// </summary>
    public class AppResult
    {
        /// <summary>
        /// 状态码 OK:200 InternalServerError:500
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 响应的数据
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; }

        public int Count { get; set; }

        public int Total { get; set; }
        

        public AppResult()
        {

        }

        public AppResult(bool success, string content, string msg, int total,  int count)
        {
            Success = success;
            Content = content;
            Message = msg;
            Count = count;
            Total = total;
        }
    }
}
