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
        static Protoc()
        {
            var tempFolder = Path.Combine(Environment.CurrentDirectory, "Temp");
            Directory.CreateDirectory(tempFolder);
            var files = Directory.GetFiles(tempFolder);
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        //var outputs = new List<string>();
        //var allRequests = PoGoWebRequest.GetAllRequests();
        //foreach (var request in allRequests.Where(r => r.RequestBody != null))
        //{
        //    var b = Protoc.DecodeRaw(request.RequestBody);
        //    if (b != null)
        //        outputs.Add(b.ToString());
        //}


        public static object DecodeRaw(byte[] data)
        {
            if (data == null) return null;
            var guid = Guid.NewGuid().ToString();
            var inPath = Path.Combine("Temp", guid + "-in");
            File.WriteAllBytes(inPath, data);
            var outPath = Path.Combine("Temp", guid + "-out");
            var arguments = $"--decode_raw < \"{inPath}\" > \"{outPath}\"";
//            var arguments = $"--decode_raw";
            var commandOutput = RunProtoc(arguments, data);
            //Console.Write(commandOutput);
            if (File.Exists(outPath))
            {
                return File.ReadAllText(outPath);
            }
            return null;

        }

        private static string RunProtoc(string arguments, byte[] data)
        {
            var sb = new StringBuilder();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd";
            startInfo.Arguments = $"/c protoc {arguments}";
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;
            //startInfo.RedirectStandardInput = true;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;

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
            //process.StandardInput.BaseStream.Write(data,0,data.Length);
            //new BinaryWriter(process.StandardInput.BaseStream).Write(data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit(5000);
            return sb.ToString();
        }
    }
}
