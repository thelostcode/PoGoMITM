using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using PoGo_Proxy.Logging;

namespace PoGo_Proxy.Config
{
    public static class AppConfig
    {
        private static readonly int _bindPort;
        //<add key = "BindIP" value="0.0.0.0"/>
        //<add key = "BindPort" value="61221"/>
        //<add key = "RootCertificateName" value="POGO MITM.Net CA"/>
        //<add key = "RootCertificateIssuer" value="POGO MITM.Net"/>
        //<add key = "LogsFolder" value="Logs"/>
        //<add key = "TempFolder" value="Temp"/>
        //<add key = "Loggers" value="FileLogger,MongoLogger"/>
        //<add key = "HostsToLog" value="sso.pokemon.com,pgorelease.nianticlabs.com"/>

        public static string BindIp { get; private set; }

        public static int BindPort => _bindPort;

        public static string RootCertificateName { get; private set; }
        public static string RootCertificateIssuer { get; private set; }
        public static string LogsFolder { get; private set; }
        public static string TempFolder { get; private set; }
        public static List<ILogger> Loggers { get; private set; }
        public static HashSet<string> HostsToLog { get; private set; }

        static AppConfig()
        {
            BindIp = ConfigurationManager.AppSettings["BindIp"] ?? "0.0.0.0";
            if (!int.TryParse(ConfigurationManager.AppSettings["BindPort"], out _bindPort))
            {
                _bindPort = 61221;
            }
            RootCertificateName = ConfigurationManager.AppSettings["RootCertificateName"] ?? "POGO Proxy.Net CA";
            RootCertificateIssuer = ConfigurationManager.AppSettings["RootCertificateIssuer"] ?? "POGO Proxy";
            LogsFolder = ConfigurationManager.AppSettings["LogsFolder"] ?? "Logs";

            if (!Path.IsPathRooted(LogsFolder))
            {
                LogsFolder = Path.Combine(Environment.CurrentDirectory, LogsFolder);
            }

            TempFolder = ConfigurationManager.AppSettings["TempFolder"] ?? "Temp";

            if (!Path.IsPathRooted(TempFolder))
            {
                TempFolder = Path.Combine(Environment.CurrentDirectory, TempFolder);
            }

            Loggers = new List<ILogger>();
            var loggers = ConfigurationManager.AppSettings["Loggers"];
            if (!string.IsNullOrWhiteSpace(loggers))
            {
                var loggerArr = loggers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var logger in loggerArr)
                {
                    switch (logger.ToLowerInvariant())
                    {
                        case "filelogger":
                            Loggers.Add(new FileLogger());
                            break;
                        case "mongologger":
                            Loggers.Add(new MongoLogger());
                            break;
                    }
                }
            }

            HostsToLog = new HashSet<string>();
            var hosts = ConfigurationManager.AppSettings["HostsToLog"];
            if (!string.IsNullOrWhiteSpace(hosts))
            {
                HostsToLog = new HashSet<string>(hosts.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }
        }


    }
}
