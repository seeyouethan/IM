using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models
{
    public class HandlerResult
    {
        /// <summary>
        /// 状态码 OK:200 InternalServerError:500
        /// </summary>
        public HttpStatusCode Code { get; set; }

        /// <summary>
        /// 响应的数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; set; }

        public object Other { get; set; }

        public bool IsMult { get; set; }

        public object Ext { get; set; }

        public HandlerResult()
        {

        }

        public HandlerResult(HttpStatusCode code, Object data)
        {
            Code = code;
            Data = data;
        }

        public HandlerResult(HttpStatusCode code, Object data, string error)
        {
            Code = code;
            Data = data;
            Error = error;
        }

        public HandlerResult(HttpStatusCode code, Object data, object other)
        {
            Code = code;
            Data = data;
            Other = other;
        }

        public HandlerResult(HttpStatusCode code, Object data, object other, string error)
        {
            Code = code;
            Data = data;
            Other = other;
            Error = error;
        }
    }
}
