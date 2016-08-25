using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;
namespace PoGoMITM.Launcher
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //#if DEBUG
            app.UseErrorPage();
            //#endif
            //app.MapSignalR(new HubConfiguration { Resolver = GlobalHost.DependencyResolver });
            app.MapSignalR();
            app.UseNancy();
            //app.UseStageMarker(PipelineStage.MapHandler);
        }
    }
}
