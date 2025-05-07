//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;
using System.Text;

namespace Ical.Net.Serialization;

/// <summary>
/// Provides encoding and decoding services for byte arrays and strings.
/// </summary>
internal class EncodingProvider : IEncodingProvider
{
    private readonly SerializationContext _mSerializationContext;

    /// <summary>
    /// Represents a method that encodes a byte array into a string.
    /// </summary>
    public delegate string EncoderDelegate(byte[] data);

    /// <summary>
    /// Represents a method that decodes a string into a byte array.
    /// </summary>
    public delegate byte[] DecoderDelegate(string value);

    /// <summary>
    /// Creates a new instance of the <see cref="EncodingProvider"/> class.
    /// </summary>
    /// <param name="ctx"></param>
    public EncodingProvider(SerializationContext ctx)
    {
        _mSerializationContext = ctx;
    }

    /// <summary>
    /// Decodes an 8-bit string into a byte array.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>A byte array of the decoded string.</returns>
    protected byte[] Decode8Bit(string value)
    {
        var utf8 = new UTF8Encoding();
        return utf8.GetBytes(value);
    }

    /// <summary>
    /// Decodes a base-64 encoded string into a byte array.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>A byte array of the decoded string.</returns>
    protected byte[] DecodeBase64(string value)
    {
        return Convert.FromBase64String(value);
    }

    /// <summary>
    /// Gets a decoder for the specified encoding.
    /// </summary>
    /// <param name="encoding"></param>
    /// <exception cref="SerializationException">Decoder not supported.</exception>
    protected virtual DecoderDelegate GetDecoderFor(string encoding)
    {
        return encoding.ToUpper() switch
        {
            "8BIT" => Decode8Bit,
            "BASE64" => DecodeBase64,
            _ => throw new SerializationException($"Encoding '{encoding}' is not supported.")
        };
    }

    /// <summary>
    /// Encodes a byte array into an 8-bit string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>An 8-bit string, if encoding is successful, else <see langword="null"/></returns>
    protected string Encode8Bit(byte[] data)
    {
        var utf8 = new UTF8Encoding();
        return utf8.GetString(data);
    }

    /// <summary>
    /// Encodes a byte array into a base-64 encoded string.
    /// </summary>
    /// <param name="data"></param>
    /// <returns>A base-64 encoded string.</returns>
    protected string EncodeBase64(byte[] data)
    {
        return Convert.ToBase64String(data);
    }

    /// <summary>
    /// Gets an encoder for the specified encoding.
    /// </summary>
    /// <param name="encoding"></param>
    /// <exception cref="SerializationException">Encoder not supported.</exception>
    protected virtual EncoderDelegate GetEncoderFor(string encoding)
    {
        return encoding.ToUpper() switch
        {
            "8BIT" => Encode8Bit,
            "BASE64" => EncodeBase64,
            _ => throw new SerializationException($"Encoding '{encoding}' is not supported.")
        };
    }

    /// <summary>
    /// Encodes a byte array into a string.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="data"></param>
    /// <returns>A string representation of <paramref name="data"/> using the specified <see paramref="encoding"/>, or <see langword="null"/> if encoding fails.</returns>
    public string Encode(string encoding, byte[] data)
    {
        var encoder = GetEncoderFor(encoding);
        return encoder.Invoke(data);
    }

    /// <summary>
    /// Encodes a string into an encoded string by using the <see cref="EncodingStack"/> service.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="value"></param>
    /// <returns>A string representation of <paramref name="value"/> using the specified <see paramref="encoding"/>.</returns>
    /// <exception cref="FormatException">Base64 string is invalid.</exception>
    public string DecodeString(string encoding, string value)
    {
        var data = DecodeData(encoding, value);

        // Decode the string into the current encoding
        var encodingStack = (EncodingStack) _mSerializationContext.GetService(typeof(EncodingStack));
        return encodingStack.Current.GetString(data);
    }

    /// <summary>
    /// Decodes an encoded string into a byte array.
    /// </summary>
    /// <param name="encoding"></param>
    /// <param name="value"></param>
    /// <returns>A string representation of <paramref name="value"/> using the specified <see paramref="encoding"/>, or <see langword="null"/> when decoding fails.</returns>
    public byte[] DecodeData(string encoding, string value)
    {
        var decoder = GetDecoderFor(encoding);
        return decoder.Invoke(value);
    }
}
