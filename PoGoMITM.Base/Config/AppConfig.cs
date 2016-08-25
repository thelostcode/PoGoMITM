using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using PoGoMITM.Base.Dumpers;

namespace PoGoMITM.Base.Config
{
    public static class AppConfig
    {
        private static readonly int _bindPort;
        private static bool _dumpRaw;
        private static bool _dumpProcessed;

        public static string BindIp { get; private set; }
        public static int BindPort => _bindPort;
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
                foreach (var dumper in dumpersArr)
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
                HostsToDump = new HashSet<string>(hosts.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries));
            }
        }


    }
}
