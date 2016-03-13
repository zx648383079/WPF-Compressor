namespace ZoDream.Compressor.Helper.Compressor
{
    public interface ICssCompressor : ICompressor
    {
        bool RemoveComments { get; set; }
    }
}