using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using EduIM.WebAPI.Filters;
using Newtonsoft.Json.Serialization;
using System.Web.Http.Cors;

namespace EduIM.WebAPI
{

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //跨域配置
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            // Web API configuration and services
            var json = config.Formatters.JsonFormatter;
            // 解决json序列化时的循环引用问题
            json.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            
            // 干掉XML序列化器
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            


            config.Filters.Add(new GlobalExceptionAttribute());
            //注册全局行为过滤器
            config.Filters.Add(new GlobalActionFilterAttribute());
            // Web API 配置和服务

            // Web API 路由
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Filters.Add(new OKMS.Foundation.Log.WebApiExtension.ActionLogFilterAttribute());
        }
    }
}
