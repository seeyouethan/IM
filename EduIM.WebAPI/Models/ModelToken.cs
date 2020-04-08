using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EduIM.WebAPI.Models
{
    public class ModelToken
    {
        public ModelToken() { }
        public String AppID { get; set; }
        public DateTime StartDate { get; set; }
        public String UserName { get; set; }
    }

}