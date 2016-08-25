using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PoGo_Proxy.Config;
using PoGo_Proxy.Models;
using PoGo_Proxy.Utils;

namespace PoGo_Proxy.Dumpers
{
    public class FileDataDumper : IDataDumper
    {
        public async Task Dump<T>(T context)
        {
            try
            {
                var logFilePath = GenerateLogFileName(typeof(T).Name);
                var sb = new StringBuilder();
                sb.AppendLine();

                sb.AppendLine(JsonConvert.SerializeObject(context, Formatting.Indented));
                await FileAsync.WriteTextAsync(logFilePath, sb.ToString(), Encoding.ASCII);

            }
            catch (Exception)
            {
            }

        }

        private string GenerateLogFileName(string name)
        {
            var fileName = $"{name}{DateTime.Now.ToString("yyyyMMddHHmmss")}.log";
            Directory.CreateDirectory(AppConfig.DumpsFolder);
            return Path.Combine(AppConfig.DumpsFolder, fileName);
        }
    }
}