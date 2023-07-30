using BinFmtScan;
using BinFmtScan.Formats.Images;

var path = Environment.GetCommandLineArgs()[1];
var out_dir = path + " extracted";

using (var stream = File.OpenRead(path))
{
    var src = new BinarySource(stream);
    Scan(src);
}

return;

void Scan(BinarySource src)
{
    while (src.Position < src.Length)
    {
        var pos = src.Position;
        FoundInfo? fmt = null;

        IDetector? det = null;
        FindAnyFormat(src, ref fmt, ref det);

        if (det != null && fmt != null && fmt.HasSize)
        {
            Directory.CreateDirectory(out_dir);
            var fn = Path.Combine(out_dir, $"{fmt.StartPosition:X}{det.Extension}");
            src.Dump(fmt.StartPosition, fmt.Size, fn);
        }

        src.Position = fmt != null && fmt.HasSize ? pos + fmt.Size : pos + 1;
    }
}

static void FindAnyFormat(BinarySource src, ref FoundInfo? fmt, ref IDetector? detector)
{
    foreach (var handler in FormatList.All)
    {
        try
        {
            handler.Detect(src, ref fmt);
        }
        catch
        {
        }

        if (fmt != null)
        {
            // Format found.
            detector = handler;

            Console.Write($"0x{fmt.StartPosition:X}");
            if (fmt.HasSize)
                Console.Write($"..0x{(fmt.StartPosition + fmt.Size):X}");
            Console.Write($": {handler.ID}");
            Console.WriteLine();

            break;
        }
    }
}