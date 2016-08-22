using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Http;

namespace PoGo_Proxy
{
    public class PoGoWebSession
    {
        public string Uri { get; set; }
        public Guid Guid { get; set; }
        public Request RawRequest { get; set; }
        public byte[] RequestBody { get; set; }
        public Response RawResponse { get; set; }
        public byte[] ResponseBody { get; set; }
        public ulong RequestId { get; set; }
        public MessageBlock RequestBlock { get; set; }
        public MessageBlock ResponseBlock { get; set; }
    }
}
