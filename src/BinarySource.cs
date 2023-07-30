using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BinFmtScan;

public unsafe sealed class BinarySource
{
    private readonly Stream Stream;
    private readonly long Offset;
    public readonly long Length;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="offset"></param>
    /// <param name="length">0 to use all available space after <see cref="offset"/>.</param>
    public BinarySource(Stream stream, long offset = 0, long length = 0)
    {
        if (length == 0)
            length = stream.Length - offset;

        this.Stream = stream;
        this.Offset = offset;
        this.Length = length;
    }

    #region Positioning
    public long Position
    {
        get => Stream.Position;
        set => Stream.Position = value;
    }

    private readonly Stack<long> PositionStack = new();
    #endregion

    public void Dump(long start, long size, string path)
    {
        using var dst = File.Create(path);
        Dump(start, size, dst);
    }

    private byte[]? DumpWindow;

    public void Dump(long start, long size, Stream dst)
    {
        DumpWindow ??= new byte[1024 * 16];

        var pos = Position;
        try
        {
            Position = start;

            var left = size;
            while (left > 0)
            {
                var cnt = (int)Math.Min(left, DumpWindow.Length);
                if (Stream.Read(DumpWindow, 0, cnt) != cnt)
                    throw new Exception($"Error reading data in {nameof(Dump)}");
                dst.Write(DumpWindow, 0, cnt);
                left -= cnt;
            }
        }
        finally
        {
            Position = pos;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadSpan(Span<byte> span)
    {
        var read = Stream.Read(span);
        if (read != span.Length)
            throw new Exception($"Read {read} bytes insted of expected {span.Length} bytes.");
    }

    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetValue<T>(bool reverse = false, bool peek = false) where T : struct
    {
        var size = Unsafe.SizeOf<T>();
        Span<byte> data = stackalloc byte[size];
        var pos = Position;
        ReadSpan(data);
        if (peek) Position = pos;
        if (reverse) data.Reverse();
        return MemoryMarshal.Read<T>(data);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T ReadValue<T>(bool reverse = false) where T : struct => GetValue<T>(reverse, peek: false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T ReadLE<T>() where T : struct => GetValue<T>(peek: false, reverse: false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T ReadBE<T>() where T : struct => GetValue<T>(peek: false, reverse: true);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T PeekLE<T>() where T : struct => GetValue<T>(peek: true, reverse: false);
    [MethodImpl(MethodImplOptions.AggressiveInlining)] public T PeekBE<T>() where T : struct => GetValue<T>(peek: true, reverse: true);

    /// <summary>
    /// Test if there are specific bytes at current position.
    /// </summary>
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Is(ReadOnlySpan<byte> bytes, bool peek = true)
    {
        var pos = Position;

        Span<byte> tmp = stackalloc byte[bytes.Length];
        ReadSpan(tmp);

        var equal = bytes.SequenceEqual(tmp);

        if (peek)
            Position = pos;

        return equal;
    }
}