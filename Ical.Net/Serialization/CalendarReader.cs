//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ical.Net.DataTypes;

namespace Ical.Net.Serialization;


#if NETSTANDARD
internal static class CalendarReaderExtensions
{
    public static string GetString(this Encoding encoding, ReadOnlySpan<byte> bytes)
        => encoding.GetString(bytes.ToArray());
}
#endif

public sealed class CalendarReader
{
    private byte[] _buffer = new byte[256];
    private int _bufferStart = 0;
    private int _bufferLength = 0;

    private Memory<byte> _contentLine;

    private long _lineNumber = 0;

    /// <summary>
    /// The current line number.
    /// </summary>
    public long LineNumber => _lineNumber;

    private static readonly Encoding _encoding = Encoding.UTF8;

    private readonly Stream _input;

    private bool _checkForBom = true;
    private static ReadOnlySpan<byte> Utf8Bom => [0xEF, 0xBB, 0xBF];

    internal CalendarReader(Stream input)
    {
        _input = input;
    }

    internal bool ReadContentLine()
    {
        var endOfStream = false;

        while (!TryGetContentLine(endOfStream, out _contentLine))
        {
            if (endOfStream)
            {
                return false;
            }

            GrowBufferIfNeeded();

            // Fill buffer
            var count = _buffer.Length - _bufferLength;
            var bytesRead = _input.Read(_buffer, _bufferStart + _bufferLength, count);

            if (bytesRead == 0)
            {
                endOfStream = true;
            }

            // Keep track of buffer data
            _bufferLength += bytesRead;
        }

        return !_contentLine.IsEmpty;
    }

    internal async Task<bool> ReadContentLineAsync(CancellationToken cancellationToken = default)
    {
        var endOfStream = false;

        while (!TryGetContentLine(endOfStream, out _contentLine))
        {
            if (endOfStream)
            {
                return false;
            }

            GrowBufferIfNeeded();

            // Fill buffer
            var offset = _bufferStart + _bufferLength;

#if NET8_0_OR_GREATER
            var bytesRead = await _input
                .ReadAsync(_buffer.AsMemory(offset), cancellationToken);
#else
            var bytesRead = await _input
                .ReadAsync(_buffer, offset, _buffer.Length - offset, cancellationToken)
                .ConfigureAwait(false);
#endif

            if (bytesRead == 0)
            {
                endOfStream = true;
            }

            // Keep track of buffer data
            _bufferLength += bytesRead;
        }

        return !_contentLine.IsEmpty;
    }

#if NET8_0_OR_GREATER
    private static readonly SearchValues<byte> _newLineBytes = SearchValues.Create("\n\r"u8);
#else
    private static readonly byte[] _newLineBytes = [(byte)'\n', (byte)'\r'];
#endif

    /// <summary>
    /// Finds a full content line. Includes unfolding.
    /// </summary>
    private bool TryGetContentLine(bool endOfStream, out Memory<byte> contentLine)
    {
        // Skip UTF-8 BOM if present
        if (_checkForBom && _bufferLength >= Utf8Bom.Length)
        {
            if (_buffer.AsSpan(_bufferStart, Utf8Bom.Length).StartsWith(Utf8Bom))
            {
                _bufferStart += Utf8Bom.Length;
                _bufferLength -= Utf8Bom.Length;
            }

            _checkForBom = false;
        }

        while (true)
        {
            var bufferedBytes = _buffer.AsSpan(_bufferStart, _bufferLength);

            var endOfLineIndex = IndexOfEndOfContentLine(bufferedBytes);

            // Skip empty lines
            if (endOfLineIndex == 0)
            {
                _bufferStart += 1;
                _bufferLength -= 1;
                continue;
            }

            // Do not require a newline at the end of the stream
            if (endOfLineIndex == -1 && endOfStream)
            {
                endOfLineIndex = _bufferLength;
            }

            // Stop if buffer does not contain an entire content line
            if (endOfLineIndex == -1)
            {
                contentLine = default;
                return false;
            }

            var unfoldedLength = UnfoldInPlace(bufferedBytes, endOfStream, out var lineCount);

            _lineNumber += lineCount;

            // Set content line
            contentLine = _buffer.AsMemory(_bufferStart, unfoldedLength);

            // Consume content line bytes. This uses the original folded
            // byte count because real data continues after that, not
            // after the unfolded byte count.
            _bufferStart += endOfLineIndex;
            _bufferLength -= endOfLineIndex;

            return true;
        }
    }

    /// <summary>
    /// Shifts data to the start of the buffer or doubles buffer size if needed.
    /// </summary>
    private void GrowBufferIfNeeded()
    {
        // Shift data to start of buffer to make room
        if (_bufferStart > 0)
        {
            // Shift data if there is any. This should occur after reading to the
            // end of the buffer without reaching the end of a content line.
            if (_bufferLength > 0)
            {
                Array.Copy(_buffer, _bufferStart, _buffer, 0, _buffer.Length - _bufferStart);
            }

            // Start writing at the start of the buffer
            _bufferStart = 0;
        }

        var bufferIsFull = _buffer.Length == _bufferLength;
        if (bufferIsFull)
        {
            Array.Resize(ref _buffer, _buffer.Length * 2);
        }
    }

    private static int IndexOfEndOfContentLine(ReadOnlySpan<byte> buffer)
    {
        var foldStart = 0;

        while (true)
        {
            var relativeEnd = buffer.Slice(foldStart).IndexOfAny(_newLineBytes);

            if (relativeEnd == -1)
            {
                // There is no new line, so no end of content line
                return -1;
            }

            var foldResult = StartsWithLineFold(buffer.Slice(foldStart + relativeEnd), out var foldLength);

            if (foldResult == LineFoldResult.EndOfContentLine)
            {
                return foldStart + relativeEnd;
            }

            if (foldResult == LineFoldResult.NeedsMoreData)
            {
                // If buffer ends on a new line, more data is needed
                // to know if the next byte will indicate a fold or not.
                return -1;
            }

            // Line is folded, continue searching
            foldStart += relativeEnd + foldLength;
        }
    }

    /// <summary>
    /// Unfolds the content line within the buffer.
    /// </summary>
    /// <param name="buffer">A buffer containing an entire content line.</param>
    /// <param name="lineCount">The number of lines before unfolding.</param>
    /// <returns>The length of the unfolded content line.</returns>
    private static int UnfoldInPlace(Span<byte> buffer, bool endOfStream, out int lineCount)
    {
        var foldStart = 0;
        var lineEnd = 0;

        lineCount = 0;

        while (true)
        {
            var relativeEnd = buffer.Slice(foldStart).IndexOfAny(_newLineBytes);

            if (relativeEnd == -1)
            {
                return buffer.Length;
            }

            var foldEnd = foldStart + relativeEnd;

            if (foldStart == 0)
            {
                lineEnd = foldEnd;
            }

            var foldResult = StartsWithLineFold(buffer.Slice(foldEnd), out var foldLength);

            if (foldResult == LineFoldResult.NeedsMoreData)
            {
                // This should only happen at the end of the stream
                // because the buffer should always have an entire
                // content line.
                if (!endOfStream)
                {
                    throw new SerializationException("Unexpected folded content line");
                }

                // At end of stream with no more content,
                // line must be unfolded.
                foldResult = LineFoldResult.EndOfContentLine;
            }

            if (foldStart > 0)
            {
                // Shift folded line backward to "remove" the fold
                var foldedLine = buffer.Slice(foldStart, relativeEnd);
                foldedLine.CopyTo(buffer.Slice(lineEnd));

                // Keep track of end of line
                lineEnd += foldedLine.Length;

                // Keep track of line number
                lineCount++;
            }

            if (foldResult == LineFoldResult.EndOfContentLine)
            {
                // Keep track of line number
                lineCount++;

                // At end of line
                return lineEnd;
            }

            foldStart = foldEnd + foldLength;
        }
    }

    private enum LineFoldResult
    {
        EndOfContentLine,
        Folded,
        NeedsMoreData,
    }

    /// <summary>
    /// Determines if the byte sequence is a line fold or if there
    /// is not enough data to know for sure.
    /// </summary>
    /// <param name="endOfLine">The start of a line ending.</param>
    /// <param name="foldLength">The number of bytes that are part of the fold.</param>
    private static LineFoldResult StartsWithLineFold(
        ReadOnlySpan<byte> endOfLine,
        out int foldLength)
    {
        if (endOfLine.Length < 2)
        {
            foldLength = 0;
            return LineFoldResult.NeedsMoreData;
        }

        Debug.Assert(endOfLine[0] is (byte)'\r' or (byte)'\n');

        int whiteSpaceIndex;
        if (endOfLine[1] == (byte)'\n')
        {
            // Needs another character after the line break
            if (endOfLine.Length < 3)
            {
                foldLength = 0;
                return LineFoldResult.NeedsMoreData;
            }

            // Line break is \r\n
            whiteSpaceIndex = 2;
        }
        else
        {
            // Line break is just \r or \n
            whiteSpaceIndex = 1;
        }

        var isFold = endOfLine[whiteSpaceIndex] is (byte) ' ' or (byte) '\t';
        if (isFold)
        {
            foldLength = 1 + whiteSpaceIndex;
            return LineFoldResult.Folded;
        }

        foldLength = 0;
        return LineFoldResult.EndOfContentLine;
    }

#if NET8_0_OR_GREATER
    private static readonly SearchValues<byte> _nameSeparator = SearchValues.Create(":;"u8);
#else
    private static readonly byte[] _nameSeparator = [(byte)';', (byte)':'];
#endif

    internal string ReadName()
    {
        var lineBytes = _contentLine.Span;
        var idx = lineBytes.IndexOfAny(_nameSeparator);

        if (idx == -1)
        {
            throw new SerializationException("Property name missing");
        }

        var name = _encoding.GetString(lineBytes.Slice(0, idx));

        AdvancePos(idx);

        return name.ToUpperInvariant();
    }

    private const byte ParameterNameSeparator = (byte) '=';

    internal bool TryReadParameterName(
#if NET
        [NotNullWhen(true)]
#endif
        out string? name)
    {
        var lineBytes = _contentLine.Span;

        if (lineBytes.Length == 0 || lineBytes[0] != ';')
        {
            name = null;
            return false;
        }

        lineBytes = lineBytes.Slice(1);

        var idx = lineBytes.IndexOf(ParameterNameSeparator);
        if (idx == -1)
        {
            name = null;
            return false;
        }

        name = _encoding.GetString(lineBytes.Slice(0, idx));

        // Add 1 for initial prefix byte
        AdvancePos(1 + idx);

        return true;
    }

    internal bool TryReadParameterValue(
#if NET
        [NotNullWhen(true)]
#endif
        out string? value)
    {
        var lineBytes = _contentLine.Span;

        if (lineBytes.Length == 0 || (lineBytes[0] != '=' && lineBytes[0] != ','))
        {
            value = null;
            return false;
        }

        lineBytes = lineBytes.Slice(1);

        var start = 0;
        int idx;

        // Check if value is quoted
        if (lineBytes.Length > 0 && lineBytes[0] == '"')
        {
            start = 1;
            idx = lineBytes.Slice(1).IndexOf((byte) '"');

            if (idx == -1)
            {
                throw new SerializationException($"Unbalanced quotes when reading parameter value at line {LineNumber}");
            }
        }
        else
        {
            idx = lineBytes.IndexOfAny(_nameSeparator);

            if (idx == -1)
            {
                value = null;
                return false;
            }
        }

        value = _encoding.GetString(lineBytes.Slice(start, idx));

        // Add 1 for initial prefix byte
        AdvancePos(1 + (start * 2) + idx);

        return true;
    }

    internal void ReadNextValueAsBase64()
    {
        GetRawValueBytes(out var valueBytes);

        if (valueBytes.Length == 0)
        {
            return;
        }

        var result = Base64.DecodeFromUtf8InPlace(valueBytes, out var written);
        if (result != OperationStatus.Done)
        {
            throw new SerializationException($"Invalid BASE64 encoded data at line {LineNumber}");
        }

        // Update length of the value, including value separator ':'
        _contentLine = _contentLine.Slice(0, 1 + written);
    }

    public byte[] ReadBinaryValue()
    {
        GetRawValueBytes(out var lineBytes);

        // Entire line is used
        _contentLine = Memory<byte>.Empty;

        return lineBytes.ToArray();
    }

    public bool TryGetBoolean(out bool result)
    {
        GetRawValueBytes(out var valueBytes);

        var strValue = _encoding.GetString(valueBytes);

        if (strValue.Equals("TRUE", StringComparison.OrdinalIgnoreCase))
        {
            result = true;
            AdvancePos(1 + valueBytes.Length);
            return true;
        }

        if (strValue.Equals("FALSE", StringComparison.OrdinalIgnoreCase))
        {
            result = false;
            AdvancePos(1 + valueBytes.Length);
            return true;
        }

        result = default;
        return false;
    }

    public bool TryGetInteger(out int result)
    {
        GetRawValueBytes(out var valueBytes);

        if (!Utf8Parser.TryParse(valueBytes, out result, out var bytesConsumed))
        {
            result = default;
            return false;
        }

        AdvancePos(1 + bytesConsumed);

        return true;
    }

    private void AdvancePos(int length) => _contentLine = _contentLine.Slice(length);

    public bool TryGetUtcOffset(
#if NET
        [NotNullWhen(true)]
#endif
        out UtcOffset? offset)
    {
        GetRawValueBytes(out var valueBytes);

        var strValue = _encoding.GetString(valueBytes);

        if (!UtcOffset.TryParse(strValue, out offset))
        {
            offset = default;
            return false;
        }

        AdvancePos(1 + valueBytes.Length);

        return true;
    }

    public bool TryGetUri(
#if NET
        [NotNullWhen(true)]
#endif
        out Uri? uri)
    {
        GetRawValueBytes(out var valueBytes);

        var strValue = _encoding.GetString(valueBytes);

        if (!Uri.TryCreate(strValue, UriKind.RelativeOrAbsolute, out uri))
        {
            return false;
        }

        AdvancePos(1 + valueBytes.Length);

        return true;
    }

    public bool TryGetDateTime(
        string? tzId,
#if NET
        [NotNullWhen(true)]
#endif
        out CalDateTime? dateTime)
    {
        if (!TryGetValueBytes(out var valueBytes))
        {
            dateTime = default;
            return false;
        }

        var strValue = _encoding.GetString(valueBytes);

        if (!CalDateTime.TryParse(strValue, tzId, out dateTime))
        {
            return false;
        }

        AdvancePos(1 + valueBytes.Length);

        return true;
    }

    public bool TryGetPeriod(
        string? tzId,
#if NET
        [NotNullWhen(true)]
#endif
        out Period? period)
    {
        if (!TryGetValueBytes(out var valueBytes))
        {
            period = default;
            return false;
        }

        var strValue = _encoding.GetString(valueBytes);

        if (!Period.TryParse(strValue, tzId, out period))
        {
            return false;
        }

        AdvancePos(1 + valueBytes.Length);

        return true;
    }

    public bool TryGetDuration(out Duration duration)
    {
        if (!TryGetValueBytes(out var valueBytes))
        {
            duration = default;
            return false;
        }

        // TODO: Reduce allocation for fixed-size values
        //Span<char> buffer = stackalloc char[256];

        var strValue = _encoding.GetString(valueBytes);

        if (!Duration.TryParse(strValue, out duration))
        {
            return false;
        }

        AdvancePos(1 + valueBytes.Length);

        return true;
    }

    /// <summary>
    /// Finds the next value in the current line,
    /// handling comma-separated values. Callers must
    /// advance reader position if bytes are used.
    /// </summary>
    /// <returns>A span of the next value.</returns>
    private bool TryGetValueBytes(out ReadOnlySpan<byte> value)
    {
        var lineBytes = _contentLine.Span;

        if (lineBytes.Length == 0 || (lineBytes[0] != ':' && lineBytes[0] != ','))
        {
            value = default;
            return false;
        }

        lineBytes = lineBytes.Slice(1);

        var endIndex = lineBytes.IndexOf((byte)',');

        if (endIndex == -1)
        {
            value = lineBytes;
            return true;
        }

        value = lineBytes.Slice(0, endIndex);
        return true;
    }

    private void GetRawValueBytes(out Span<byte> value)
    {
        var lineBytes = _contentLine.Span;

        if (lineBytes.Length == 0 || lineBytes[0] != ':')
        {
            value = default;
            return;
        }

        value = lineBytes.Slice(1);
    }

    /// <summary>
    /// Returns the complete value as a string, ignoring any escaping or list delimiters.
    /// </summary>
    public string GetRawStringValue()
    {
        GetRawValueBytes(out var valueBytes);
        return _encoding.GetString(valueBytes);
    }

    public string GetTextValue()
    {
        if (!TryGetTextValue(out var textValue))
        {
            return string.Empty;
        }

        return textValue;
    }

    /// <summary>
    /// Reads a single TEXT value from a comma-separated list of TEXT values.
    /// Call multiple times to get all values.
    /// </summary>
    /// <param name="value">The unescaped text value.</param>
    /// <returns>True if there is a value to get.</returns>
    public bool TryGetTextValue(
#if NET
        [NotNullWhen(true)]
#endif
        out string? value) => TryReadTextValueCore((byte) ',', out value);

    /// <summary>
    /// Reads a single value from a "structured" text value
    /// delimited by a semicolon. Call multiple times to get all values.
    /// </summary>
    /// <param name="value">The unescaped text segment.</param>
    /// <returns>True if there is a value to get.</returns>
    public bool TryGetStructuredTextValue(
#if NET
        [NotNullWhen(true)]
#endif
        out string? value) => TryReadTextValueCore((byte) ';', out value);

    private bool TryReadTextValueCore(
        byte delimiter,
#if NET
        [NotNullWhen(true)]
#endif
        out string? textValue)
    {
        var lineBytes = _contentLine.Span;

        if (lineBytes.Length == 0 || (lineBytes[0] != ':' && lineBytes[0] != delimiter))
        {
            textValue = null;
            return false;
        }

        lineBytes = lineBytes.Slice(1);

        // Get unescaped bytes. The unescaped length must be smaller
        // than the original length, but the byte length used could be
        // much bigger than needed because it could be multiple values.
        var rentedBytes = ArrayPool<byte>.Shared.Rent(lineBytes.Length);
        UnescapeTextValue(lineBytes, rentedBytes, delimiter, out var writtenBytes, out var readBytes);

#if NET8_0_OR_GREATER
        var unescapedBytes = rentedBytes.AsSpan(0, writtenBytes);
        textValue = _encoding.GetString(unescapedBytes);
#else
        textValue = _encoding.GetString(rentedBytes, 0, writtenBytes);
#endif

        ArrayPool<byte>.Shared.Return(rentedBytes);

        // Advance reader. Add 1 for prefix ':' or delimiter.
        AdvancePos(1 + readBytes);

        return true;
    }

    #region Text unescape

#if NET8_0_OR_GREATER
    private static readonly SearchValues<byte> _escapeOrValueSeparator = SearchValues.Create("\\,"u8);
    private static readonly SearchValues<byte> _escapeOrStructureSeparator = SearchValues.Create("\\;"u8);
#else
    private static readonly byte[] _escapeOrValueSeparator = [(byte)'\\', (byte)','];
    private static readonly byte[] _escapeOrStructureSeparator = [(byte)'\\', (byte)';'];
#endif

    /// <summary>
    /// Unescapes the first value from the source. Stops at the
    /// first unescaped comma.
    /// </summary>
    internal static void UnescapeTextValue(
        ReadOnlySpan<byte> source,
        Span<byte> destination,
        byte delimiter,
        out int written,
        out int idx)
    {
        Debug.Assert(destination.Length >= source.Length);

        var searchValues = delimiter == (byte)';'
            ? _escapeOrStructureSeparator : _escapeOrValueSeparator;

        idx = 0;
        written = 0;

        while (true)
        {
            var remaining = source.Slice(idx);

            var nextUnescapedLength = remaining.IndexOfAny(searchValues);
            if (nextUnescapedLength == -1)
            {
                nextUnescapedLength = remaining.Length;
            }

            // Write unescaped bytes
            remaining.Slice(0, nextUnescapedLength).CopyTo(destination.Slice(written));
            written += nextUnescapedLength;
            idx += nextUnescapedLength;

            // If all source bytes are copied or the
            // delimiter has been reached, then stop.
            if (idx == source.Length || source[idx] == delimiter)
            {
                return;
            }

            Debug.Assert(source[idx] == (byte) '\\');

            destination[written++] = source[++idx] switch
            {
                (byte) 'n' or (byte) 'N' => (byte) '\n',
                (byte) '\\' => (byte) '\\',
                (byte) ';' => (byte) ';',
                (byte) ',' => (byte) ',',

                // Double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
                (byte) '"' => (byte) '"',

                // Backslash is escaping an invalid character, just
                // include the backslash and leave it unchanged.
                _ => (byte) '\\',
            };

            // If all source bytes are copied or an
            // unescaped comma has been reached, then stop.
            if (++idx == source.Length || source[idx] == delimiter)
            {
                return;
            }
        }
    }
#endregion
}
