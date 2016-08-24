using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo_Proxy.ProtoHelpers
{
    public static class Protoc
    {
        private static readonly string ProtocPath;
        private static readonly string TempFolder;
        static Protoc()
        {
            TempFolder = Path.Combine(Environment.CurrentDirectory, "Temp");
            ProtocPath = Path.Combine(Environment.CurrentDirectory, "protoc.exe");
            Directory.CreateDirectory(TempFolder);
        }
        public static object DecodeRaw(byte[] data)
        {
            var guid = Guid.NewGuid().ToString();
            var inPath = Path.Combine(TempFolder, guid + "-in");
            File.WriteAllBytes(inPath, data);
            var outPath = Path.Combine(TempFolder, guid + "-out");
            var arguments = $"--decode_raw \"{inPath}\" \"{outPath}\"";
            var commandOutput = RunProtoc(arguments);
            Console.Write(commandOutput);
            if (File.Exists(outPath))
            {
                return File.ReadAllText(outPath);
            }
            return null;

        }

        private static string RunProtoc(string arguments)
        {
            var sb = new StringBuilder();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ProtocPath;
            startInfo.Arguments = arguments;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null) sb.AppendLine(args.Data);
            };
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null) sb.AppendLine(args.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit(5000);
            return sb.ToString();
        }
    }
}
