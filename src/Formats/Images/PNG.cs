namespace BinFmtScan.Formats.Images;

internal class PNG : IDetector
{
    private static readonly byte[] Signature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public void Detect(BinarySource src, ref object? res)
    {
        var start = src.Position;

        if (!src.Is(Signature, peek: false))
            return;

        long size_of_all_chunks = 0;

        // Chunks
        while (true)
        {
            var chunk_size = src.ReadBE<int>();
            size_of_all_chunks += chunk_size;

            src.Position += 4 + chunk_size + 4; // skip name, data, crc

            if (chunk_size == 0)
                break;
        }

        if (size_of_all_chunks != 0)
        {
            // Return result if there is some chunk data, not just PNG header.
            res = new FoundPNG
            {
                StartPosition = start,
                Size = src.Position - start,
            };
        }
        else
        {
            // Header only.
            res = new PNGHeaderLocation
            {
                StartPosition = start,
                Comment = "PNG Header"
            };
        }
    }
}

file class PNGHeaderLocation : IFoundPosition, IComment
{
    public long StartPosition { get; init; }
    public string Comment { get; init; } = "";
}

file class FoundPNG : IFoundRange, IHasFileExtension
{
    public long StartPosition { get; set; }

    public long Size { get; set; }

    public string Extension => ".png";
}