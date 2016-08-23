using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGo_Proxy.Logging;
using PoGo_Proxy.Models;
using POGOProtos.Map;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Responses;

namespace PoGo_Proxy.Sample
{
    internal class Program
    {
        private static readonly HashSet<string> HostsToLog = new HashSet<string> { "sso.pokemon.com", "pgorelease.nianticlabs.com" };
        private static List<ILogger> _loggers = new List<ILogger>();

        private static void Main()
        {
            Console.WriteLine("Hit any key to stop the proxy and exit..");
            Console.WriteLine();

            _loggers.Add(new FileLogger());
            _loggers.Add(new MongoLogger("mongodb://127.0.0.1/PoGoMITM?maxPoolSize=1000&minPoolSize=100"));

            var controller = new ProxyController("0.0.0.0", 8080) { Out = Console.Out };

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

            foreach (var logger in _loggers)
            {
                await logger.Log(webRequest);
            }

        }




    }
}