using BinFmtScan;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
            return;

        var path = args[0];
        if (string.IsNullOrEmpty(path) | !File.Exists(path))
            return;

        var out_dir = path + " extracted";

        using var stream = File.OpenRead(path);
        var src = new BinarySource(stream);
        Scan(src, out_dir);
    }

    private static void FindAnyFormat(BinarySource src, ref FoundInfo? fmt, ref IDetector? detector)
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
                    Console.Write($"..0x{fmt.StartPosition + fmt.Size:X}");
                Console.Write($": {handler.ID}");
                Console.WriteLine();

                break;
            }
        }
    }

    private static void Scan(BinarySource src, string out_dir, bool dump = false)
    {
        while (src.Position < src.Length)
        {
            var pos = src.Position;
            FoundInfo? fmt = null;

            IDetector? det = null;
            FindAnyFormat(src, ref fmt, ref det);

            if (dump)
            {
                if (det != null && fmt != null && fmt.HasSize)
                {
                    Directory.CreateDirectory(out_dir);
                    var fn = Path.Combine(out_dir, $"{fmt.StartPosition:X}{det.Extension}");
                    src.Dump(fmt.StartPosition, fmt.Size, fn);
                }
            }

            src.Position = fmt != null && fmt.HasSize ? pos + fmt.Size : pos + 1;
        }
    }
}