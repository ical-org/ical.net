//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Runtime.Serialization;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class DeserializationSingleTests
{
    [Test]
    public void DateTime1_Unrepresentable_DateTimeArgs_ShouldThrow_Obsolete()
    {
        Assert.That(() =>
        {
            _ = Calendar.Load(IcsFiles.DateTime1);
        }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>());
    }

    /// <summary>
    /// Treat "out of range" dates as SerializationException instead of
    /// previous ArgumentOutOfRangeException.
    /// </summary>
    [Test]
    public void DateTime1_Unrepresentable_DateTimeArgs_ShouldThrow()
    {
        Assert.That(() =>
        {
            _ = CalendarSerializer.Deserialize<Calendar>(IcsFiles.DateTime1);
        }, Throws.Exception.TypeOf<SerializationException>());
    }
}
