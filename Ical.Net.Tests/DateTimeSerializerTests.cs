﻿//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using Ical.Net.DataTypes;
using Ical.Net.Serialization.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class DateTimeSerializerTests
{
    [Test, Category("Deserialization")]
    public void TZIDPropertyShouldBeAppliedForLocalTimezones()
    {
        // see http://www.ietf.org/rfc/rfc2445.txt p.36
        var result = new DateTimeSerializer()
            .SerializeToString(
                new CalDateTime(new DateTime(1997, 7, 14, 13, 30, 0, DateTimeKind.Local), "US-Eastern"));

        // TZID is applied elsewhere - just make sure this doesn't have 'Z' appended. 
        Assert.That(result, Is.EqualTo("19970714T133000"));
    }
}