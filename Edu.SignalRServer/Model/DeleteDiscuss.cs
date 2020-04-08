using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Edu.SignalRServer.Model
{
    public class DeleteDiscuss
    {
        public string CreateTime { get; set; }
        public string CurUserId { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string RealName { get; set; }
        public IList<string> UserIds { get; set; }
    }
}