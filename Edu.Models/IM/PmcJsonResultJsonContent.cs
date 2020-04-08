using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edu.Models.Models;

namespace Edu.Models.IM
{
    public class PmcJsonResultJsonContent
    {
        public bool Success { get; set; }
        public List<MyGroup> Content { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public string Message { get; set; }
        public object MessageList { get; set; }
        public object Ext { get; set; }
    }

    public class PmcJsonResultCreateGroup
    {
        public bool Success { get; set; }
        public MyGroup Content { get; set; }
        public int? Total { get; set; }
        public int? Count { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public object MessageList { get; set; }
        public object Ext { get; set; }
    }

    public class PmcJsonResultWeb
    {
        public int Code { get; set; }
        public object Data { get; set; }
        public object Error { get; set; }
        public object Ext { get; set; }
        public bool IsMult { get; set; }
        public object Other { get; set; }
    }

    public class PmcJsonResultWebApi
    {
        public bool Success { get; set; }
        public object Content { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public string Message { get; set; }
        public object MessageList { get; set; }
        public object Ext { get; set; }
    }


}
