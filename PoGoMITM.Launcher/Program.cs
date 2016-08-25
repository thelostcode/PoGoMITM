using System;
using PoGoMITM.Base;
using PoGoMITM.Base.Config;
using PoGoMITM.Base.Models;

namespace PoGoMITM.Launcher
{
    internal class Program
    {
        private static void Main()
        {

            Console.WriteLine("Hit escape to stop the proxy and exit..");
            Console.WriteLine();

            var proxy = new ProxyHandler(AppConfig.BindIp, AppConfig.BindPort);

            proxy.RequestSent += Proxy_RequestSent;
            proxy.RequestCompleted += Proxy_RequestCompleted;

            proxy.Start();

            Console.WriteLine($"Proxy is started on {AppConfig.BindIp}:{AppConfig.BindPort}");

            while (true)
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        proxy.Stop();
                        return;
                }
            }
        }

        private static void Proxy_RequestSent(RawContext rawContext)
        {
            if (!AppConfig.HostsToDump.Contains(rawContext.RequestUri.Host)) return;
            Console.WriteLine(rawContext.RequestUri.AbsoluteUri + " Request Sent.");
        }

        private static async void Proxy_RequestCompleted(RawContext rawContext)
        {
            if (!AppConfig.HostsToDump.Contains(rawContext.RequestUri.Host)) return;
            Console.WriteLine(rawContext.RequestUri.AbsoluteUri + " Request Completed.");

            if (AppConfig.DumpRaw)
            {
                foreach (var dumper in AppConfig.DataDumpers)
                {
                    await dumper.Dump(rawContext);

                }
            }
            if (AppConfig.DumpProcessed)
            {
                var processedContext = await RequestContext.GetInstance(rawContext);
                foreach (var dumper in AppConfig.DataDumpers)
                {
                    await dumper.Dump(processedContext);

                }
            }
        }
    }
}