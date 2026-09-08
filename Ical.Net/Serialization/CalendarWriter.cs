//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;
using System.Buffers.Text;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Ical.Net.Serialization;

public sealed class CalendarWriter
{
    private const int LineByteLimit = 75;
    private const int LineByteLimitFolded = LineByteLimit - 1;
    private const byte NameValueSeparator = (byte) ':';
    private const byte CarriageReturn = (byte) '\r';
    private const byte LineFeed = (byte) '\n';
    private const byte Space = (byte) ' ';

    // Sized for the max line size including line break
    private readonly byte[] _buffer = new byte[LineByteLimit + 2];

    private char[] _charBuffer = new char[256];
    private int _charBufferLength = 0;

    private bool _hasWrittenParameterValue = false;
    private bool _hasWrittenValue = false;
    private bool _writeNextValueAsBase64 = false;
    private int _valueStartIndex = 0;

    private static readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    private readonly Stream _output;

    internal CalendarWriter(Stream output)
    {
        _output = output;
    }

    /// <summary>
    /// Writes a line, folding as needed.
    /// </summary>
    /// <param name="line"></param>
    private void WriteContentLine(char[] line, int lineLength)
    {
        // Write first line
        var start = 0;
        var charCount = FindCharCount(line, start, lineLength, LineByteLimit);
        var bytesWritten = _encoding.GetBytes(line, start, charCount, _buffer, 0);
        WriteLineEnd(bytesWritten);

        start = charCount;

        // Fold remaining
        while (start < lineLength)
        {
            // Prefix folded lines with a space
            _buffer[0] = Space;
            bytesWritten = 1;

            // Write next line
            charCount = FindCharCount(line, start, lineLength - start, LineByteLimitFolded);
            bytesWritten += _encoding.GetBytes(line, start, charCount, _buffer, 1);
            WriteLineEnd(bytesWritten);
            start += charCount;
        }
    }

    private static int FindCharCount(char[] line, int charIndex, int lineLength, int maxByteCount)
    {
        // Look no further than the max bytes. The longest char length would
        // be 1 char for every byte.
        var length = Math.Min(maxByteCount, lineLength);

        // Do not cut UTF-16 multibyte character in half
        if (length > 0 && char.IsHighSurrogate(line[charIndex + length - 1]))
        {
            length--;
        }

        var byteCount = _encoding.GetByteCount(line, charIndex, length);

        // Reduce line length by each grapheme cluster until
        // total byte count of the line does not exceed the limit
        while (byteCount > maxByteCount)
        {
            if (--length > 0 && char.IsLowSurrogate(line[charIndex + length]))
            {
                length--;
            }

            var lastCharByteCount = char.IsHighSurrogate(line[charIndex + length]) ? 2 : 1;
            byteCount -= _encoding.GetByteCount(line, length, lastCharByteCount);
        }

        // Do not cut UTF-16 multibyte character in half
        if (length > 0 && char.IsHighSurrogate(line[charIndex + length - 1]))
        {
            length--;
        }

        return length;
    }

    /// <summary>
    /// Writes a line without folder or escaping.
    /// This should only be used for constant values known to be safe.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="property"></param>
    internal void WriteRawContentLine(ReadOnlySpan<byte> name, string property)
    {
#if NET
        _output.Write(name);
#else
        name.CopyTo(_buffer);
        _output.Write(_buffer, 0, name.Length);
#endif

        _output.WriteByte(NameValueSeparator);

        var bytesWritten = _encoding.GetBytes(property, 0, property.Length, _buffer, 0);
        WriteLineEnd(bytesWritten);
    }

    internal void WriteName(string name)
    {
        if (name.IndexOfAny(_textEscapeChars) != -1)
        {
            throw new SerializationException("Property name contains invalid characters");
        }

        GrowBuffer(name.Length);

        name.CopyTo(0, _charBuffer, 0, name.Length);
        _charBufferLength = name.Length;

        _hasWrittenValue = false;
    }

    public void WriteParameter(string name, string value)
    {
        WriteParameterName(name);
        WriteParameterValue(value);
    }

    public void WriteParameterName(string name)
    {
        if (name.IndexOfAny(_textEscapeChars) != -1)
        {
            throw new SerializationException("Parameter name contains invalid characters");
        }

        GrowBuffer(name.Length + 1);

        _charBuffer[_charBufferLength++] = ';';

        name.CopyTo(0, _charBuffer, _charBufferLength, name.Length);
        _charBufferLength += name.Length;

        _hasWrittenParameterValue = false;
    }

#if NET8_0_OR_GREATER
    /// <summary>
    /// Characters that must be escaped for TEXT values. Note that \r is also
    /// included even though it is not specified in the RFC - it is treated as
    /// a \n value.
    /// </summary>
    private static readonly SearchValues<char> _parameterValueRequiresQuotes = SearchValues.Create(":;,");
#else
    private static readonly char[] _parameterValueRequiresQuotes = [':', ';', ','];
#endif

    public void WriteParameterValue(string value)
    {
        var wrapInQuotes = value.IndexOfAny(_parameterValueRequiresQuotes) != -1;

        // Value length plus delimiter
        var maxLength = value.Length + 1;

        if (wrapInQuotes)
        {
            maxLength += 2;
        }

        GrowBuffer(maxLength);

        if (_hasWrittenParameterValue)
        {
            _charBuffer[_charBufferLength++] = ',';
        }
        else
        {
            _hasWrittenParameterValue = true;
            _charBuffer[_charBufferLength++] = '=';
        }

        if (wrapInQuotes)
        {
            _charBuffer[_charBufferLength++] = '"';
        }

        value.CopyTo(0, _charBuffer, _charBufferLength, value.Length);
        _charBufferLength += value.Length;

        if (wrapInQuotes)
        {
            _charBuffer[_charBufferLength++] = '"';
        }
    }

    internal void WriteNextValueAsBase64() => _writeNextValueAsBase64 = true;

    // From .NET Convert source
    private static int GetMaxBase64CharLength(int byteLength) => (int) ((uint)(byteLength + 2) / 3 * 4);

    private void StartWriteValue(int charLength, char separator = ',')
    {
        // Plus 1 for separator character
        GrowBuffer(charLength + 1);

        // Write separator character. Use comma if this
        // is not the first value already.
        if (_hasWrittenValue)
        {
            _charBuffer[_charBufferLength++] = separator;
        }
        else
        {
            _hasWrittenValue = true;
            _charBuffer[_charBufferLength++] = (char) NameValueSeparator;

            // Keep track of value start position so that
            // value can be BASE64 encoded if needed
            _valueStartIndex = _charBufferLength;
        }
    }

    private void StartStructuredTextWrite(int charLength) => StartWriteValue(charLength, ';');

#if NET8_0_OR_GREATER
    public void WriteBinaryValue(ReadOnlySpan<byte> value)
    {
        var maxBase64Length = GetMaxBase64CharLength(value.Length);

        StartWriteValue(maxBase64Length);

        if (!Convert.TryToBase64Chars(value, _charBuffer.AsSpan(_charBufferLength), out var written))
        {
            throw new SerializationException("Failed to write value as base-64");
        }

        _charBufferLength += written;

        // Value was written directly as base-64, prevent encoding again
        _writeNextValueAsBase64 = false;
    }
#endif

    public void WriteBinaryValue(byte[] value) => WriteBinaryValue(value, 0, value.Length);

    public void WriteBinaryValue(byte[] value, int offset, int count)
    {
        var maxBase64Length = GetMaxBase64CharLength(count);

        StartWriteValue(maxBase64Length);

        try
        {
            var written = Convert.ToBase64CharArray(value, offset, count, _charBuffer, _charBufferLength);
            _charBufferLength += written;
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new SerializationException("Failed to write value as base-64");
        }

        // Value was written directly as base-64, prevent encoding again
        _writeNextValueAsBase64 = false;
    }

    public void WriteValue(int value)
    {
        var strValue = Convert.ToString(value, CultureInfo.InvariantCulture);

        // There is no need to escape an int value
        WriteValueRaw(strValue);
    }

    public void WriteValue(Uri uri)
        // TODO: Use Uri.TryFormat to write directly to char buffer
        => WriteValueRaw(uri.OriginalString);

    public void WriteValue(string value) => WriteValueEscape(value);

    /// <summary>
    /// Writes text segment. Repeated calls delimit the text
    /// value with a semicolon.
    /// </summary>
    public void WriteStructuredTextSegment(string value)
    {
        // Max size is every value escaped
        var maxEscapedLength = value.Length * 2;

        StartStructuredTextWrite(maxEscapedLength);

        var written = Escape(value, _charBuffer.AsSpan(_charBufferLength));
        _charBufferLength += written;
    }

    private void WriteValueEscape(string value)
    {
        // Max size is every value escaped
        var maxEscapedLength = value.Length * 2;

        StartWriteValue(maxEscapedLength);

        var written = Escape(value, _charBuffer.AsSpan(_charBufferLength));
        _charBufferLength += written;
    }

    internal void WriteValueRaw(ReadOnlySpan<char> value)
    {
        StartWriteValue(value.Length);

        value.CopyTo(_charBuffer.AsSpan(_charBufferLength));
        _charBufferLength += value.Length;
    }

    private static void EncodeToBase64InPlace(char[] buffer, int index, int length, out int base64Length)
    {
        var maxByteCount = _encoding.GetMaxByteCount(length);
        var maxBase64ByteCount = Base64.GetMaxEncodedToUtf8Length(maxByteCount);

        // Get buffer large enough for base64 too
        var rentedBytes = ArrayPool<byte>.Shared.Rent(maxBase64ByteCount);

        // Get bytes
        var utf8ByteLength = _encoding.GetBytes(buffer, index, length, rentedBytes, 0);

        // Convert to base64
        var result = Base64.EncodeToUtf8InPlace(rentedBytes.AsSpan(), utf8ByteLength, out var base64BytesWritten);
        if (result != OperationStatus.Done)
        {
            ArrayPool<byte>.Shared.Return(rentedBytes, true);
            throw new SerializationException($"Failed to encode value as BASE64 ({result})");
        }

        // Write bytes back to buffer
        base64Length = _encoding.GetChars(rentedBytes, 0, base64BytesWritten, buffer, index);

        ArrayPool<byte>.Shared.Return(rentedBytes, true);
    }

    private void EncodeValueToBase64()
    {
        var valueLength = _charBufferLength - _valueStartIndex;
        var maxBase64Length = GetMaxBase64CharLength(valueLength);

        // Increase buffer size to fit base64 encoded value
        GrowBuffer(maxBase64Length - valueLength);

        EncodeToBase64InPlace(_charBuffer, _valueStartIndex, valueLength, out var base64Length);

        // Adjust length to fit new value size
        _charBufferLength = _charBufferLength - valueLength + base64Length;
    }

    internal void EndLine()
    {
        if (_writeNextValueAsBase64)
        {
            EncodeValueToBase64();
        }

        // Write char buffer
        WriteContentLine(_charBuffer, _charBufferLength);

        _writeNextValueAsBase64 = false;
    }

    private void GrowBuffer(int additionalLength)
    {
        var maxLength = _charBufferLength + additionalLength;

        if (_charBuffer.Length > maxLength)
        {
            return;
        }

        Array.Resize(ref _charBuffer, maxLength * 2);
    }

    private void WriteLineEnd(int bytesWritten)
    {
        _buffer[bytesWritten++] = CarriageReturn;
        _buffer[bytesWritten++] = LineFeed;
        _output.Write(_buffer, 0, bytesWritten);
    }


    #region Text Escape

#if NET8_0_OR_GREATER
    /// <summary>
    /// Characters that must be escaped for TEXT values. Note that \r is also
    /// included even though it is not specified in the RFC - it is treated as
    /// a \n value.
    /// </summary>
    private static readonly SearchValues<char> _textEscapeChars = SearchValues.Create("\\;,\n\r");
#else
    private static readonly char[] _textEscapeChars = ['\\', ';', ',', '\n', '\r'];
#endif

    private static int Escape(ReadOnlySpan<char> value, Span<char> destination)
    {
        var idx = value.IndexOfAny(_textEscapeChars);
        if (idx == -1)
        {
            // Nothing to escape
            value.CopyTo(destination);
            return value.Length;
        }

        var chunk = value.Slice(0, idx);
        chunk.CopyTo(destination);

        var written = chunk.Length;

        value = value.Slice(idx);

        while (!value.IsEmpty)
        {
            // Escape the character
            destination[written++] = '\\';

            var toEscape = value[0];
            var consumed = 1;

            if (toEscape == '\r')
            {
                // Treat \r as \n
                destination[written++] = 'n';

                // Treat \r\n as \n
                if (value.Length > 1 && value[1] == '\n')
                {
                    consumed = 2;
                }
            }
            else if (toEscape == '\n')
            {
                destination[written++] = 'n';
            }
            else
            {
                destination[written++] = toEscape;
            }

            value = value.Slice(consumed);

            idx = value.IndexOfAny(_textEscapeChars);
            if (idx == -1)
            {
                value.CopyTo(destination.Slice(written));
                written += value.Length;
                break;
            }

            chunk = value.Slice(0, idx);
            chunk.CopyTo(destination.Slice(written));
            written += chunk.Length;

            value = value.Slice(idx);
        }

        return written;
    }

    #endregion
}
