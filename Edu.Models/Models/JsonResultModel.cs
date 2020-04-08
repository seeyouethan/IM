using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.Models
{
    public class JsonResultModel
    {
        public JsonResultType code { get; set; }
        public object data { get; set; }
        public string msg { get; set; }
    }
    /// <summary>
    /// 成功失败
    /// </summary>
    public enum JsonResultType
    {
        Success = 0,
        Failed = 1
    }


    public class JsonResultCustomer
    {
        public bool Success { get; set; }
        public object Content { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
    }
}
