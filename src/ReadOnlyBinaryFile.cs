namespace BinFmtScan;

public class ReadOnlyBinaryFile(string path) : IDisposable
{
    private readonly Stream SourceStream = File.OpenRead(path);

    public BinarySource CreateReader(long offset, long length)
    {
        return new BinarySource(SourceStream, offset, length);
    }

    public void Dispose()
    {
        SourceStream.Dispose();
        GC.SuppressFinalize(this);
    }
}
