namespace BinFmtScan.Formats.Executable;

internal class PE : IDetector
{
    public void Detect(BinarySource src, ref object? res)
    {
        if (!src.Is("MZ"u8))
            return;

        var start = src.Position;

        src.Position += 0x3C;
        var e_lfanew = src.ReadLE<uint>();

        var newpos = start + e_lfanew;
        if (newpos <= start || newpos >= src.Length)
        {
            // Invalid offset.
            return;
        }

        src.Position = newpos;

        // Is it Portable Executable?
        if (!src.Is("PE\0\0"u8))
            return;

        src.Position += 4;

        src.ReadLE<ushort>(); // machine
        var number_of_sections = src.ReadLE<ushort>();
        src.Position += 0xC;
        var size_of_optional_header = src.ReadLE<ushort>();
        var optional_header_pos = src.Position + 2;
        var section_header_pos = optional_header_pos + size_of_optional_header;

        // Optional Header.
        src.Position = optional_header_pos;
        var magic = src.ReadLE<ushort>();

        long data_dir_pos;

        if (magic == 0x20B)
        {
            // PE64
            data_dir_pos = optional_header_pos + 0x70;
        }
        else
        {
            // PE32
            // Not implemented yet.
            return;
        }

        // Find section max raw position.

        uint max_raw_end = 0;

        for (var n = 0; n < number_of_sections; n++)
        {
            src.Position = section_header_pos + 40 * n;

            src.Position += 0x10;
            var raw_size = src.ReadLE<uint>();
            var raw_ofs = src.ReadLE<uint>();

            if (raw_size != 0)
            {
                var raw_end = raw_ofs + raw_size;
                if (raw_end > max_raw_end)
                    max_raw_end = raw_end;
            }
        }

        var size = max_raw_end;

        // Check security certificate data directory.
        src.Position = data_dir_pos + 0x20;
        var sec_dir_pos = src.ReadLE<uint>();
        var sec_dir_size = src.ReadLE<uint>();
        if (sec_dir_size != 0)
        {
            var sec_dir_end = sec_dir_pos + sec_dir_size;
            if (sec_dir_end > size)
                size = sec_dir_end;
        }

        if (size == 0)
            return;

        res = new PERange
        {
            StartPosition = start,
            Size = size,
        };
    }
}

class PERange : IFoundRange
{
    public long StartPosition { get; init; }
    public long Size { get; init; }
}