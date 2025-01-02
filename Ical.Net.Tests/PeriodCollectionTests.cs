//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

#nullable enable
using Ical.Net.Collections;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class PeriodCollectionTests
{
    [Test]
    public void RemovePeriod_ShouldDecreaseCount()
    {
        // Arrange
        var recCollection = new RecurrencePeriodCollection();
        var period = new Period(new CalDateTime(2023, 1, 1, 0, 0, 0), Duration.FromHours(1));
        recCollection.Add(period);

        // Act
        recCollection.Remove(period);

        // Assert
        Assert.That(recCollection, Has.Count.EqualTo(0));
    }

    [Test]
    public void GetSet_Period_ShouldReturnCorrectPeriod()
    {
        // Arrange
        var recCollection = new RecurrencePeriodCollection();
        var period1 = new Period(new CalDateTime(2025, 1, 1, 0, 0, 0), Duration.FromHours(1));
        var period2 = new Period(new CalDateTime(2025, 2, 1, 0, 0, 0), Duration.FromHours(1));

        recCollection.Add(period1);
        recCollection.Add(period1);

        // Act
        var retrievedPeriod = recCollection[0];
        recCollection[1] = period2;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(period1, Is.EqualTo(retrievedPeriod));
            Assert.That(recCollection.Contains(period1), Is.True);
        });
    }

    [Test]
    public void Clear_ShouldRemoveAll_PeriodsAdded()
    {
        // Arrange
        var recCollection = new RecurrencePeriodCollection();
        var pl = new PeriodList
        {
            new CalDateTime(2025, 1, 2),
            new CalDateTime(2025, 1, 3)
        };

        recCollection.AddRange( [pl] );
        recCollection.AddRange(
        [
            new CalDateTime(2025, 10, 1),
            new CalDateTime(2025, 10, 2)
        ]);

        var count = recCollection.Count;

        // Act
        recCollection.Clear();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(count, Is.EqualTo(4));
            Assert.That(recCollection, Has.Count.EqualTo(0));
            Assert.That(recCollection.IsReadOnly, Is.False);
        });
    }

    [Test]
    public void CopyToPeriod_ShouldCopyPeriodsCorrectly()
    {
        // Arrange
        var recCollection = new RecurrencePeriodCollection([new CalDateTime(2025, 1, 2),
            new CalDateTime(2025, 1, 3)]);

        var array = new Period[2];

        // Act
        recCollection.CopyTo(array, 0);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(array[0], Is.EqualTo(recCollection[0]));
            Assert.That(array[1], Is.EqualTo(recCollection[1]));
        });
    }

    [Test]
    public void Create_RecurrencePeriodCollection_With_DateTime()
    {
        var recCollection = new RecurrencePeriodCollection(new CalDateTime(2025, 1, 2));

        Assert.Multiple(() =>
        {
            Assert.That(recCollection, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public void Create_RecurrencePeriodCollection_With_PeriodEnumerable()
    {
        var recCollection = new RecurrencePeriodCollection([new Period(new CalDateTime(2025, 1, 2), Duration.FromDays(1))]);

        Assert.Multiple(() =>
        {
            Assert.That(recCollection, Has.Count.EqualTo(1));
            Assert.That(recCollection[0].Duration, Is.EqualTo(Duration.FromDays(1)));
        });
    }

    [Test]
    public void Create_ExceptionDateCollection_With_DateTimeEnumerable()
    {
        var exDateCollection = new ExceptionDateCollection([new CalDateTime(2025, 1, 2),
            new CalDateTime(2025, 1, 3)]);

        Assert.Multiple(() =>
        {
            Assert.That(exDateCollection, Has.Count.EqualTo(2));
        });
    }

    [Test]
    public void Create_ExceptionDateCollection_With_Period()
    {
        var exDateCollection = new ExceptionDateCollection();
        exDateCollection.Add(new Period(new CalDateTime(2025, 1, 2), Duration.FromHours(1)));
        
        Assert.Multiple(() =>
        {
            Assert.That(exDateCollection, Has.Count.EqualTo(1));
            Assert.That(exDateCollection[0].EffectiveDuration, Is.Null);
        });
    }
}
