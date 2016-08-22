using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Google.Protobuf;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using POGOProtos.Map;
using POGOProtos.Map.Pokemon;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Responses;

namespace PoGo_Proxy.Sample
{
    internal class Program
    {
        private static readonly HashSet<string> HostsToLog = new HashSet<string> { "sso.pokemon.com", "pgorelease.nianticlabs.com" };
        private static string _logFileName;

        private static void Main()
        {
            _logFileName = GenerateLogFileName();
            Console.WriteLine("Hit any key to stop the proxy and exit..");
            Console.WriteLine();

            var controller = new ProxyController("0.0.0.0", 8080) { Out = Console.Out };

            controller.RequestCompleted += Controller_RequestCompleted;
            controller.RequestSent += Controller_RequestSent;

            controller.Start();
            Console.ReadKey();
            controller.Stop();


        }

        private static void Controller_RequestSent(PoGoWebSession webSession)
        {
            if (!HostsToLog.Contains(webSession.RawRequest.RequestUri.Host)) return;

            Console.WriteLine(webSession.Uri + " Request Sent.");
            WriteToLogFile("\r\n=============== REQUEST BEGIN ====================");
            WriteToLogFile(DateTime.UtcNow + " " + webSession.Uri);
            if (webSession.RawRequest != null)
            {
                foreach (var value in webSession.RawRequest.RequestHeaders.Values)
                {
                    WriteToLogFile(value.Name + ": " + value.Value);
                }
            }
            if (webSession.RequestBlock != null)
            {
                Console.WriteLine(string.Join(Environment.NewLine, webSession.RequestBlock.ParsedMessages.Keys));
                WriteToLogFile(JsonConvert.SerializeObject(webSession.RequestBlock,Formatting.Indented));
            }
            WriteToLogFile("=============== REQUEST END ====++================");
        }

        private static void Controller_RequestCompleted(PoGoWebSession webSession)
        {

            if (!HostsToLog.Contains(webSession.RawRequest.RequestUri.Host)) return;
            WriteToLogFile("\r\n=============== RESPONSE BEGIN ===================");
            WriteToLogFile(DateTime.UtcNow + " " + webSession.Uri);
            Console.WriteLine(webSession.Uri + " Request Completed.");
            if (webSession.RawResponse != null)
            {
                foreach (var value in webSession.RawResponse.ResponseHeaders.Values)
                {
                    WriteToLogFile(value.Name + ": " + value.Value);
                }
            }
            if (webSession.ResponseBlock != null)
            {
                Console.WriteLine(string.Join(Environment.NewLine, webSession.ResponseBlock.ParsedMessages.Keys));
                WriteToLogFile(JsonConvert.SerializeObject(webSession.ResponseBlock, Formatting.Indented));
            }
            WriteToLogFile("=============== RESPONSE END =====================");
        }

        private static void WriteToLogFile(string text, bool addNewLine = true)
        {
            File.AppendAllText(_logFileName, text + (addNewLine ? Environment.NewLine : string.Empty));
        }

        private static string GenerateLogFileName()
        {
            var fileName = $"{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.log";
            var logFolder = Path.Combine(Environment.CurrentDirectory, "Logs");
            Directory.CreateDirectory(logFolder);
            return Path.Combine(logFolder, fileName);
        }


    }
}