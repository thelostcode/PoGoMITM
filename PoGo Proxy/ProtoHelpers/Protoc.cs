using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using PoGo_Proxy.Utils;

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


        public static async Task<string> DecodeRaw(byte[] data)
        {

            if (data == null || data.Length == 0) return null;
            var guid = Guid.NewGuid().ToString();
            var inPath = Path.Combine("Temp", guid + "-in");
            await FileAsync.WriteAsync(inPath, data);
            var outPath = Path.Combine("Temp", guid + "-out");
            var arguments = $"--decode_raw < \"{inPath}\" > \"{outPath}\"";
            var commandOutput = await RunProtoc(arguments);
            if (File.Exists(outPath))
            {
                return await FileAsync.ReadTextAsync(outPath, Encoding.ASCII);
            }
            return null;

        }

        private static Task<string> RunProtoc(string arguments)
        {
            var tcs = new TaskCompletionSource<string>();
            var sb = new StringBuilder();
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/c protoc {arguments}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };
            //startInfo.RedirectStandardInput = true;

            var process = new Process
            {
                StartInfo = startInfo,
                EnableRaisingEvents = true
            };
            process.ErrorDataReceived += (sender, args) =>
            {
                if (args.Data != null) sb.AppendLine(args.Data);
                tcs.SetResult(sb.ToString());
            };
            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null) sb.AppendLine(args.Data);
            };
            if (process.Start())
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit(5000);
                tcs.SetResult(sb.ToString());
            }
            //process.StandardInput.BaseStream.Write(data,0,data.Length);
            //new BinaryWriter(process.StandardInput.BaseStream).Write(data);
            return tcs.Task;
        }
    }
}
