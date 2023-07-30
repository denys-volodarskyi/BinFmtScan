namespace BinFmtScan;

internal static class FormatList
{
    internal static List<IDetector> All = new()
    {
        new Formats.Images.PNG(),
    };
}
