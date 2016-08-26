using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Core;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.Owin.Hosting;
using Nancy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoGoMITM.Base;
using PoGoMITM.Base.Cache;
using PoGoMITM.Base.Config;
using PoGoMITM.Base.Logging;
using PoGoMITM.Base.Models;
using PoGoMITM.Launcher.Models;

namespace PoGoMITM.Launcher
{
    internal class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("Proxy");

        private static void Main()
        {
            var data = "16,136,21,34,43,10,3,103,112,115,16,176,197,191,207,255,255,255,255,255,1,37,0,0,0,192,109,8,208,25,66,117,38,149,217,65,173,1,0,0,160,65,208,1,3,224,1,1,34,42,10,7,110,101,116,119,111,114,107,16,186,253,255,255,255,255,255,255,255,1,109,128,211,25,66,117,169,87,217,65,173,1,162,197,101,66,208,1,3,224,1,1,42,174,2,18,18,2,3,6,7,9,16,22,23,26,30,68,69,70,78,80,84,85,86,26,72,0,128,156,67,0,0,250,66,0,128,137,67,0,0,75,67,0,128,165,67,0,0,156,66,0,0,254,66,0,0,44,66,0,0,72,66,0,0,81,67,0,0,204,66,0,0,100,66,0,0,161,67,0,0,224,65,0,0,43,67,0,0,99,67,0,0,136,67,0,0,162,67,34,72,0,0,112,65,0,0,28,66,0,0,32,66,0,0,60,66,0,0,134,66,0,0,176,65,0,0,152,65,0,0,100,66,0,0,96,65,0,0,160,65,0,0,144,65,0,0,108,66,0,0,20,66,0,0,144,65,0,0,56,66,0,0,0,64,0,0,112,65,0,0,32,65,42,72,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,50,18,1,1,1,1,1,1,1,1,1,1,0,0,1,0,1,0,0,1,58,18,1,1,1,1,1,1,1,1,1,1,0,0,1,0,1,0,0,1,66,18,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,58,36,8,134,19,129,1,0,0,0,96,237,155,199,63,137,1,0,0,0,32,149,207,208,191,145,1,0,0,0,160,80,154,35,64,152,1,3,66,193,1,10,16,57,98,51,53,51,102,51,57,55,97,51,48,49,54,98,97,18,10,104,97,109,109,101,114,104,101,97,100,26,6,72,72,90,50,48,104,34,6,103,111,111,103,108,101,42,10,104,97,109,109,101,114,104,101,97,100,50,6,77,79,66,51,48,89,58,10,104,97,109,109,101,114,104,101,97,100,66,3,76,71,69,74,7,78,101,120,117,115,32,53,82,10,104,97,109,109,101,114,104,101,97,100,98,12,114,101,108,101,97,115,101,45,107,101,121,115,106,4,117,115,101,114,114,67,103,111,111,103,108,101,47,104,97,109,109,101,114,104,101,97,100,47,104,97,109,109,101,114,104,101,97,100,58,54,46,48,46,49,47,77,79,66,51,48,89,47,51,48,54,55,52,54,56,58,117,115,101,114,47,114,101,108,101,97,115,101,45,107,101,121,115,74,0,80,180,200,237,155,255,255,255,255,255,1,160,1,146,175,130,178,1,178,1,16,128,75,9,72,32,106,50,210,116,245,200,3,176,113,61,146,184,1,193,168,212,196,236,42,194,1,20,234,221,204,193,212,230,165,204,158,1,151,211,221,194,155,154,191,151,233,1,200,1,132,142,231,252,192,209,191,152,102";
            var trimmed = data.Substring(3);
            trimmed = trimmed.Substring(0, trimmed.Length - 3);
            var res = data.Split(',');
            var arr = new byte[res.Length];
            arr = res.Select(byte.Parse).ToArray();

            StaticConfiguration.DisableErrorTraces = false;
            JsonConvert.DefaultSettings = () =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            };

            Log4NetHelper.AddAppender(Log4NetHelper.ConsoleAppender(Level.All));
            Log4NetHelper.AddAppender(Log4NetHelper.FileAppender(Level.All));


            var proxy = new ProxyHandler(AppConfig.ProxyIp, AppConfig.ProxyPort, Logger);

            proxy.RequestSent += Proxy_RequestSent;
            proxy.RequestCompleted += Proxy_RequestCompleted;

            proxy.Start();

            Logger.Info($"Proxy is started on {AppConfig.ProxyIp}:{AppConfig.ProxyPort}");

            var webApp = WebApp.Start<Startup>($"http://*:{AppConfig.WebServerPort}");
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

                ContextCache.RawContexts.TryAdd(rawContext.Guid, rawContext);
                NotificationHub.SendRawContext(RequestContextListModel.FromRawContext(rawContext));


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