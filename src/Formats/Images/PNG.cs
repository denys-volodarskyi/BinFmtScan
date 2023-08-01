﻿namespace BinFmtScan.Formats.Images;

internal class PNG : IDetector
{
    public string ID => "PNG";

    private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    public void Detect(BinarySource src, ref object? res)
    {
        var start = src.Position;

        if (!src.Is(Signature, peek: false))
            return;

        // Chunks
        while (true)
        {
            var chunk_size = src.ReadBE<int>();
            src.Position += 4 + chunk_size + 4; // name, data, crc
            if (chunk_size == 0)
                break;
        }

        var end = src.Position;

        res = new FoundPNG
        {
            StartPosition = start,
            Size = end - start,
        };
    }
}

file class FoundPNG : IFoundRange, IHasFileExtension
{
    public long StartPosition { get; set; }

    public long Size { get; set; }

    public string Extension => ".png";
}