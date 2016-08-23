using System;
using System.Linq;
using POGOProtos.Networking.Envelopes;
using Titanium.Web.Proxy.Http;

namespace PoGo_Proxy.Models
{
    public class PoGoWebSession
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
            return new PoGoWebRequest
            {
                Uri = RawRequest.RequestUri,
                RequestTime = RequestTime,
                RequestHeaders = RawRequest.RequestHeaders.Values.ToList(),
                ResponseHeaders = RawResponse.ResponseHeaders.Values.ToList(),
                RequestBody = RequestBody,
                ResponseBody = ResponseBody,
                RequestEnvelope = RequestEnvelope,
                ResponseEnvelope = ResponseEnvelope,
                RequestBlock = RequestBlock,
                ResponseBlock = ResponseBlock
            };
        }
    }
}
