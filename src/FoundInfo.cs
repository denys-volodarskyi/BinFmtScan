namespace BinFmtScan;

public class FoundInfo
{
    public required long StartPosition;

    /// <summary>
    /// Optional size.
    /// </summary>
    public long Size = 0;

    public bool HasSize => Size > 0;
}
