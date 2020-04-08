using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EduIM.WebAPI.Models
{
    public class UploadFileResult
    {
        public int convertID { get; set; }
        public string fileCode { get; set; }
        public string fileUrl { get; set; }
    }
}