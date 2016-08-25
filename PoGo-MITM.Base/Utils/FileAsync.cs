using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoGo_Proxy.Utils
{
    public static class FileAsync
    {
        public static async Task WriteAsync(string filePath, byte[] data)
        {
            using (var sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                4096, true))
            {
                await sourceStream.WriteAsync(data, 0, data.Length);
            };
        }

        public static async Task WriteTextAsync(string filePath, string text, Encoding encoding)
        {
            if (encoding == null) encoding = Encoding.ASCII;
            var encodedText = encoding.GetBytes(text);

            using (var sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.None, 4096, true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        public static async Task<byte[]> ReadAsync(string filePath)
        {
            byte[] result;
            using (var sourceStream = File.Open(filePath, FileMode.Open))
            {
                result = new byte[sourceStream.Length];
                await sourceStream.ReadAsync(result, 0, (int)sourceStream.Length);
            }
            return result;
        }

        public static async Task<string> ReadTextAsync(string filePath, Encoding encoding)
        {
            if (encoding == null) encoding = Encoding.ASCII;

            byte[] result;
            using (var sourceStream = File.Open(filePath, FileMode.Open))
            {
                result = new byte[sourceStream.Length];
                await sourceStream.ReadAsync(result, 0, (int)sourceStream.Length);
            }
            return encoding.GetString(result);
        }
    }
}
