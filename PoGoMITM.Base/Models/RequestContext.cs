using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PoGoMITM.Base.ProtoHelpers;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using Titanium.Web.Proxy.Models;

namespace PoGoMITM.Base.Models
{
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

            if (result.RequestUri.Host == "pgorelease.nianticlabs.com")
            {

                result.RawDecodedRequestBody = await Protoc.DecodeRaw(result.RequestBody);
                result.RawDecodedResponseBody = await Protoc.DecodeRaw(result.ResponseBody);

                var codedRequest = new CodedInputStream(result.RequestBody);
                result.RequestEnvelope = RequestEnvelope.Parser.ParseFrom(codedRequest);
                if (result.RequestEnvelope != null && result.RequestEnvelope.Requests != null &&
                    result.RequestEnvelope.Requests.Count > 0)
                {
                    foreach (var request in result.RequestEnvelope.Requests)
                    {
                        if (Enum.IsDefined(typeof(RequestType), request.RequestType))
                        {
                            Console.WriteLine(request.RequestType);
                        }
                        else
                        {
                            Console.WriteLine($"Unknown RequestType {request.RequestType}");
                        }
                    }
                }

                var codedResponse = new CodedInputStream(result.ResponseBody);
                result.ResponseEnvelope = ResponseEnvelope.Parser.ParseFrom(codedResponse);
            }
            else
            {
                if (result.RequestBody != null)
                    result.RawDecodedRequestBody = Encoding.UTF8.GetString(result.RequestBody);
                if (result.ResponseBody != null)
                    result.RawDecodedResponseBody = Encoding.UTF8.GetString(result.ResponseBody);
            }



            return result;
        }

    }
}