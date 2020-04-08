using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Cors;


[assembly: OwinStartup(typeof(EduIM.ChatServer.Startup))]

namespace EduIM.ChatServer
{
    public class Startup
    {


        //public void Configuration(IAppBuilder app)
        //{

        //    // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
        //    app.MapSignalR();
        //}
        public void Configuration(IAppBuilder app)
        {

            GlobalHost.DependencyResolver.UseRedis(Edu.Tools.ConfigHelper.GetConfigString("RedisServerIP"), Edu.Tools.ConfigHelper.GetConfigInt("RedisServerPort"), string.Empty, "ServerHub");
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);

                var hubConfiguration = new HubConfiguration
                {
                    EnableJSONP = true
                };
                map.RunSignalR(hubConfiguration);
            });
            //app.MapSignalR();
        }
    }
}



 