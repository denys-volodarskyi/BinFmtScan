namespace BinFmtScan;

internal interface IDetector
{
    string ID { get; }
    Category Category { get; }
    string Extension { get; }
    void Detect(BinarySource src, ref FoundInfo? res);
}