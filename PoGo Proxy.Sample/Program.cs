using System;
using PoGo_Proxy.Config;
using PoGo_Proxy.Models;

namespace PoGo_Proxy.Sample
{
    internal class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hit any key to stop the proxy and exit..");
            Console.WriteLine();

            var controller = new ProxyController(AppConfig.BindIp, AppConfig.BindPort) { Out = Console.Out };

            controller.RequestCompleted += Controller_RequestCompleted;
            controller.RequestSent += Controller_RequestSent;

            controller.Start();
            Console.ReadKey();
            controller.Stop();

        }

        private static void Controller_RequestSent(PoGoWebRequest webRequest)
        {
            if (!AppConfig.HostsToLog.Contains(webRequest.Uri.Host)) return;
            Console.WriteLine(webRequest.Uri.AbsoluteUri + " Request Sent.");
        }

        private static async void Controller_RequestCompleted(PoGoWebRequest webRequest)
        {

            if (!AppConfig.HostsToLog.Contains(webRequest.Uri.Host)) return;
            Console.WriteLine(webRequest.Uri.AbsoluteUri + " Request Completed.");

            foreach (var logger in AppConfig.Loggers)
            {
                await logger.Log(webRequest);
            }

        }
    }
}