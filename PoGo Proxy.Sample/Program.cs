using System;
using System.Collections.Generic;
using System.Linq;
using PoGo_Proxy.Logging;
using PoGo_Proxy.Models;
using PoGo_Proxy.MongoDB;
using PoGo_Proxy.ProtoHelpers;

namespace PoGo_Proxy.Sample
{
    internal class Program
    {
        private static readonly HashSet<string> HostsToLog = new HashSet<string> { "sso.pokemon.com", "pgorelease.nianticlabs.com" };
        private static readonly List<ILogger> Loggers = new List<ILogger>();

        private static void Main()
        {
            //var allRequests = PoGoWebRequest.GetAllRequests();
            //foreach (var request in allRequests.Where(r => r.RequestBody != null))
            //{
            //    var b = Protoc.DecodeRaw(request.RequestBody);
            //}
            Console.WriteLine("Hit any key to stop the proxy and exit..");
            Console.WriteLine();

            Loggers.Add(new FileLogger());
            Loggers.Add(new MongoLogger());
            var controller = new ProxyController("0.0.0.0", 61221) { Out = Console.Out };

            controller.RequestCompleted += Controller_RequestCompleted;
            controller.RequestSent += Controller_RequestSent;

            controller.Start();
            Console.ReadKey();
            controller.Stop();


        }

        private static void Controller_RequestSent(PoGoWebRequest webRequest)
        {
            if (!HostsToLog.Contains(webRequest.Uri.Host)) return;
            Console.WriteLine(webRequest.Uri.AbsoluteUri + " Request Sent.");
        }

        private static async void Controller_RequestCompleted(PoGoWebRequest webRequest)
        {

            if (!HostsToLog.Contains(webRequest.Uri.Host)) return;
            Console.WriteLine(webRequest.Uri.AbsoluteUri + " Request Completed.");

            foreach (var logger in Loggers)
            {
                await logger.Log(webRequest);
            }

        }




    }
}