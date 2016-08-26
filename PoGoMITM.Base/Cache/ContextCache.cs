using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoGoMITM.Base.Models;

namespace PoGoMITM.Base.Cache
{
    public static class ContextCache
    {
        public static ConcurrentDictionary<Guid, RawContext> RawContexts = new ConcurrentDictionary<Guid, RawContext>();
        public static ConcurrentDictionary<Guid, RequestContext> RequestContext = new ConcurrentDictionary<Guid, RequestContext>();

    }
}
