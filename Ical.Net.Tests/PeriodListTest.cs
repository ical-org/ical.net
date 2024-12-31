//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using System;
using System.IO;
using Ical.Net.DataTypes;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class PeriodListTests
{
    [Test]
    public void RemovePeriod_ShouldDecreaseCount()
    {
        // Arrange
        var periodList = new PeriodList();
        var period = new Period(new CalDateTime(2023, 1, 1, 0, 0, 0), Duration.FromHours(1));
        periodList.Add(period);

        // Act
        periodList.Remove(period);

        // Assert
        Assert.That(periodList, Has.Count.EqualTo(0));
    }

    [Test]
    public void GetSet_Period_ShouldReturnCorrectPeriod()
    {
        // Arrange
        var periodList = new PeriodList();
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1));
        var period2 = new Period(new CalDateTime(2025, 2, 1, 0, 0, 0), Duration.FromHours(1));

        periodList.Add(period1);
        periodList.Add(period1);

        // Act
        var retrievedPeriod = periodList[0];
        periodList[1] = period2;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(period1, Is.EqualTo(retrievedPeriod));
            Assert.That(periodList.Contains(period1), Is.True);
            Assert.That(periodList[periodList.IndexOf(period2)], Is.EqualTo(period2));
        });
    }

    [Test]
    public void Clear_ShouldRemoveAllPeriods()
    {
        // Arrange
        var periodList = new PeriodList();
        var pl = new PeriodList
        {
            new CalDateTime(2025, 1, 2),
            new CalDateTime(2025, 1, 3)
        };

        periodList.AddRange(pl);
        var count = periodList.Count;

        // Act
        periodList.Clear();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(count, Is.EqualTo(2));
            Assert.That(periodList, Has.Count.EqualTo(0));
        });
    }

    [Test]
    public void Create_FromStringReader_ShouldSucceed()
    {
        // Arrange
        const string periodString = "20250101T000000Z/20250101T010000Z,20250102T000000Z/20250102T010000Z";
        using var reader = new StringReader(periodString);

        // Act
        var periodList = PeriodList.FromStringReader(reader);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(periodList, Has.Count.EqualTo(2));
            Assert.That(periodList[0].StartTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 0, 0, 0, "UTC")));
            Assert.That(periodList[0].EndTime, Is.EqualTo(new CalDateTime(2025, 1, 1, 1, 0, 0, "UTC")));
            Assert.That(periodList[1].StartTime, Is.EqualTo(new CalDateTime(2025, 1, 2, 0, 0, 0, "UTC")));
            Assert.That(periodList[1].EndTime, Is.EqualTo(new CalDateTime(2025, 1, 2, 1, 0, 0, "UTC")));
            Assert.That(periodList.IsReadOnly, Is.EqualTo(false));
        });
    }

    [Test]
    public void InsertAt_ShouldInsertPeriodAtCorrectPosition()
    {
        // Arrange
        var periodList = new PeriodList();
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1));
        var period2 = new Period(new CalDateTime(2025, 1, 2, 0, 0, 0), Duration.FromHours(1));
        var period3 = new Period(new CalDateTime(2025, 1, 3, 0, 0, 0), Duration.FromHours(1));
        periodList.Add(period1);
        periodList.Add(period3);

        // Act
        periodList.Insert(1, period2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(periodList, Has.Count.EqualTo(3));
            Assert.That(periodList[1], Is.EqualTo(period2));
        });
    }

    [Test]
    public void RemoveAt_ShouldRemovePeriodAtCorrectPosition()
    {
        // Arrange
        var periodList = new PeriodList();
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1));
        var period2 = new Period(new CalDateTime(2025, 1, 2, 0, 0, 0), Duration.FromHours(1));
        var period3 = new Period(new CalDateTime(2025, 1, 3, 0, 0, 0), Duration.FromHours(1));
        periodList.Add(period1);
        periodList.Add(period2);
        periodList.Add(period3);

        // Act
        periodList.RemoveAt(1);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(periodList, Has.Count.EqualTo(2));
            Assert.That(periodList[1], Is.EqualTo(period3));
        });
    }
}
