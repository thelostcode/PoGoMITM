using System;
using log4net;
using log4net.Core;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using PoGoMITM.Base;
using PoGoMITM.Base.Config;
using PoGoMITM.Base.Logging;
using PoGoMITM.Base.Models;

namespace PoGoMITM.Launcher
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("Proxy");

        private static void Main()
        {
            Log4NetHelper.AddAppender(Log4NetHelper.ConsoleAppender(Level.All));
            Log4NetHelper.AddAppender(Log4NetHelper.FileAppender(Level.All));


            var proxy = new ProxyHandler(AppConfig.ProxyIp, AppConfig.ProxyPort, Logger);

            proxy.RequestSent += Proxy_RequestSent;
            proxy.RequestCompleted += Proxy_RequestCompleted;

            proxy.Start();

            Logger.Info($"Proxy is started on {AppConfig.ProxyIp}:{AppConfig.ProxyPort}");

            var webApp = WebApp.Start<Startup>($"http://localhost:{AppConfig.WebServerPort}");
            Logger.Info($"Web Server is started on http://localhost:{AppConfig.WebServerPort}");

            Console.WriteLine();
            Logger.Info("Hit escape to stop the proxy and exit..");
            Console.WriteLine();


            while (true)
            {
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        proxy.Dispose();
                        Logger.Info("Proxy is stopped.");
                        webApp.Dispose();
                        Logger.Info("Web server is stopped.");
                        return;
                }
            }
        }

        private static void Proxy_RequestSent(RawContext rawContext)
        {
            NotificationHub.Send(rawContext.RequestUri.AbsoluteUri);
            try
            {
                if (!AppConfig.HostsToDump.Contains(rawContext.RequestUri.Host)) return;
                Logger.Info(rawContext.RequestUri.AbsoluteUri + " Request Sent.");
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }

        private static async void Proxy_RequestCompleted(RawContext rawContext)
        {
            try
            {
                if (!AppConfig.HostsToDump.Contains(rawContext.RequestUri.Host)) return;
                Logger.Info(rawContext.RequestUri.AbsoluteUri + " Request Completed.");

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
            catch (Exception ex)
            {
                Logger.LogException(ex);
            }
        }
    }
}