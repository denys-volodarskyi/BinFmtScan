using System.Globalization;
using System.Text;

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
            if (args[i] == "-x")
                extract = true;
            else if (args[i] == "-fmts")
                DisplayRegisteredFormats();
            else if (i == args.Length - 1)
                path = args[i];
            else
                Console.WriteLine($"Unknown switch: {args[i]}");
        }

        if (string.IsNullOrEmpty(path))
        {
            // No file was provided to scan.
            return;
        }

        if (!File.Exists(path))
        {
            // Given path does not exist.
            Console.WriteLine("Invalid source path");
            return;
        }

        var out_dir = path + " extracted";

        using var stream = File.OpenRead(path);
        var src = new BinarySource(stream);
        Scan(src, out_dir, extract);
    }

    private static void DisplayRegisteredFormats()
    {
        if (FormatList.All.Count == 0)
        {
            Console.WriteLine("There are no registered formats.");
            return;
        }

        Console.WriteLine("Registered formats:");

        foreach (var fmt in FormatList.All)
        {
            Console.WriteLine($"- {fmt.ID}");
        }
    }

    private static void Scan(BinarySource src, string out_dir, bool extract = false)
    {
        if (FormatList.All.Count == 0)
            return;

        int count = 0;

        while (src.Position < src.Length)
        {
            var pos = src.Position;

            FindAnyFormat(src, out object? found);

            if (found != null)
            {
                count++;

                if (found is IFoundRange range)
                {
                    if (extract)
                    {
                        var fn = Path.Combine(out_dir, $"{range.StartPosition:X}");

                        if (range is IHasFileExtension withext)
                            fn += withext.Extension;

                        Directory.CreateDirectory(out_dir);
                        src.Dump(range.StartPosition, range.Size, fn);
                    }
                }
            }

            if (found is IFoundRange r)
                src.Position = pos + r.Size;
            else
                src.Position = pos + 1;
        }


        if (count == 0)
            Console.WriteLine("Nothing found");
    }

    private static void FindAnyFormat(BinarySource src, out object? found)
    {
        found = null;

        foreach (var fmt in FormatList.All)
        {
            try
            {
                fmt.Detector.Detect(src, ref found);
            }
            catch
            {
            }

            if (found != null)
            {
                // Format found.

                if (found is not IFoundPosition foundpos)
                    throw new NotImplementedException();

                // Display position or range.

                Console.Write($"0x{foundpos.StartPosition:X}");

                if (found is IFoundRange range)
                {
                    Console.Write($"..0x{range.End:X}");
                }

                Console.Write($": {fmt.ID}");

                if (found is IComment comment)
                {
                    Console.Write("; " + comment.Comment);
                }
                else if (found is ITextPreview textPreview)
                {
                    Console.Write("; '" + ProcessPreviewText(textPreview.Text) + "'");
                }

                Console.WriteLine();

                break;
            }
        }
    }

    private static string ProcessPreviewText(string text)
    {
        var lim = 55;
        var sb = new StringBuilder(lim + 3);
        for (var i = 0; i < text.Length; i++)
        {
            if (i > lim)
            {
                sb.Append("...");
                break;
            }

            switch (char.GetUnicodeCategory(text[i]))
            {
                case UnicodeCategory.LineSeparator:
                    sb.Append(' ');
                    break;
                case UnicodeCategory.Control:
                case UnicodeCategory.Surrogate:
                    break;
                default:
                    sb.Append(text[i]);
                    break;
            }
        }
        return sb.ToString();
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
              -x:    extract
              -fmts: display registered formats
            """);
    }
}