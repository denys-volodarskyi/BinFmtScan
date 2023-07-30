using BinFmtScan;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }


        bool extract = false;
        string path = "";

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-e":
                    extract = true;
                    break;
            }

            if (i == args.Length - 1)
                path = args[i];
            else if (args[i] == "-x")
                extract = true;
            else
                Console.WriteLine($"Unknown switch: {args[i]}");
        }


        if (string.IsNullOrEmpty(path) | !File.Exists(path))
        {
            Console.WriteLine("Invalid source path");
            return;
        }

        var out_dir = path + " extracted";

        using var stream = File.OpenRead(path);
        var src = new BinarySource(stream);
        Scan(src, out_dir, extract);
    }

    private static void Scan(BinarySource src, string out_dir, bool extract = false)
    {
        int count = 0;

        while (src.Position < src.Length)
        {
            var pos = src.Position;
            FoundInfo? fmt = null;

            IDetector? det = null;
            FindAnyFormat(src, ref fmt, ref det);

            if (fmt != null)
            {
                count++;

                if (det != null && fmt.HasSize)
                {
                    if (extract)
                    {
                        Directory.CreateDirectory(out_dir);
                        var fn = Path.Combine(out_dir, $"{fmt.StartPosition:X}{det.Extension}");
                        src.Dump(fmt.StartPosition, fmt.Size, fn);
                    }
                }
            }

            src.Position = fmt != null && fmt.HasSize ? pos + fmt.Size : pos + 1;
        }


        if (count == 0)
            Console.WriteLine("Nothing found");
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

    private static void ShowUsage()
    {
        var fg = Console.ForegroundColor;

        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("Binary Format Scanner");
        Console.ForegroundColor = fg;

        Console.WriteLine(" by Denys Volodarskyi");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("https://github.com/denys-volodarskyi/BinFmtScan");
        Console.ForegroundColor = fg;

        Console.WriteLine();
        Console.WriteLine(
            $"""
            Usage: {AppDomain.CurrentDomain.FriendlyName} [options] filename

            Options:
              -x: extract
            """);
    }
}