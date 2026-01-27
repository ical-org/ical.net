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
/// Similar to <see cref="System.Buffers.ArrayBufferWriter{T}"/> but using ArrayPool for memory efficiency.
/// <para/>
/// Note that failing to dispose this struct after use will result in a memory leak.
/// </summary>
internal struct PooledCharBuffer : IDisposable
{
    // ArrayPool is thread-safe
    private static readonly ArrayPool<char>
        Pool = ArrayPool<char>.Shared;

    private char[] _bufferArray;
    private int _currentLength;
    private bool _isDisposed;

    /// <summary>
    /// The default initial capacity of the buffer.
    /// </summary>
    public const int DefaultBufferCapacity = 8192;
    
    /// <summary>
    /// Creates a new <see cref="PooledCharBuffer"/> with an initial capacity of <see cref="DefaultBufferCapacity"/>.
    /// </summary>
    public PooledCharBuffer() : this(DefaultBufferCapacity)
    {
    }

    /// <summary>
    /// Creates a new <see cref="PooledCharBuffer"/> with the specified initial capacity.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity of the buffer.</param>
    public PooledCharBuffer(int initialCapacity)
    {
        _bufferArray = Pool.Rent(initialCapacity);
        _currentLength = 0;
        _isDisposed = false;
    }

    /// <summary>
    /// Gets the <see cref="Span{T}"/> of the buffer.
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
    /// Gets the current length of the buffer.
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
    /// Gets the capacity of the buffer.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public int Capacity
    {
        get
        {
            ThrowIfDisposed();
            return _bufferArray.Length;
        }
    }

    /// <summary>
    /// Resets the buffer by setting the current length to zero, allowing the buffer to be reused.
    /// </summary>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Reset()
    {
        ThrowIfDisposed();
        _currentLength = 0;
    }

    /// <summary>
    /// Writes the specified data to the buffer. Resizes the buffer if necessary.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public void Write(ReadOnlySpan<char> data)
    {
        ThrowIfDisposed();
        GrowBufferIfNeeded(data.Length);
        data.CopyTo(_bufferArray.AsSpan(_currentLength, data.Length));
        _currentLength += data.Length;
    }

    /// <summary>
    /// Grows the buffer if needed to accommodate the additional data.
    /// Does nothing if the buffer is already large enough.
    /// </summary>
    private void GrowBufferIfNeeded(int dataLength)
    {
        var requiredLength = _currentLength + dataLength;
        if (requiredLength <= Capacity) return;

        // Grow the buffer by renting a larger array and copying existing data
        var newCapacity = requiredLength * 2;
        var newArray = Pool.Rent(newCapacity);
        Array.Copy(_bufferArray, newArray, Math.Min(_currentLength, newCapacity));
        Pool.Return(_bufferArray, clearArray: true);
        _bufferArray = newArray;
    }

    /// <summary>
    /// Returns <see langword="true"/> if the buffer has been disposed.
    /// </summary>
    public bool IsDisposed => _isDisposed;

    private void ThrowIfDisposed()
    {
        if (_isDisposed) throw new ObjectDisposedException(nameof(PooledCharBuffer));
    }

    /// <summary>
    /// Returns the string representation of the buffer.
    /// </summary>
    /// <returns>The string representation of the buffer.</returns>
    /// <exception cref="ObjectDisposedException"></exception>
    public override string ToString()
    {
        ThrowIfDisposed();
        return new string(_bufferArray, 0, _currentLength);
    }

    /// <summary>
    /// Disposes the buffer, returning it to the <see cref="ArrayPool{T}"/> and clearing it for security.
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        Pool.Return(_bufferArray, clearArray: true);
        _isDisposed = true;
    }
}
