using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using PoGo_Proxy.MongoDB;
using POGOProtos.Networking.Envelopes;
using Titanium.Web.Proxy.Models;

namespace PoGo_Proxy.Models
{
    public class PoGoWebRequest
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Guid { get; set; }
        public Uri Uri { get; set; }
        public DateTime RequestTime { get; set; }
        public List<HttpHeader> RequestHeaders { get; set; }
        public List<HttpHeader> ResponseHeaders { get; set; }
        public byte[] RequestBody { get; set; }
        public string RawDecodedRequestBody { get; set; }
        public byte[] ResponseBody { get; set; }
        public string RawDecodedResponseBody { get; set; }
        public RequestEnvelope RequestEnvelope { get; set; }
        public ResponseEnvelope ResponseEnvelope { get; set; }
        public MessageBlock RequestBlock { get; set; }
        public MessageBlock ResponseBlock { get; set; }


        public static List<PoGoWebRequest> GetAllRequests()
        {
            return MongoHelper.GetCollection<PoGoWebRequest>().Find(FilterDefinition<PoGoWebRequest>.Empty).Project(e=>new PoGoWebRequest {RequestBody = e.RequestBody}).ToList();
        }
    }
}