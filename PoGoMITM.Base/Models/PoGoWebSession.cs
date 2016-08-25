using System;
using System.Linq;
using PoGoMITM.Base.ProtoHelpers;
using POGOProtos.Networking.Envelopes;
using Titanium.Web.Proxy.Http;

namespace PoGoMITM.Base.Models
{
    internal class PoGoWebSession
    {
        public string Uri { get; set; }
        public DateTime RequestTime { get; set; }
        public Guid Guid { get; set; }
        public Request RawRequest { get; set; }
        public byte[] RequestBody { get; set; }
        public Response RawResponse { get; set; }
        public byte[] ResponseBody { get; set; }
        public ulong RequestId { get; set; }
        public MessageBlock RequestBlock { get; set; }
        public MessageBlock ResponseBlock { get; set; }
        public RequestEnvelope RequestEnvelope { get; set; }
        public ResponseEnvelope ResponseEnvelope { get; set; }

        public PoGoWebRequest AsPoGoWebRequest()
        {
            var rawRequest = Protoc.DecodeRaw(RequestBody);
            var rawResponse = Protoc.DecodeRaw(ResponseBody);
            return new PoGoWebRequest
            {
                Uri = RawRequest.RequestUri,
                RequestTime = RequestTime,
                RequestHeaders = RawRequest.RequestHeaders.Values.ToList(),
                ResponseHeaders = RawResponse.ResponseHeaders.Values.ToList(),
                RequestBody = RequestBody,
                RawDecodedRequestBody = rawRequest?.ToString(),
                ResponseBody = ResponseBody,
                RawDecodedResponseBody = rawResponse?.ToString(),
                RequestEnvelope = RequestEnvelope,
                ResponseEnvelope = ResponseEnvelope,
                RequestBlock = RequestBlock,
                ResponseBlock = ResponseBlock
            };
        }
    }
}
