using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGoMITM.Base.Models;

namespace PoGoMITM.Launcher.ViewModels
{
    public class RawContextListItemViewModel
    {
        public string Guid { get; set; }
        public DateTime RequestTime { get; set; }
        public string RequestUri { get; set; }
        public bool IsActive { get; set; }
    }
}
