//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Buffers;

namespace Ical.Net.Utility;

/// <summary>
/// A lightweight container that rents a char array as a buffer from an <see cref="ArrayPool{T}"/> and returns it when disposed.
/// It simplifies passing around the buffer without intermediate memory allocations.
/// <para/>
/// Note that failing to dispose this struct after use will result in a memory leak.
/// </summary>
internal struct ZCharArray : IDisposable
{
    // ArrayPool is thread-safe
    private static readonly ArrayPool<char>
        Pool = ArrayPool<char>.Shared;

    private char[]? _bufferArray;
    private int _currentLength;

    /// <summary>
    /// The default capacity of the array.
    /// </summary>
    public const int DefaultBufferCapacity = 10_000_000;
    
    /// <summary>
    /// Creates a new <see cref="ZCharArray"/> with a length of <see cref="DefaultBufferCapacity"/>>.
    /// </summary>
    public ZCharArray() : this(DefaultBufferCapacity)
    {
    }

    /// <summary>
    /// Creates a new <see cref="ZCharArray"/> with the specified length.
    /// </summary>
    /// <param name="length">The length of the array.</param>
    public ZCharArray(int length)
    {
        _bufferArray = Pool.Rent(length);
        _currentLength = 0;
    }

    /// <summary>
    /// Gets the underlying character array used to store the buffer's contents.
    /// </summary>
    internal char[] UnderlyingArray
    {
        get
        {
            ThrowIfDisposed();
            return _bufferArray!;
        }
    }

    /// <summary>
    /// Gets the <see cref="Span{T}"/> of the array.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public Span<char> Span
    {
        get
        {
            ThrowIfDisposed();
            return _bufferArray.AsSpan(0, _currentLength);
        }
    }

    /// <summary>
    /// Gets the length of the array.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public int Length
    {
        get
        {
            ThrowIfDisposed();
            return _currentLength;
        }
    }

    /// <summary>
    /// Gets the capacity of the array.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public int Capacity
    {
        get
        {
            ThrowIfDisposed();
            return _bufferArray!.Length;
        }
    }

    /// <summary>
    /// Sets the current length of the array to zero.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Reset()
    {
        ThrowIfDisposed();
        _currentLength = 0;
    }

    /// <summary>
    /// Grows the array to the specified length,
    /// copying the existing elements if necessary.
    /// </summary>
    /// <param name="length">The new length of the array.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    private void Grow(int length)
    {
        var newArray = Pool.Rent(length);
        Array.Copy(_bufferArray!, newArray, Math.Min(_bufferArray!.Length, length));
        Pool.Return(_bufferArray);
        _bufferArray = newArray;
    }

    /// <summary>
    /// Writes the specified data to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(ReadOnlySpan<char> data)
    {
        ThrowIfDisposed();
        GrowBufferIfNeeded(data.Length);
        data.CopyTo(_bufferArray!.AsSpan(_currentLength, data.Length));
        _currentLength += data.Length;
    }

    /// <summary>
    /// Writes the specified char to the array. Resizes the array if necessary.
    /// </summary>
    /// <param name="c">The char to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(char c)
    {
        ThrowIfDisposed();
        GrowBufferIfNeeded(1);
        _bufferArray![_currentLength++] = c;
    }

    private void GrowBufferIfNeeded(int dataLength)
    {
        var requiredLength = _currentLength + dataLength;
        if (requiredLength <= Capacity) return;

        Grow(requiredLength * 2); // Does nothing if the buffer is already large enough
    }

    /// <summary>
    /// Returns <see langword="true"/> if the array has been disposed.
    /// </summary>
    public bool IsDisposed => _bufferArray is null;

    private void ThrowIfDisposed()
    {
        if (IsDisposed) throw new ObjectDisposedException(nameof(ZCharArray));
    }

    /// <summary>
    /// Returns the string representation of the array.
    /// </summary>
    /// <returns>The string representation of the array.</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public override string ToString()
    {
        ThrowIfDisposed();
        return new string(_bufferArray!, 0, _currentLength);
    }

    /// <summary>
    /// Disposes the array, returning it to the <see cref="ArrayPool{T}"/>.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed) return;

        Pool.Return(_bufferArray!);
        _bufferArray = null;
    }
}
