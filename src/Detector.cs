namespace BinFmtScan;

public interface IFoundPoint
{
    long StartPosition { get; }
}

public interface IFoundRange : IFoundPoint
{
    long Size { get; }
    long End => StartPosition + Size;
}

public interface IHasFileExtension
{
    string Extension { get; }
}

public interface ITextPreview
{
    string Text { get; }
}

internal interface IDetector
{
    string ID { get; }
    string Extension { get; }
    void Detect(BinarySource src, ref object? res);
}