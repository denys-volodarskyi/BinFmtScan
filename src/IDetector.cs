namespace BinFmtScan;

internal interface IDetector
{
    string ID { get; }
    string Extension { get; }
    void Detect(BinarySource src, ref FoundInfo? res);
}