using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Edu.Service
{
    public static class BaseUrl
    {
        /// <summary>
        /// 获取当前的虚拟目录
        /// </summary>
        public static string Url
        {
            get
            {
               return HttpContext.Current.Request.ApplicationPath.TrimEnd('/');
            }
        }
    }
}
