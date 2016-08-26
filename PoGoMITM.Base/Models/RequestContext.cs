using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using PoGoMITM.Base.Cache;
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
        [JsonIgnore]
        public byte[] RequestBody { get; set; }
        [JsonIgnore]
        public string RawDecodedRequestBody { get; set; }
        public RequestEnvelope RequestEnvelope { get; set; }
        public Dictionary<RequestType, IMessage> Requests { get; set; } = new Dictionary<RequestType, IMessage>();
        public int RequestBodyLength { get; set; }

        // Response
        public List<HttpHeader> ResponseHeaders { get; set; }
        [JsonIgnore]
        public byte[] ResponseBody { get; set; }
        [JsonIgnore]
        public string RawDecodedResponseBody { get; set; }
        public ResponseEnvelope ResponseEnvelope { get; set; }
        public Dictionary<RequestType, IMessage> Responses { get; set; } = new Dictionary<RequestType, IMessage>();
        public int ResponseBodyLength { get; set; }

        public static async Task<RequestContext> GetInstance(RawContext context)
        {
            RequestContext requestContext;
            if (ContextCache.RequestContext.TryGetValue(context.Guid, out requestContext))
            {
                return requestContext;
            }
            var result = new RequestContext();

            result.Guid = context.Guid;
            result.RequestTime = context.RequestTime;
            result.RequestUri = context.RequestUri;

            result.RequestBody = context.RequestBody;
            result.RequestBodyLength = context.RequestBody?.Length ?? 0;
            result.ResponseBody = context.ResponseBody;
            result.ResponseBodyLength = context.ResponseBody?.Length ?? 0;

            result.RequestHeaders = context.RequestHeaders;
            result.ResponseHeaders = context.ResponseHeaders;

            if (result.RequestUri.Host == "pgorelease.nianticlabs.com")
            {

                result.RawDecodedRequestBody = await Protoc.DecodeRaw(result.RequestBody);
                result.RawDecodedResponseBody = await Protoc.DecodeRaw(result.ResponseBody);

                var codedRequest = new CodedInputStream(result.RequestBody);
                result.RequestEnvelope = RequestEnvelope.Parser.ParseFrom(codedRequest);
                if (result.RequestEnvelope?.Requests != null && result.RequestEnvelope.Requests.Count > 0)
                {
                    foreach (var request in result.RequestEnvelope.Requests)
                    {
                        if (Enum.IsDefined(typeof(RequestType), request.RequestType))
                        {
                            var type = Type.GetType("POGOProtos.Networking.Requests.Messages." + request.RequestType + "Message");
                            if (type != null)
                            {
                                var instance = (IMessage)Activator.CreateInstance(type);
                                instance.MergeFrom(request.RequestMessage);
                                result.Requests.Add(request.RequestType, instance);
                            }
                            else
                            {
                                result.Requests.Add(request.RequestType, request);
                            }
                        }
                        else
                        {
                            result.Requests.Add(request.RequestType, request);
                        }
                    }
                }

                var codedResponse = new CodedInputStream(result.ResponseBody);
                result.ResponseEnvelope = ResponseEnvelope.Parser.ParseFrom(codedResponse);
                if (result.ResponseEnvelope?.Returns != null && result.ResponseEnvelope.Returns.Count > 0)
                {
                    var index = 0;
                    foreach (var request in result.Requests)
                    {
                        if (Enum.IsDefined(typeof(RequestType), request.Key))
                        {
                            var requestType = (RequestType)request.Key;
                            var typeName = "POGOProtos.Networking.Responses." + requestType + "Response";
                            var type = Type.GetType(typeName);
                            if (type != null)
                            {
                                var instance = (IMessage)Activator.CreateInstance(type);
                                instance.MergeFrom(result.ResponseEnvelope.Returns[index]);
                                result.Responses.Add(requestType, instance);
                            }
                            else
                            {
                                result.Responses.Add(requestType, null);
                            }
                        }
                        else
                        {
                            result.Responses.Add(request.Key, null);
                        }
                        index++;
                    }
                }
            }
            else
            {
                if (result.RequestBody != null)
                    result.RawDecodedRequestBody = Encoding.UTF8.GetString(result.RequestBody);
                if (result.ResponseBody != null)
                    result.RawDecodedResponseBody = Encoding.UTF8.GetString(result.ResponseBody);
            }


            ContextCache.RequestContext.TryAdd(context.Guid, result);
            return result;
        }

    }
}