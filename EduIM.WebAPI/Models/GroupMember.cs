using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EduIM.WebAPI.Models
{
    public class GroupMember
    {

        public string uid { get; set; }
        public string realName { get; set; }
        public bool isAdmin { get; set; }
        public string photo { get; set; }
    }
}