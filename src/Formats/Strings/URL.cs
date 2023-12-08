using System.Text;

namespace BinFmtScan.Formats.Strings;

internal class URL : IDetector
{
    public void Detect(BinarySource src, ref object? res)
    {
        var start = src.Position;
        var enc = Encoding.ASCII;

        if (src.Is("http://", enc) ||
            src.Is("https://", enc) ||
            src.Is("ftp://", enc) ||
            src.Is("ftps://", enc))
        {
            var str = src.GetString(enc, peek: false);
            res = new FoundURL
            {
                StartPosition = start,
                Size = src.Position - start,
                Text = str,
            };
        }
    }
}

file class FoundURL : IFoundRange, ITextPreview, IHasFileExtension
{
    public long StartPosition { get; set; }
    public long Size { get; set; }

    public string Text { get; set; } = string.Empty;

    public string Extension => ".txt";
}