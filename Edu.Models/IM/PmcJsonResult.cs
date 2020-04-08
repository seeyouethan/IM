using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edu.Models.IM
{
    public class PmcJsonResult
    {
        public bool Success { get; set; }
        public List<string> Content { get; set; }
        public int Count { get; set; }
        public int Total { get; set; }
        public string Message { get; set; }
        public object MessageList { get; set; }
        public object Ext { get; set; }
    }

    public class PmcGetGroupInfoJsonResult
    {
        public bool Success { get; set; }
        public GroupInfo Content { get; set; }
        public object Count { get; set; }
        public object Total { get; set; }
        public string Message { get; set; }
        public object MessageList { get; set; }
        public object Ext { get; set; }
        public object Error { get; set; }
    }

    public class GroupInfo
    {
        public string Name { get; set; }
    }



    public class PmcGetGroupInfoJsonResult2
    {
        public bool Success { get; set; }
        public GroupInfo2 Content { get; set; }
        public object Count { get; set; }
        public object Total { get; set; }
        public string Message { get; set; }
        public object MessageList { get; set; }
        public object Ext { get; set; }
        public object Error { get; set; }
    }



    public class GroupInfo2
    {
        public string ID { get; set; }
        public string CreateUserID { get; set; }

        public string Name { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
    }

    public class Leader
    {
        public string UserID { get; set; }
        public string RealName { get; set; }
    }
}
