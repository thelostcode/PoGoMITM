using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Models;

namespace PoGo_Proxy
{
    public sealed class ProxyController
    {
        public delegate void RequestSentEventHandler(PoGoWebSession webSession);
        public delegate void RequestCompletedEventHandler(PoGoWebSession webSession);

        private Dictionary<string, PoGoWebSession> _webSessions = new Dictionary<string, PoGoWebSession>();
        public List<PoGoWebSession> WebSessions = new List<PoGoWebSession>();
        private readonly ProxyServer _proxyServer;
        //        private readonly Dictionary<ulong, RequestHandledEventArgs> _apiBlocks;

        public event RequestSentEventHandler RequestSent;
        public event RequestCompletedEventHandler RequestCompleted;

        public string Ip { get; }
        public int Port { get; }
        public TextWriter Out { get; set; }

        public ProxyController(string ipAddress, int port)
        {
            _proxyServer = new ProxyServer("POGO Proxy.Net CA", "POGO Proxy");
            //_proxyServer.RootCertificateIssuerName = "PoGoProxy";
            //_proxyServer.RootCertificateName = "PoGoProxy Root CA";
            //_apiBlocks = new Dictionary<ulong, RequestHandledEventArgs>();

            Ip = ipAddress;
            Port = port;
        }

        public void Start()
        {
            // Link up handlers
            _proxyServer.Enable100ContinueBehaviour = true;
            _proxyServer.BeforeRequest += OnRequest;
            _proxyServer.BeforeResponse += OnResponse;
            _proxyServer.ServerCertificateValidationCallback += OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback += OnCertificateSelection;

            // Set ip and port to monitor
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Parse(Ip), Port, true);
            _proxyServer.AddEndPoint(explicitEndPoint);

            // Start proxy server
            _proxyServer.Start();

            if (Out != StreamWriter.Null)
            {
                Out.WriteLine($"[+++] Proxy started: listening at {explicitEndPoint.IpAddress}:{explicitEndPoint.Port} ");
                Out.WriteLine();
            }
        }

        public void Stop()
        {
            // Unlink handlers
            _proxyServer.BeforeRequest -= OnRequest;
            _proxyServer.BeforeResponse -= OnResponse;
            _proxyServer.ServerCertificateValidationCallback -= OnCertificateValidation;
            _proxyServer.ClientCertificateSelectionCallback -= OnCertificateSelection;

            // Stop server
            _proxyServer.Stop();

            if (Out != StreamWriter.Null) Out.WriteLine("[---] Proxy stopped");
        }


        private async Task OnRequest(object sender, SessionEventArgs e)
        {
            switch (e.WebSession.Request.RequestUri.PathAndQuery)
            {
                case "/install-cert":

                    await e.Ok("<html><head><title>Certificate Installation</title></head><body><div style=\"text-align: center; margin-top: 100px;\"><h1><a href=/install-cert-android>Download Certificate</a></h1></div></body></html>");
                    return;
                case "/install-cert-android":
                    try
                    {
                        var cert = GetCertificateFromStore();
                        var pem = ConvertToPem(cert);
                        var headers = new Dictionary<string, HttpHeader>();
                        headers.Add("Content-Type", new HttpHeader("Content-Type", "application/x-x509-ca-cert"));
                        headers.Add("Content-Disposition", new HttpHeader("Content-Disposition", "inline; filename=cert.pfx"));
                        await e.Ok(pem, headers);
                    }
                    catch (Exception ex)
                    {
                        Out.WriteLine("An exception occured.");
                        Out.WriteLine(ex.GetType().Name);
                        Out.WriteLine(ex.StackTrace);
                    }
                    return;
            }
            Console.WriteLine(e.WebSession.Request.RequestUri.AbsoluteUri + " Request initialized.");

            var uid = Guid.NewGuid();
            var pogoWebSession = new PoGoWebSession
            {
                RawResponse = e.WebSession.Response,
                RawRequest = e.WebSession.Request,
                Guid = uid,
                Uri = e.WebSession.Request.RequestUri.AbsoluteUri
            };
            _webSessions.Add(uid.ToString(), pogoWebSession);

            e.WebSession.Response.ResponseHeaders.Add("POGO_UID", new HttpHeader("POGO_UID", uid.ToString()));

            try
            {
                pogoWebSession.RequestBody = await e.GetRequestBody();
            }
            catch (BodyNotFoundException)
            {
                OnRequestSent(pogoWebSession);
                return;
            }

            if (e.WebSession.Request.RequestUri.Host != "pgorelease.nianticlabs.com")
            {
                OnRequestSent(pogoWebSession);
                return;
            }

            if (e.WebSession.Request.RequestHeaders.ContainsKey("Connection"))
            {
                e.WebSession.Request.RequestHeaders["Connection"].Value =
                    e.WebSession.Request.RequestHeaders["Connection"].Value.Replace("Keep-Alive", "Close");
            }

            try
            {

                // Get session data
                var callTime = DateTime.Now;


                var codedInputStream = new CodedInputStream(pogoWebSession.RequestBody);
                var requestEnvelope = RequestEnvelope.Parser.ParseFrom(codedInputStream);

                if (requestEnvelope == null)
                {
                    OnRequestSent(pogoWebSession);
                    return;
                }

                pogoWebSession.RequestId = requestEnvelope.RequestId;
                // Initialize the request block
                var requestBlock = new MessageBlock
                {
                    MessageInitialized = callTime,
                    ParsedMessages = new Dictionary<RequestType, IMessage>()
                };

                // Parse all the requests
                foreach (var request in requestEnvelope.Requests)
                {
                    // Had to add assembly name to end of typeName string since protocs cs files are in a different assembly
                    var type = Type.GetType("POGOProtos.Networking.Requests.Messages." + request.RequestType + "Message,POGOProtos");

                    if (type == null)
                    {
                        Console.WriteLine("[*] GetType returns null for requestType: " + request.RequestType);
                        Console.WriteLine("[*] Check if POGOProtos.Networking.Requests.Messages." + request.RequestType + "Message exists.");
                        Console.WriteLine("[*]");

                        requestBlock.ParsedMessages.Add(request.RequestType, default(IMessage));
                    }
                    else
                    {
                        var instance = (IMessage)Activator.CreateInstance(type);
                        instance.MergeFrom(request.RequestMessage);

                        requestBlock.ParsedMessages.Add(request.RequestType, instance);
                    }
                }
                pogoWebSession.RequestBlock = requestBlock;

                //// TODO check if there is a double response or if a response already exists
                //if (_apiBlocks.ContainsKey(requestEnvelope.RequestId))
                //{

                //    //Console.WriteLine($"[*] Request Id({requestEnvelope.RequestId}) already exists.");
                //    //Console.WriteLine($"[*] Old request:\n{_apiBlocks[requestEnvelope.RequestId].RequestBlock}");
                //    //Console.WriteLine($"[*] New request:\n{requestBlock}");


                //    if (_apiBlocks[requestEnvelope.RequestId].ResponseBlock == null)
                //    {
                //        //Console.WriteLine($"[*] Response for the old request doesn't - replacing old request");
                //        _apiBlocks[requestEnvelope.RequestId].RequestBlock = requestBlock;
                //    }
                //    else
                //    {
                //        //Console.WriteLine($"[*] Response for the old request exists - do nothing");
                //    }
                //    //Out.WriteLine("[*]\n");

                //    return;
                //}

                //// Initialize a new request/response paired block and track it to update response
                //var args = new RequestHandledEventArgs
                //{
                //    RequestId = requestEnvelope.RequestId,
                //    RequestBlock = requestBlock,

                //};
                //_apiBlocks.Add(args.RequestId, args);
                OnRequestSent(pogoWebSession);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception on request handler.");
                Console.WriteLine(ex.GetType().Name);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private async Task OnResponse(object sender, SessionEventArgs e)
        {
            if (e.WebSession.Request.RequestUri.AbsoluteUri == "https://www.google.com/loc/m/api")
            {
                return;
            }
            if (e.WebSession.Request.RequestUri.PathAndQuery.StartsWith("/install-cert"))
            {
                return;
            }

            PoGoWebSession pogoWebSession;
            if (e.WebSession.Response.ResponseHeaders.ContainsKey("POGO_UID") &&
                _webSessions.ContainsKey(e.WebSession.Response.ResponseHeaders["POGO_UID"].Value))
            {
                pogoWebSession = _webSessions[e.WebSession.Response.ResponseHeaders["POGO_UID"].Value];
            }
            else
            {
                Console.WriteLine("Couldn't find the PoGoWebSession for the response");
                return;
            }
            e.WebSession.Response.ResponseHeaders.Remove("POGO_UID");


            try
            {
                pogoWebSession.ResponseBody = await e.GetResponseBody();
                //Console.WriteLine(Encoding.UTF8.GetString(pogoWebSession.ResponseBody));
                await e.SetResponseBody(pogoWebSession.ResponseBody);
            }
            catch (BodyNotFoundException)
            {
                OnRequestCompleted(pogoWebSession);
                return;
            }

            if (e.WebSession.Request.RequestUri.Host != "pgorelease.nianticlabs.com")
            {
                OnRequestCompleted(pogoWebSession);
                return;
            }
            if (e.WebSession.Response.ResponseStatusCode == "200")
            {
                try
                {

                    // Get session data
                    var callTime = DateTime.Now;

                    var codedInputStream = new CodedInputStream(pogoWebSession.ResponseBody);
                    var responseEnvelope = ResponseEnvelope.Parser.ParseFrom(codedInputStream);

                    // Initialize the response block
                    var responseBlock = new MessageBlock
                    {
                        MessageInitialized = callTime,
                        ParsedMessages = new Dictionary<RequestType, IMessage>()
                    };
                    pogoWebSession.ResponseBlock = responseBlock;
                    // Grab the paired request
                    //var args = _apiBlocks[responseEnvelope.RequestId];

                    // Grab request types
                    var requestTypes = pogoWebSession.RequestBlock.ParsedMessages.Keys.ToList();

                    // The case of missmatched requests and responses seems to be a handshake. The inital request sends 5 messages and gets back 2 that are empty bytestrings
                    if (pogoWebSession.RequestBlock.ParsedMessages.Count != responseEnvelope.Returns.Count)
                    {
                        if ((pogoWebSession.RequestBlock.ParsedMessages.Count == 5 && responseEnvelope.Returns.Count == 2) &&
                            (responseEnvelope.Returns[0].IsEmpty && responseEnvelope.Returns[1].IsEmpty))
                        {
                            // if (Out != StreamWriter.Null) Out.WriteLine($"[*] Handshake complete\n");
                            //_apiBlocks.Remove(args.RequestId);
                            OnRequestCompleted(pogoWebSession);
                            return;
                        }

                        // If there is a case of missmatched requests and responses, and it doesn't look like a handshake, log it
                        //if (Out != StreamWriter.Null)
                        //{
                        //    Out.WriteLine($"[*] Request messages count ({args.RequestBlock.ParsedMessages.Count}) is different than the response messages count ({responseEnvelope.Returns.Count}).");

                        //    Out.WriteLine("Request:");
                        //    Out.WriteLine(args.RequestBlock);

                        //    Out.WriteLine("Response:");
                        //    Out.WriteLine("Not sure yet how to read this without knowing what it is.");

                        //    Out.WriteLine($"[*]\n");
                        //}

                        // This section converts all of the responses into all of the request types that exist to see which one fits
                        int responseIndex = 0;

                        foreach (var responseBytes in responseEnvelope.Returns)
                        {
                            int start = responseIndex;
                            int end = responseIndex + requestTypes.Count - responseEnvelope.Returns.Count;

                            // Parse the responses
                            for (int i = start; i < end; i++)
                            {
                                Type type = null;
                                try
                                {
                                    type = Type.GetType("POGOProtos.Networking.Responses." + requestTypes[i] + "Response,POGOProtos");
                                }
                                catch (Exception)
                                {
                                    var requestType = RequestType.MethodUnset;
                                    if (requestTypes.Count >= i)
                                    {
                                        requestType = requestTypes[i];
                                    }
                                    Console.WriteLine("Couldn't get the type");
                                    Console.WriteLine("[***] GetType returns null for requestType");
                                    Console.WriteLine("[***] Check if POGOProtos.Networking.Requests.Messages." + requestType + "Message exists.");
                                }
                                if (type != null)
                                {
                                    var instance = (IMessage)Activator.CreateInstance(type);
                                    instance.MergeFrom(responseBytes);
                                }

                                //Out.WriteLine($"[*] Parsing as response {responseIndex} as {requestTypes[i]}");
                                //Out.WriteLine(JsonConvert.SerializeObject(instance));
                            }

                            responseIndex++;
                            //Out.WriteLine();
                        }
                        //Out.WriteLine($"[*]\n");
                    }

                    // Parse the responses
                    for (int i = 0; i < responseEnvelope.Returns.Count; i++)
                    {
                        // Had to add assembly name to end of typeName string since protocs.cs files are in a different assembly
                        Type type = null;
                        try
                        {
                            type = Type.GetType("POGOProtos.Networking.Responses." + requestTypes[i] + "Response,POGOProtos");

                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Couldn't get the type");
                            Console.WriteLine("[***] GetType returns null for requestType");
                            Console.WriteLine("[***] Check if POGOProtos.Networking.Requests.Messages. Message exists.");
                        }

                        if (type == null)
                        {

                            responseBlock.ParsedMessages.Add(RequestType.MethodUnset, default(IMessage));
                        }
                        else
                        {
                            var instance = (IMessage)Activator.CreateInstance(type);
                            instance.MergeFrom(responseEnvelope.Returns[i]);

                            responseBlock.ParsedMessages.Add(requestTypes[i], instance);
                        }
                    }

                    // Have not seen this issue yet - here just in case
                    //if (!_apiBlocks.ContainsKey(responseEnvelope.RequestId))
                    //{

                    //    Console.WriteLine($"[*] Request doesn't exist with specified RequestId ({responseEnvelope.RequestId}).");
                    //    Console.WriteLine($"Response:\n{responseBlock}");
                    //    Console.WriteLine("[*]\n");

                    //}

                    // Remove block from dictionary and invoke event handler
                    //args.ResponseBlock = responseBlock;
                    //_apiBlocks.Remove(args.RequestId);

                    OnRequestCompleted(pogoWebSession);
                }
                catch (Exception ex)
                {
                    OnRequestCompleted(pogoWebSession);
                    Console.WriteLine("An exception on response handler occured.");
                    Console.WriteLine(ex.GetType().Name);
                    Console.WriteLine(ex.StackTrace);
                }

            }

        }

        /// <summary>
        /// Allows overriding default certificate validation logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static Task OnCertificateValidation(object sender, CertificateValidationEventArgs e)
        {
            //set IsValid to true/false based on Certificate Errors
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                e.IsValid = true;
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Allows overriding default client certificate selection logic during mutual authentication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static Task OnCertificateSelection(object sender, CertificateSelectionEventArgs e)
        {
            return Task.FromResult(0);
        }

        private static string ConvertToPem(X509Certificate2 cert)
        {
            if (cert == null)
                throw new ArgumentNullException(nameof(cert));

            var base64 = Convert.ToBase64String(cert.RawData).Trim();

            var pem = "-----BEGIN CERTIFICATE-----" + Environment.NewLine;

            do
            {
                pem += base64.Substring(0, 64) + Environment.NewLine;
                base64 = base64.Remove(0, 64);
            }
            while (base64.Length > 64);

            pem += base64 + Environment.NewLine + "-----END CERTIFICATE-----";

            return pem;
        }

        private static X509Certificate2 GetCertificateFromStore()
        {

            // Get the certificate store for the current user.
            var store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                return store.Certificates.Cast<X509Certificate2>().FirstOrDefault(cert => cert.SubjectName.Name != null && cert.SubjectName.Name.Contains("POGO Proxy.Net CA"));
            }
            finally
            {
                store.Close();
            }

        }


        private void OnRequestCompleted(PoGoWebSession websession)
        {
            if (websession == null) return;
            WebSessions.Add(websession);
            RequestCompleted?.Invoke(websession);
        }

        private void OnRequestSent(PoGoWebSession websession)
        {
            if (websession == null) return;
            RequestSent?.Invoke(websession);
        }
    }
}
