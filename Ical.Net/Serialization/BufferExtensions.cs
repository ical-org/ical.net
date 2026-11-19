//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ical.Net.Serialization;

internal static class BufferExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Write<T>(this IBufferWriter<T> writer, T value)
    {
        var span = writer.GetSpan(1);
        span[0] = value;
        writer.Advance(1);
    }


#if NETSTANDARD2_0

    /// <summary>
    /// Encodes the specified chars to bytes using the encoding
    /// and writes the result to <paramref name="writer"/>.
    /// </summary>
    /// <returns>The number of bytes written to <paramref name="writer"/></returns>
    public static long GetBytes(this Encoding encoding, ReadOnlySpan<char> chars, IBufferWriter<byte> writer)
    {
        if (chars.Length == 0)
        {
            return 0;
        }

        // Since GetByteCount does not have a span overload,
        // use max byte count instead of using unsafe. This
        // means larger buffers than needed though.
        var byteCount = encoding.GetMaxByteCount(chars.Length);

        // There is no overload for encoding span to span in netstandard2.0,
        // so rent an array and copy.
        var rentedBytes = ArrayPool<byte>.Shared.Rent(byteCount);

        // Instead of duplicating code for arrays or using unsafe to
        // get an array from a Span, rent and copy to create an array.
        var rentedChars = ArrayPool<char>.Shared.Rent(chars.Length);

        try
        {
            chars.CopyTo(rentedChars);

            // Encode string to rented array
            var bytesWritten = encoding.GetBytes(rentedChars, 0, chars.Length, rentedBytes, 0);
            var valueSpan = rentedBytes.AsSpan(0, bytesWritten);

            // Write bytes to writer
            writer.Write(valueSpan);

            return bytesWritten;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rentedBytes);
            ArrayPool<char>.Shared.Return(rentedChars);
        }
    }

    public static Task WriteAsync(
        this Stream stream,
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        if (MemoryMarshal.TryGetArray(buffer, out ArraySegment<byte> array))
        {
            return stream.WriteAsync(array.Array!, array.Offset, array.Count, cancellationToken);
        }

        byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);

        buffer.Span.CopyTo(sharedBuffer);
        return FinishWriteAsync(
            stream.WriteAsync(sharedBuffer, 0, buffer.Length, cancellationToken),
            sharedBuffer);
    }

    private static async Task FinishWriteAsync(Task writeTask, byte[] localBuffer)
    {
        try
        {
            await writeTask.ConfigureAwait(false);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(localBuffer);
        }
    }

    public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
    {
        byte[] sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try
        {
            buffer.CopyTo(sharedBuffer);
            stream.Write(sharedBuffer, 0, buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(sharedBuffer);
        }
    }
#endif
}
