﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using PoGoMITM.Base.Dumpers;

namespace PoGoMITM.Base.Config
{
    public static class AppConfig
    {
        private static readonly int _bindPort;
        private static readonly int _webServerPort;
        private static readonly bool _dumpRaw;
        private static readonly bool _dumpProcessed;

        public static string ProxyIp { get; private set; }
        public static int ProxyPort => _bindPort;
        public static int WebServerPort => _webServerPort;
        public static string RootCertificateName { get; private set; }
        public static string RootCertificateIssuer { get; private set; }
        public static string LogsFolder { get; private set; }
        public static string DumpsFolder { get; private set; }
        public static string TempFolder { get; private set; }
        public static List<IDataDumper> DataDumpers { get; private set; }

        public static bool DumpRaw => _dumpRaw;
        public static bool DumpProcessed => _dumpProcessed;

        public static HashSet<string> HostsToDump { get; private set; }

        static AppConfig()
        {
            ProxyIp = ConfigurationManager.AppSettings["ProxyIp"] ?? "0.0.0.0";
            if (!int.TryParse(ConfigurationManager.AppSettings["ProxyPort"], out _bindPort))
            {
                _bindPort = 61221;
            }
            if (!int.TryParse(ConfigurationManager.AppSettings["WebServerPort"], out _webServerPort))
            {
                _bindPort = 61222;
            }
            RootCertificateName = ConfigurationManager.AppSettings["RootCertificateName"] ?? "POGO Proxy.Net CA";
            RootCertificateIssuer = ConfigurationManager.AppSettings["RootCertificateIssuer"] ?? "POGO Proxy";

            LogsFolder = ConfigurationManager.AppSettings["LogsFolder"] ?? "Logs";
            if (!Path.IsPathRooted(LogsFolder))
            {
                LogsFolder = Path.Combine(Environment.CurrentDirectory, LogsFolder);
            }

            DumpsFolder = ConfigurationManager.AppSettings["DumpsFolder"] ?? "Dumps";
            if (!Path.IsPathRooted(DumpsFolder))
            {
                DumpsFolder = Path.Combine(Environment.CurrentDirectory, DumpsFolder);
            }

            TempFolder = ConfigurationManager.AppSettings["TempFolder"] ?? "Temp";
            if (!Path.IsPathRooted(TempFolder))
            {
                TempFolder = Path.Combine(Environment.CurrentDirectory, TempFolder);
            }

            var dumpRaw = ConfigurationManager.AppSettings["DumpRaw"] ?? "true";
            bool.TryParse(dumpRaw, out _dumpRaw);
            var dumpProcessed = ConfigurationManager.AppSettings["DumpProcessed"] ?? "true";
            bool.TryParse(dumpProcessed, out _dumpProcessed);

            DataDumpers = new List<IDataDumper>();
            var dumpers = ConfigurationManager.AppSettings["DataDumpers"];
            if (!string.IsNullOrWhiteSpace(dumpers))
            {
                var dumpersArr = dumpers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var dumper in dumpersArr.Select(d => d.Trim()))
                {
                    switch (dumper.ToLowerInvariant())
                    {
                        case "filedumper":
                            DataDumpers.Add(new FileDataDumper());
                            break;
                        case "mongodumper":
                            DataDumpers.Add(new MongoDataDumper());
                            break;
                    }
                }
            }

            HostsToDump = new HashSet<string>();
            var hosts = ConfigurationManager.AppSettings["HostsToDump"];
            if (!string.IsNullOrWhiteSpace(hosts))
            {
                HostsToDump = new HashSet<string>(hosts.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(h => h.Trim()));
            }
        }


    }
}
