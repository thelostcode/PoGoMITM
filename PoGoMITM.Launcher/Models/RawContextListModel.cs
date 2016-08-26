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
    public class RawContextListModel
    {
        private static RawContextListModel _instance;

        public static RawContextListItemViewModel FromRawContext(RawContext context)
        {
            var model=new RawContextListItemViewModel();
            model.Guid = context.Guid.ToString();
            model.RequestTime = context.RequestTime;
            model.RequestUri = context.RequestUri.Host;
            return model;
        }

        public List<RawContextListItemViewModel> ContextList
        {
            get { return ContextCache.RawContexts.Values.OrderBy(c => c.RequestTime).Select(FromRawContext).ToList(); }
        }

        public string ContextListAsJson => JsonConvert.SerializeObject(ContextList);

        public static RawContextListModel Instance => _instance ?? (_instance = new RawContextListModel());
    }
}
