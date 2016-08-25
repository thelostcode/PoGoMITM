using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGoMITM.Base.Models;
using PoGoMITM.Launcher.ViewModels;

namespace PoGoMITM.Launcher.Models
{
    public static class RawContextListModel
    {
        public static RawContextListItemViewModel FromRawContext(RawContext context)
        {
            var model=new RawContextListItemViewModel();
            model.Guid = context.Guid.ToString();
            model.RequestTime = context.RequestTime;
            model.RequestUri = context.RequestUri.AbsoluteUri;
            return model;
        }
    }
}
