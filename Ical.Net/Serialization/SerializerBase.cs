//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.IO;
using System.Text;

namespace Ical.Net.Serialization;

public abstract class SerializerBase : IStringSerializer
{
    private SerializationContext _mSerializationContext;

    protected SerializerBase()
    {
        _mSerializationContext = SerializationContext.Default;
    }

    protected SerializerBase(SerializationContext ctx)
    {
        _mSerializationContext = ctx;
    }

    public virtual SerializationContext SerializationContext // NOSONAR: auto-property
    {
        get => _mSerializationContext;
        set => _mSerializationContext = value;
    }

    public abstract Type TargetType { get; }
    public abstract string? SerializeToString(object? obj);
    public abstract object? Deserialize(TextReader tr);

    public virtual object? Deserialize(ReadOnlySpan<char> value)
    {
        // Default implementation for backward compatibility
        using var reader = new StringReader(value.ToString());
        return Deserialize(reader);
    }

    public object? Deserialize(Stream stream, Encoding encoding)
    {
        using var sr = new StreamReader(stream, encoding);
        var encodingStack = GetService<EncodingStack>();
        encodingStack.Push(encoding);
        var obj = Deserialize(sr);
        encodingStack.Pop();
        return obj;
    }

    /// <summary>
    /// Serializes the specified object to the provided stream using the specified encoding.
    /// </summary>
    /// <remarks>This method writes the serialized representation of the object to the stream without closing
    /// the stream, allowing the caller to continue using it.
    /// bytes.</remarks>
    /// <param name="obj">The object to serialize. Must not be null.</param>
    /// <param name="stream">The stream to which the serialized data will be written. Must be writable.</param>
    /// <param name="encoding">
    /// The character encoding to use for serialization.
    /// If <see langword="null"/> or missing, UTF-8 encoding
    /// without a byte order mark (BOM) is used.
    /// A BOM is incompatible with many iCalendar apps.
    /// </param>
    public void Serialize(object obj, Stream stream, Encoding? encoding = null)
    {
        // Ensure that no BOM is written to the stream
        encoding ??= new UTF8Encoding(false);

        //This is StreamWriter's built-in default buffer size
        const int defaultBuffer = 1024;
        // Important: leave the stream open so that the caller can continue to use it
        using var sw = new StreamWriter(stream, encoding, defaultBuffer, leaveOpen: true);

        // Push the current object onto the serialization stack
        SerializationContext.Push(obj);

        // Push the current encoding on the stack
        var encodingStack = GetService<EncodingStack>();
        encodingStack.Push(encoding);

        sw.Write(SerializeToString(obj));

        // Pop the current encoding off the serialization stack
        encodingStack.Pop();

        // Pop the current object off the serialization stack
        SerializationContext.Pop();
    }

    public virtual object GetService(Type serviceType) => SerializationContext.GetService(serviceType);

    public virtual object GetService(string name) => SerializationContext.GetService(name);

    public virtual T GetService<T>()
        => SerializationContext.GetService<T>();

    public virtual T GetService<T>(string name) =>
        SerializationContext.GetService<T>(name);

    public void SetService(string name, object obj)
        => SerializationContext.SetService(name, obj);

    public void SetService(object obj)
        => SerializationContext.SetService(obj);

    public void RemoveService(Type type)
        => SerializationContext.RemoveService(type);

    public void RemoveService(string name)
        => SerializationContext.RemoveService(name);
}
