using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Compressor.Helper.Compressor;
using ZoDream.Compressor.Model;

namespace ZoDream.Compressor.Helper
{
    public class CompressorHelper
    {
        public bool RemoveComments { get; set; } = false;

        public bool GzipCompress { get; set; } = false;

        public void Compress(string Path)
        {
            Compress(new FileItem(Path));
        }

        public void Compress(FileItem item)
        {
            TxtEncoder txtEncoder = new TxtEncoder();
            StreamReader sr = new StreamReader(item.Path, txtEncoder.GetEncoding(item.Path));
            string content = sr.ReadToEnd();
            sr.Close();
            string result = null;
            try
            {
                switch (item.Kind)
                {
                    case FileKind.Css:
                        CssCompressor compressor = new CssCompressor();
                        compressor.RemoveComments = RemoveComments;
                        result = compressor.Compress(content);
                        break;
                    case FileKind.Js:
                        JavaScriptCompressor jscompressor = new JavaScriptCompressor();
                        result = jscompressor.Compress(content);
                        break;
                    default:
                        throw new ArgumentException("File Kind Is Error!");
                }
                FileStream fs = new FileStream(item.getCompressorPath(), FileMode.CreateNew);
                if (GzipCompress)
                {
                    GzipCompressor gzip = new GzipCompressor();
                    gzip.Compress(result, fs);
                } else
                {
                    StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                    sw.Write(result);
                    sw.Close();
                }
                fs.Close();
                item.Status = FileStatus.Complete;
            }
            catch (Exception ex)
            {
                item.Status = FileStatus.Failure;
                item.Message = ex.Message;
            }
        }
    }
}
