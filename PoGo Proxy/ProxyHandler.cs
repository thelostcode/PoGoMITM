using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using PoGo_Proxy.Config;
using PoGo_Proxy.Models;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Exceptions;
using Titanium.Web.Proxy.Models;

namespace PoGo_Proxy
{
    public class ProxyHandler : IDisposable
    {
        public delegate void RequestSentEventHandler(RawContext rawContext);
        public delegate void RequestCompletedEventHandler(RawContext rawContext);

        private readonly ProxyServer _proxyServer;
        private Dictionary<string, RawContext> _contexts = new Dictionary<string, RawContext>();

        private readonly string _ip;
        private readonly int _port;

        public event RequestSentEventHandler RequestSent;
        public event RequestCompletedEventHandler RequestCompleted;

        public ProxyHandler(string ipAddress, int port)
        {
            _proxyServer = new ProxyServer(AppConfig.RootCertificateName, AppConfig.RootCertificateIssuer);
            _ip = ipAddress;
            _port = port;
        }

        public void Start()
        {
            // Link up handlers
            _proxyServer.Enable100ContinueBehaviour = true;
            _proxyServer.BeforeRequest += ProxyServer_BeforeRequest; ;
            _proxyServer.BeforeResponse += ProxyServer_BeforeResponse; ;
            _proxyServer.ServerCertificateValidationCallback += ProxyServer_ServerCertificateValidationCallback; ;
            _proxyServer.ClientCertificateSelectionCallback += ProxyServer_ClientCertificateSelectionCallback; ;

            // Set ip and port to monitor
            var explicitEndPoint = new ExplicitProxyEndPoint(IPAddress.Parse(_ip), _port, true);
            _proxyServer.AddEndPoint(explicitEndPoint);

            // Start proxy server
            _proxyServer.Start();
        }

        public void Stop()
        {
            // Unlink handlers
            _proxyServer.BeforeRequest -= ProxyServer_BeforeResponse;
            _proxyServer.BeforeResponse -= ProxyServer_BeforeRequest;
            _proxyServer.ServerCertificateValidationCallback -= ProxyServer_ServerCertificateValidationCallback;
            _proxyServer.ClientCertificateSelectionCallback -= ProxyServer_ClientCertificateSelectionCallback;

            // Stop server
            _proxyServer.Stop();
            _proxyServer.Dispose();
        }


        private async Task ProxyServer_BeforeRequest(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
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
                        headers.Add("Content-Disposition", new HttpHeader("Content-Disposition", $"inline; filename={AppConfig.RootCertificateName}.cer"));
                        await e.Ok(pem, headers);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("An exception occured.");
                        Console.WriteLine(ex.GetType().Name);
                        Console.WriteLine(ex.StackTrace);
                    }
                    return;
            }

            if (!AppConfig.HostsToDump.Contains(e.WebSession.Request.RequestUri.Host)) return;

            var uid = Guid.NewGuid();
            var context = new RawContext()
            {
                RequestHeaders = e.WebSession.Request.RequestHeaders.Values.ToList(),
                Guid = uid,
                RequestUri = e.WebSession.Request.RequestUri,
                RequestTime = DateTime.UtcNow
            };
            _contexts.Add(uid.ToString(), context);

            e.WebSession.Response.ResponseHeaders.Add("POGO_UID", new HttpHeader("POGO_UID", uid.ToString()));

            try
            {
                context.RequestBody = await e.GetRequestBody();
            }
            catch (BodyNotFoundException)
            {
            }
            OnRequestSent(context);

        }



        private async Task ProxyServer_BeforeResponse(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
        {
            if (!AppConfig.HostsToDump.Contains(e.WebSession.Request.RequestUri.Host)) return;

            RawContext context;
            if (e.WebSession.Response.ResponseHeaders.ContainsKey("POGO_UID") &&
                _contexts.ContainsKey(e.WebSession.Response.ResponseHeaders["POGO_UID"].Value))
            {
                context = _contexts[e.WebSession.Response.ResponseHeaders["POGO_UID"].Value];
            }
            else
            {
                Console.WriteLine("Couldn't find the RawContext for the response");
                return;
            }
            e.WebSession.Response.ResponseHeaders.Remove("POGO_UID");

            context.ResponseHeaders = e.WebSession.Response.ResponseHeaders.Values.ToList();
            try
            {
                context.ResponseBody = await e.GetResponseBody();
                await e.SetResponseBody(context.ResponseBody);
            }
            catch (BodyNotFoundException)
            {
            }
            _contexts.Remove(context.Guid.ToString());

            OnRequestCompleted(context);

        }

        private void OnRequestCompleted(RawContext context)
        {
            if (context == null) return;
            RequestCompleted?.Invoke(context);
        }

        private void OnRequestSent(RawContext context)
        {
            if (context == null) return;
            RequestSent?.Invoke(context);
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
                return store.Certificates.Cast<X509Certificate2>().FirstOrDefault(cert => cert.SubjectName.Name != null && cert.SubjectName.Name.Contains(AppConfig.RootCertificateName));
            }
            finally
            {
                store.Close();
            }

        }


        private static Task ProxyServer_ClientCertificateSelectionCallback(object arg1, Titanium.Web.Proxy.EventArguments.CertificateSelectionEventArgs e)
        {
            return Task.FromResult(0);
        }

        private static Task ProxyServer_ServerCertificateValidationCallback(object arg1, Titanium.Web.Proxy.EventArguments.CertificateValidationEventArgs e)
        {
            //set IsValid to true/false based on Certificate Errors
            if (e.SslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                e.IsValid = true;
            }

            return Task.FromResult(0);
        }

        public void Dispose()
        {
            try
            {
                _proxyServer.Dispose();
            }
            catch { }
        }


    }
}
