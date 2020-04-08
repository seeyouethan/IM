using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using EduIM.WebAPI.Filters;
using EduIM.WebAPI.Service;

namespace EduIM.WebAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.config"));
            GlobalConfiguration.Configuration.Filters.Add(new WebApiTrackerAttribute());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
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
