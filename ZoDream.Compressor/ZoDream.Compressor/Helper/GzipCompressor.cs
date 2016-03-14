using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.Compressor.Helper
{
    public class GzipCompressor
    {
        public string Compress(string source)
        {
            MemoryStream ms = new MemoryStream();
            Compress(source, ms);
            string content = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return content;
        }

        public void Compress(string source, Stream outputStream)
        {
            byte[] buffers = Encoding.UTF8.GetBytes(source);
            GZipStream compressedzipStream = new GZipStream(outputStream, CompressionMode.Compress, true);
            compressedzipStream.Write(buffers, 0, buffers.Length);
            compressedzipStream.Close();
        }

        public string Decompress(string source)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(source));
            string content = Decompress(ms);
            ms.Close();
            return content;
        }

        public string Decompress(Stream inputStream)
        {
            GZipStream compressedzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
            MemoryStream outBuffer = new MemoryStream();
            byte[] block = new byte[1024];
            while (true)
            {
                int bytesRead = compressedzipStream.Read(block, 0, block.Length);
                if (bytesRead <= 0)
                    break;
                else
                    outBuffer.Write(block, 0, bytesRead);
            }
            compressedzipStream.Close();
            string content = Encoding.UTF8.GetString(outBuffer.ToArray());
            outBuffer.Close();
            return content;
        }
    }
}
