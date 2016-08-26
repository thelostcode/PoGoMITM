using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.Owin;
using PoGoMITM.Base.Cache;
using PoGoMITM.Base.Models;
using PoGoMITM.Launcher.Models;

namespace PoGoMITM.Launcher.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = x => View["Index", RawContextListModel.Instance];
            Get["/details/{guid}", true] = async (x, ct) =>
             {
                 var parsedGuid = Guid.Parse(x.guid);
                 RawContext rawContext;
                 if (ContextCache.RawContexts.TryGetValue(parsedGuid, out rawContext))
                 {
                     return Response.AsJson(await RequestContext.GetInstance(rawContext));
                 }
                 return Response.AsJson(default(RawContext));
             };
        }
    }
}
