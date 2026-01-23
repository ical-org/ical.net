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
public class ZCharArrayTests
{
    [Test]
    public void DefaultConstructor_ShouldCreateArrayWithDefaultCapacity()
    {
        using var zCharArray = new ZCharArray();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.Capacity, Is.GreaterThanOrEqualTo(ZCharArray.DefaultBufferCapacity));
            Assert.That(zCharArray.Length, Is.EqualTo(0));
            Assert.That(zCharArray.IsDisposed, Is.False);
        }
    }

    [Test]
    public void ParameterizedConstructor_ShouldCreateArrayWithSpecifiedCapacity()
    {
        const int specifiedCapacity = 5000;
        using var zCharArray = new ZCharArray(specifiedCapacity);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.Capacity, Is.GreaterThanOrEqualTo(specifiedCapacity));
            Assert.That(zCharArray.Length, Is.EqualTo(0));
        }
    }

    [Test]
    public void Length_ShouldReturnZero_WhenNoDataWritten()
    {
        using var zCharArray = new ZCharArray(100);
        
        Assert.That(zCharArray.Length, Is.EqualTo(0));
    }

    [Test]
    public void Capacity_ShouldReturnAtLeastRequestedSize()
    {
        const int requestedSize = 1000;
        using var zCharArray = new ZCharArray(requestedSize);
        
        Assert.That(zCharArray.Capacity, Is.GreaterThanOrEqualTo(requestedSize));
    }

    [Test]
    public void WriteChar_ShouldIncrementLength()
    {
        using var zCharArray = new ZCharArray(100);
        
        zCharArray.Write('a');
        zCharArray.Write('b');
        
        Assert.That(zCharArray.Length, Is.EqualTo(2));
    }

    [Test]
    public void WriteChar_ShouldStoreCharacter()
    {
        using var zCharArray = new ZCharArray(100);
        
        zCharArray.Write('H');
        zCharArray.Write('i');
        
        Assert.That(zCharArray.ToString(), Is.EqualTo("Hi"));
    }

    [Test]
    public void WriteSpan_ShouldIncrementLength()
    {
        using var zCharArray = new ZCharArray(100);
        zCharArray.Write("Hello".AsSpan());
        
        Assert.That(zCharArray.Length, Is.EqualTo(5));
    }

    [Test]
    public void WriteMultipleTimes_ShouldAppendData()
    {
        using var zCharArray = new ZCharArray(100);
        
        zCharArray.Write("Hello".AsSpan());
        zCharArray.Write(' ');
        zCharArray.Write("World".AsSpan());
        
        Assert.That(zCharArray.Length, Is.EqualTo(11));
        Assert.That(zCharArray.ToString(), Is.EqualTo("Hello World"));
    }

    [Test]
    public void WriteEmptySpan_ShouldNotChangeLength()
    {
        using var zCharArray = new ZCharArray(100);
        var emptyData = ReadOnlySpan<char>.Empty;
        
        zCharArray.Write(emptyData);
        
        Assert.That(zCharArray.Length, Is.EqualTo(0));
    }

    [Test]
    public void Write_ShouldGrowCapacity_WhenExceedingInitialSize()
    {
        using var zCharArray = new ZCharArray(10);
        var initialCapacity = zCharArray.Capacity;
        
        // Write more than initial capacity
        zCharArray.Write("This is a very long string that exceeds initial capacity".AsSpan());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.Capacity, Is.GreaterThan(initialCapacity));
            Assert.That(zCharArray.ToString(), Is.EqualTo("This is a very long string that exceeds initial capacity"));
        }
    }

    [Test]
    public void WriteChar_ShouldGrowCapacity_WhenExceedingInitialSize()
    {
        using var zCharArray = new ZCharArray(2);
        var initialCapacity = zCharArray.Capacity;
        
        for (var i = 0; i < 100; i++)
        {
            zCharArray.Write('a');
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.Capacity, Is.GreaterThan(initialCapacity));
            Assert.That(zCharArray.Length, Is.EqualTo(100));
        }
    }

    [Test]
    public void Reset_ShouldSetLengthToZero()
    {
        using var zCharArray = new ZCharArray(100);
        
        zCharArray.Write("Test data".AsSpan());
        Assert.That(zCharArray.Length, Is.EqualTo(9));
        
        zCharArray.Reset();
        
        Assert.That(zCharArray.Length, Is.EqualTo(0));
    }

    [Test]
    public void Reset_ShouldNotChangeCapacity()
    {
        using var zCharArray = new ZCharArray(100);
        var initialCapacity = zCharArray.Capacity;
        
        zCharArray.Write("Test data".AsSpan());
        zCharArray.Reset();
        
        Assert.That(zCharArray.Capacity, Is.EqualTo(initialCapacity));
    }

    [Test]
    public void Reset_ShouldAllowReuse()
    {
        using var zCharArray = new ZCharArray(100);
        
        zCharArray.Write("First".AsSpan());
        zCharArray.Reset();
        zCharArray.Write("Second".AsSpan());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.ToString(), Is.EqualTo("Second"));
            Assert.That(zCharArray.Length, Is.EqualTo(6));
        }
    }

    [Test]
    public void ToString_ShouldReturnEmptyString_WhenNoDataWritten()
    {
        using var zCharArray = new ZCharArray(100);
        
        Assert.That(zCharArray.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void ToString_ShouldReturnCorrectString()
    {
        using var zCharArray = new ZCharArray(100);
        const string expected = "Test String";
        
        zCharArray.Write(expected.AsSpan());
        
        Assert.That(zCharArray.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void IsDisposed_ShouldReturnFalse_WhenNotDisposed()
    {
        var zCharArray = new ZCharArray(100);
        
        Assert.That(zCharArray.IsDisposed, Is.False);
        
        zCharArray.Dispose();
    }

    [Test]
    public void IsDisposed_ShouldReturnTrue_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(zCharArray.IsDisposed, Is.True);
    }

    [Test]
    public void Dispose_ShouldBeIdempotent()
    {
        var zCharArray = new ZCharArray(100);
        
        zCharArray.Dispose();
        Assert.DoesNotThrow(() => zCharArray.Dispose());
        
        Assert.That(zCharArray.IsDisposed, Is.True);
    }

    [Test]
    public void Length_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(() => _ = zCharArray.Length, 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Capacity_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(() => _ = zCharArray.Capacity, 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void WriteChar_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(() => zCharArray.Write('a'), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void WriteSpan_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(() => zCharArray.Write("test".AsSpan()), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Reset_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(() => zCharArray.Reset(), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void ToString_ShouldThrowObjectDisposedException_AfterDispose()
    {
        var zCharArray = new ZCharArray(100);
        zCharArray.Dispose();
        
        Assert.That(() => zCharArray.ToString(), 
            Throws.TypeOf<ObjectDisposedException>());
    }

    [Test]
    public void Constants_ShouldHaveExpectedValues()
        => Assert.That(ZCharArray.DefaultBufferCapacity, Is.EqualTo(10_000_000));

    [Test]
    public void LargeDataWrite_ShouldHandleCorrectly()
    {
        using var zCharArray = new ZCharArray(100);
        var largeString = new string('x', 50000);
        
        zCharArray.Write(largeString.AsSpan());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.Length, Is.EqualTo(50000));
            Assert.That(zCharArray.ToString(), Is.EqualTo(largeString));
        }
    }

    [Test]
    public void MultipleGrows_ShouldPreserveData()
    {
        using var zCharArray = new ZCharArray(5);
        
        zCharArray.Write("1".AsSpan());
        zCharArray.Write("2".AsSpan());
        zCharArray.Write("3".AsSpan());
        zCharArray.Write("4".AsSpan());
        zCharArray.Write("5".AsSpan());
        zCharArray.Write("6789012345".AsSpan()); // Should trigger grow
        
        Assert.That(zCharArray.ToString(), Is.EqualTo("123456789012345"));
    }

    [Test]
    public void SpecialCharacters_ShouldBeHandledCorrectly()
    {
        using var zCharArray = new ZCharArray(100);
        const string specialChars = "Hello\nWorld\r\nTab\tNull\0End";
        
        zCharArray.Write(specialChars.AsSpan());
        
        Assert.That(zCharArray.ToString(), Is.EqualTo(specialChars));
    }

    [Test]
    public void UnicodeCharacters_ShouldBeHandledCorrectly()
    {
        using var zCharArray = new ZCharArray(100);
        const string unicode = "Hello 世界 🌍 Ω α β";
        
        zCharArray.Write(unicode.AsSpan());
        
        Assert.That(zCharArray.ToString(), Is.EqualTo(unicode));
    }

    [Test]
    public void SmallCapacity_ShouldStillWork()
    {
        using var zCharArray = new ZCharArray(1);
        
        zCharArray.Write('a');

        using (Assert.EnterMultipleScope())
        {
            Assert.That(zCharArray.Length, Is.EqualTo(1));
            Assert.That(zCharArray.ToString(), Is.EqualTo("a"));
        }
    }

    [Test]
    public void GetSpan()
    {
        using var zCharArray = new ZCharArray();
        zCharArray.Write("HelloWorld".AsSpan());
        var span = zCharArray.Span;
        Assert.That(span.ToString(), Is.EqualTo("HelloWorld"));
    }

    [Test]
    public void SequentialWrites_ShouldMaintainOrder()
    {
        using var zCharArray = new ZCharArray(100);
        
        for (var i = 0; i < 10; i++)
        {
            zCharArray.Write(i.ToString(CultureInfo.InvariantCulture).AsSpan());
        }
        
        Assert.That(zCharArray.ToString(), Is.EqualTo("0123456789"));
    }

    [Test]
    public void AlternatingWriteMethods_ShouldWork()
    {
        using var zCharArray = new ZCharArray(100);
        
        zCharArray.Write("Hello".AsSpan());
        zCharArray.Write(',');
        zCharArray.Write(' ');
        zCharArray.Write("World".AsSpan());
        zCharArray.Write('!');
        
        Assert.That(zCharArray.ToString(), Is.EqualTo("Hello, World!"));
    }
}
