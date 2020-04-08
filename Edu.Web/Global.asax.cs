using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Edu.Web.Service;
using Exceptionless;


namespace Edu.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            RegisterExceptionless();
            MassTransitConfig.MassTransit_Start();
        }

        protected void Application_Stop()
        {
            MassTransitConfig.MassTransit_Stop();
        }

        void RegisterExceptionless()
        {
            if (string.Equals(ConfigurationManager.AppSettings["ExceptionlessEnabled"], "true", StringComparison.OrdinalIgnoreCase))
            {
                ExceptionlessClient.Default.Configuration.UseTraceLogger();
                ExceptionlessClient.Default.Configuration.UseReferenceIds();
            }
        }

    }
}
