namespace BinFmtScan;

public interface IFoundPosition
{
    long StartPosition { get; }
}

public interface IFoundRange : IFoundPosition
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
    void Detect(BinarySource src, ref object? res);
}