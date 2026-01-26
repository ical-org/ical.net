//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Globalization;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class PooledCharBufferTests
{
    [Test]
    public void DefaultConstructor_ShouldCreateArrayWithDefaultCapacity()
    {
        using var buffer = new PooledCharBuffer();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Capacity, Is.GreaterThanOrEqualTo(PooledCharBuffer.DefaultBufferCapacity));
            Assert.That(buffer.Length, Is.EqualTo(0));
            Assert.That(buffer.IsDisposed, Is.False);
        }
    }

    [Test]
    public void ParameterizedConstructor_ShouldCreateArrayWithSpecifiedCapacity()
    {
        const int specifiedCapacity = 5000;
        using var buffer = new PooledCharBuffer(specifiedCapacity);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Capacity, Is.GreaterThanOrEqualTo(specifiedCapacity));
            Assert.That(buffer.Length, Is.EqualTo(0));
        }
    }

    [Test]
    public void Length_ShouldReturnZero_WhenNoDataWritten()
    {
        using var buffer = new PooledCharBuffer(100);
        
        Assert.That(buffer.Length, Is.EqualTo(0));
    }

    [Test]
    public void Capacity_ShouldReturnAtLeastRequestedSize()
    {
        const int requestedSize = 1000;
        using var buffer = new PooledCharBuffer(requestedSize);
        
        Assert.That(buffer.Capacity, Is.GreaterThanOrEqualTo(requestedSize));
    }

    [Test]
    public void WriteSpan_ShouldIncrementLength()
    {
        using var buffer = new PooledCharBuffer(100);
        buffer.Write("Hello".AsSpan());
        
        Assert.That(buffer.Length, Is.EqualTo(5));
    }

    [Test]
    public void WriteMultipleTimes_ShouldAppendData()
    {
        using var buffer = new PooledCharBuffer(100);
        
        buffer.Write("Hello".AsSpan());
        buffer.Write(" ".AsSpan());
        buffer.Write("World".AsSpan());
        
        Assert.That(buffer.Length, Is.EqualTo(11));
        Assert.That(buffer.ToString(), Is.EqualTo("Hello World"));
    }

    [Test]
    public void WriteEmptySpan_ShouldNotChangeLength()
    {
        using var buffer = new PooledCharBuffer(100);
        var emptyData = ReadOnlySpan<char>.Empty;
        
        buffer.Write(emptyData);
        
        Assert.That(buffer.Length, Is.EqualTo(0));
    }

    [Test]
    public void Write_ShouldGrowCapacity_WhenExceedingInitialSize()
    {
        using var buffer = new PooledCharBuffer(10);
        var initialCapacity = buffer.Capacity;
        
        // Write more than initial capacity
        buffer.Write("This is a very long string that exceeds initial capacity".AsSpan());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Capacity, Is.GreaterThan(initialCapacity));
            Assert.That(buffer.ToString(), Is.EqualTo("This is a very long string that exceeds initial capacity"));
        }
    }

    [Test]
    public void Reset_ShouldSetLengthToZero()
    {
        using var buffer = new PooledCharBuffer(100);
        
        buffer.Write("Test data".AsSpan());
        Assert.That(buffer.Length, Is.EqualTo(9));
        
        buffer.Reset();
        
        Assert.That(buffer.Length, Is.EqualTo(0));
    }

    [Test]
    public void Reset_ShouldNotChangeCapacity()
    {
        using var buffer = new PooledCharBuffer(100);
        var initialCapacity = buffer.Capacity;
        
        buffer.Write("Test data".AsSpan());
        buffer.Reset();
        
        Assert.That(buffer.Capacity, Is.EqualTo(initialCapacity));
    }

    [Test]
    public void Reset_ShouldAllowReuse()
    {
        using var buffer = new PooledCharBuffer(100);
        
        buffer.Write("First".AsSpan());
        buffer.Reset();
        buffer.Write("Second".AsSpan());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.ToString(), Is.EqualTo("Second"));
            Assert.That(buffer.Length, Is.EqualTo(6));
        }
    }

    [Test]
    public void ToString_ShouldReturnEmptyString_WhenNoDataWritten()
    {
        using var buffer = new PooledCharBuffer(100);
        
        Assert.That(buffer.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToString_ShouldReturnCorrectString()
    {
        using var buffer = new PooledCharBuffer(100);
        const string expected = "Test String";
        
        buffer.Write(expected.AsSpan());
        
        Assert.That(buffer.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void IsDisposed_ShouldReturnFalse_WhenNotDisposed()
    {
        var buffer = new PooledCharBuffer(100);
        
        Assert.That(buffer.IsDisposed, Is.False);
        
        buffer.Dispose();
    }

    [Test]
    public void IsDisposed_ShouldReturnTrue_AfterDispose()
    {
        var buffer = new PooledCharBuffer(100);
        buffer.Dispose();
        
        Assert.That(buffer.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_ShouldBeIdempotent()
    {
        var buffer = new PooledCharBuffer(100);
        
        buffer.Dispose();
        Assert.DoesNotThrow(() => buffer.Dispose());
        
        Assert.That(buffer.IsDisposed, Is.True);
    }

    [Test]
    public void Length_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var buffer = new PooledCharBuffer(100);
        buffer.Dispose();
        
        Assert.That(() => _ = buffer.Length, 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Capacity_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var buffer = new PooledCharBuffer(100);
        buffer.Dispose();
        
        Assert.That(() => _ = buffer.Capacity, 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void WriteSpan_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var buffer = new PooledCharBuffer(100);
        buffer.Dispose();
        
        Assert.That(() => buffer.Write("test".AsSpan()), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Reset_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var buffer = new PooledCharBuffer(100);
        buffer.Dispose();
        
        Assert.That(() => buffer.Reset(), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void ToString_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var buffer = new PooledCharBuffer(100);
        buffer.Dispose();
        
        Assert.That(() => buffer.ToString(), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Constants_ShouldHaveExpectedValues()
        => Assert.That(PooledCharBuffer.DefaultBufferCapacity, Is.EqualTo(8192));

    [Test]
    public void LargeDataWrite_ShouldHandleCorrectly()
    {
        using var buffer = new PooledCharBuffer(100);
        var largeString = new string('x', 50000);
        
        buffer.Write(largeString.AsSpan());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Length, Is.EqualTo(50000));
            Assert.That(buffer.ToString(), Is.EqualTo(largeString));
        }
    }

    [Test]
    public void MultipleGrows_ShouldPreserveData()
    {
        using var buffer = new PooledCharBuffer(5);
        
        buffer.Write("1".AsSpan());
        buffer.Write("2".AsSpan());
        buffer.Write("3".AsSpan());
        buffer.Write("4".AsSpan());
        buffer.Write("5".AsSpan());
        buffer.Write("6789012345".AsSpan()); // Should trigger grow
        
        Assert.That(buffer.ToString(), Is.EqualTo("123456789012345"));
    }

    [Test]
    public void SpecialCharacters_ShouldBeHandledCorrectly()
    {
        using var buffer = new PooledCharBuffer(100);
        const string specialChars = "Hello\nWorld\r\nTab\tNull\0End";
        
        buffer.Write(specialChars.AsSpan());
        
        Assert.That(buffer.ToString(), Is.EqualTo(specialChars));
    }

    [Test]
    public void UnicodeCharacters_ShouldBeHandledCorrectly()
    {
        using var buffer = new PooledCharBuffer(100);
        const string unicode = "Hello 世界 🌍 Ω α β";
        
        buffer.Write(unicode.AsSpan());
        
        Assert.That(buffer.ToString(), Is.EqualTo(unicode));
    }

    [Test]
    public void SmallCapacity_ShouldStillWork()
    {
        using var buffer = new PooledCharBuffer(1);
        
        buffer.Write("a".AsSpan());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(buffer.Length, Is.EqualTo(1));
            Assert.That(buffer.ToString(), Is.EqualTo("a"));
        }
    }

    [Test]
    public void GetSpan()
    {
        using var buffer = new PooledCharBuffer();
        buffer.Write("HelloWorld".AsSpan());
        var span = buffer.Span;
        Assert.That(span.ToString(), Is.EqualTo("HelloWorld"));
    }

    [Test]
    public void SequentialWrites_ShouldMaintainOrder()
    {
        using var buffer = new PooledCharBuffer(100);
        
        for (var i = 0; i < 10; i++)
        {
            buffer.Write(i.ToString(CultureInfo.InvariantCulture).AsSpan());
        }
        
        Assert.That(buffer.ToString(), Is.EqualTo("0123456789"));
    }

    [Test]
    public void AlternatingWriteMethods_ShouldWork()
    {
        using var buffer = new PooledCharBuffer(100);
        
        buffer.Write("Hello".AsSpan());
        buffer.Write(",".AsSpan());
        buffer.Write(" ".AsSpan());
        buffer.Write("World".AsSpan());
        buffer.Write("!".AsSpan());
        
        
        Assert.That(buffer.ToString(), Is.EqualTo("Hello, World!"));
    }
}
