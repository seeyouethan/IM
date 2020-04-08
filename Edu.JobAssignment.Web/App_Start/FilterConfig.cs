using System.Web;
using System.Web.Mvc;

namespace Edu.JobAssignment.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new OKMS.Foundation.Log.MVCExtension.ActionLogFilterAttribute());
        }
    }
}
