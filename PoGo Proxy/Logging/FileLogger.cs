using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGo_Proxy.Models;

namespace PoGo_Proxy.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _logFileName;
        private readonly string _logFilePath;

        public FileLogger() : this(null) { }
        public FileLogger(string filename)
        {
            _logFileName = filename ?? string.Empty;
            _logFilePath = GenerateLogFileName();
        }

        public async Task Log(PoGoWebRequest webRequest)
        {
            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("==================================================");
            sb.AppendLine($"{DateTime.UtcNow} {webRequest.Uri.AbsoluteUri}");
            sb.AppendLine("=============== REQUEST BEGIN ====================");
            sb.AppendLine(JsonConvert.SerializeObject(webRequest, Formatting.Indented));
            sb.AppendLine("=============== RESPONSE END =====================");


            var encodedText = Encoding.UTF8.GetBytes(sb.ToString());

            using (var sourceStream = new FileStream(_logFilePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                4096, true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };


        }

        private string GenerateLogFileName()
        {
            var fileName = $"{_logFileName}{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
            var logFolder = Path.Combine(Environment.CurrentDirectory, "Logs");
            Directory.CreateDirectory(logFolder);
            return Path.Combine(logFolder, fileName);
        }
    }
}