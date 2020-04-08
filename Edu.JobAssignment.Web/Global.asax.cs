using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MassTransitConfig = Edu.JobAssignment.Web.Service.MassTransitConfig;

namespace Edu.JobAssignment.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            MassTransitConfig.MassTransit_Start();
        }
        protected void Application_Stop()
        {
            MassTransitConfig.MassTransit_Stop();
        }
    }
}
