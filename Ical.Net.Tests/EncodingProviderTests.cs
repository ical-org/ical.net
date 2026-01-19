//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;
using Ical.Net.Serialization;
using NUnit.Framework;
using EncodingProvider = Ical.Net.Serialization.EncodingProvider;

namespace Ical.Net.Tests;

[TestFixture]
public class EncodingProviderTests
{
    private EncodingProvider GetEncodingProvider() => new EncodingProvider(new SerializationContext());

    [Test]
    public void Encode_ShouldReturnEncodedString_WhenValidEncodingIsProvided()
    {
        const string encoding = "8BIT";
        var data = "Hello"u8.ToArray();

        var result = GetEncodingProvider().Encode(encoding, data);

        Assert.That(result, Is.EqualTo("Hello"));
    }

    [Test]
    public void Encode_ShouldBeNull_WhenInvalidEncodingIsProvided()
    {
        const string encoding = "Invalid-Encoding";
        var data = "Hello"u8.ToArray();

        Assert.That(() => GetEncodingProvider().Encode(encoding, data), Throws.TypeOf<SerializationException>());
    }

    [Test]
    public void Decode_ShouldReturnDecodedByteArray_WhenValidEncodingIsProvided()
    {
        const string encoding = "8BIT";
        const string data = "Hello";

        var result = GetEncodingProvider().DecodeString(encoding, data);

        Assert.That(result, Is.EqualTo("Hello"u8.ToArray()));
    }

    [Test]
    public void Decode_ShouldBeNull_WhenInvalidEncodingIsProvided()
    {
        const string encoding = "Invalid-Encoding";
        const string data = "Hello";

        Assert.That(() => GetEncodingProvider().DecodeString(encoding, data), Throws.TypeOf<SerializationException>());
    }

    [Test]
    public void Encode_ShouldReturnEncodedString_WithBase64Encoding()
    {
        const string encoding = "BASE64";
        var data = "Hello"u8.ToArray();

        var result = GetEncodingProvider().Encode(encoding, data);

        Assert.That(result, Is.EqualTo("SGVsbG8=")); // "Hello" in Base64
    }

    [Test]
    public void Decode_ShouldReturnDecodedByteArray_WithBase64Encoding()
    {
        const string encoding = "BASE64";
        const string data = "SGVsbG8="; // "Hello" in Base64

        var result = GetEncodingProvider().DecodeString(encoding, data);

        Assert.That(result, Is.EqualTo("Hello"u8.ToArray()));
    }

    [Test]
    public void Decode_ShouldThrow_WithInvalidBase64String()
    {
        const string encoding = "BASE64";
        const string data = "InvalidBase64==="; // Invalid Base64 string

        Assert.Throws<FormatException>(() => GetEncodingProvider().DecodeString(encoding, data));
    }
}
