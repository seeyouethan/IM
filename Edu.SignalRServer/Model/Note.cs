using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.SignalRServer.Model
{
    public class Note
    {
        public string Content { get; set; }
        public string CreateTime { get; set; }
        public string Extension { get; set; }
        public string Id { get; set; }
        public List<string> MembersList { get; set; }
        public string ParentRealName { get; set; }
        public string PId { get; set; }
        public string RealName { get; set; }
        public string SourceId { get; set; }
        public string Title { get; set; }
        public int SourceType { get; set; }
    }
}