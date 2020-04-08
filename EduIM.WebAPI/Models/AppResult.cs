using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace EduIM.WebAPI.Models
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
        public object Content { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

        public object Message { get; set; }

        public object Count { get; set; }

        public object Total { get; set; }

        public object Ext { get; set; }

        public AppResult()
        {

        }

        public AppResult(bool success, Object data, object total, string msg, object count)
        {
            Count = count;
            Success = success;
            Content = data;
            Total = total;
            Message = msg;
        }
    }
}
