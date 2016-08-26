using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGoMITM.Base.Cache;
using PoGoMITM.Base.Models;
using PoGoMITM.Launcher.ViewModels;

namespace PoGoMITM.Launcher.Models
{
    public class RequestContextListModel
    {
        private static RequestContextListModel _instance;

        public static RequestContextListItemViewModel FromRawContext(RawContext context)
        {
            RequestContext requestContext = null;
            Task.Run(async () => { requestContext = await RequestContext.GetInstance(context); }).Wait();
            var model = new RequestContextListItemViewModel();
            if (requestContext != null)
            {
                model.Guid = requestContext.Guid.ToString();
                model.RequestTime = requestContext.RequestTime;
                model.Host = requestContext.RequestUri.Host;
            }
            return model;
        }

        public List<RequestContextListItemViewModel> ContextList
        {
            get { return ContextCache.RawContexts.Values.OrderBy(c => c.RequestTime).Select(FromRawContext).ToList(); }
        }

        public string ContextListAsJson => JsonConvert.SerializeObject(ContextList);

        public static RequestContextListModel Instance => _instance ?? (_instance = new RequestContextListModel());
    }
}
