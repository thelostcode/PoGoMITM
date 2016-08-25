using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using PoGoMITM.Base.ProtoHelpers;
using POGOProtos.Networking.Envelopes;
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

    public class RequestContext
    {
        public Guid Guid { get; set; }
        public DateTime RequestTime { get; set; }
        public Uri RequestUri { get; set; }

        // Request
        public List<HttpHeader> RequestHeaders { get; set; }
        public byte[] RequestBody { get; set; }
        public string RawDecodedRequestBody { get; set; }
        public RequestEnvelope RequestEnvelope { get; set; }
        public MessageBlock RequestBlock { get; set; }

        // Response
        public List<HttpHeader> ResponseHeaders { get; set; }
        public byte[] ResponseBody { get; set; }
        public string RawDecodedResponseBody { get; set; }
        public ResponseEnvelope ResponseEnvelope { get; set; }
        public MessageBlock ResponseBlock { get; set; }

        public static async Task<RequestContext> GetInstance(RawContext context)
        {
            var result = new RequestContext();

            result.Guid = context.Guid;
            result.RequestTime = context.RequestTime;
            result.RequestUri = context.RequestUri;

            result.RequestBody = context.RequestBody;
            result.ResponseBody = context.ResponseBody;

            result.RequestHeaders = context.RequestHeaders;
            result.ResponseHeaders = context.ResponseHeaders;

            result.RawDecodedRequestBody = await Protoc.DecodeRaw(result.RequestBody);
            result.RawDecodedResponseBody = await Protoc.DecodeRaw(result.ResponseBody);

            return result;
        }

    }
}
