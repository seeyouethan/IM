using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Owin;

[assembly: OwinStartup(typeof(Edu.SignalRServer.Startup))]

namespace Edu.SignalRServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(9);
            //app.MapSignalR();
            app.Map("/signalr", map =>
            {
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration();
#if DEBUG
                hubConfiguration.EnableDetailedErrors = true;
#endif
                map.RunSignalR(hubConfiguration);
            });
        }
    }
}
