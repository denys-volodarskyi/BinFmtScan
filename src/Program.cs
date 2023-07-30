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

        foreach (var handler in FormatList.All)
        {
            try
            {
                handler.Detect(src, ref found);
            }
            catch
            {
            }

            if (found != null)
            {
                // Format found.

                if (found is not IFoundPoint pt)
                    throw new NotImplementedException();

                Console.Write($"0x{pt.StartPosition:X}");

                if (found is IFoundRange range)
                    Console.Write($"..0x{range.End:X}");

                Console.Write($": {handler.ID}");

                if (found is ITextPreview textPreview)
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
              -x: extract
            """);
    }
}