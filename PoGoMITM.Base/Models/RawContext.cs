using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Titanium.Web.Proxy.Models;

namespace PoGoMITM.Base.Models
{
    public class RawContext
    {
        [BsonId]
        public Guid Guid { get; set; }
        public DateTime RequestTime { get; set; }
        public Uri RequestUri { get; set; }

        public List<HttpHeader> RequestHeaders { get; set; }
        public byte[] RequestBody { get; set; }

        public List<HttpHeader> ResponseHeaders { get; set; }
        public byte[] ResponseBody { get; set; }
    }
}
