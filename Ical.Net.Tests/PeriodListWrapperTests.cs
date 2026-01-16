//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class PeriodListWrapperTests
{
    #region ** ExceptionDates **

    [Test]
    public void AddExDateTime_ShouldCreate_DedicatePeriodList()
    {
        var cal = new Calendar();
        var evt = new CalendarEvent();
        cal.Events.Add(evt);
        var exDates = evt.ExceptionDates;

        exDates // Add date-only
            .Add(new CalDateTime(2025, 1, 1))
            .Add(new CalDateTime(2025, 1, 2))
            // duplicate
            .Add(new CalDateTime(2025, 1, 2))
            // Should go to a new PeriodList
            .Add(new CalDateTime(2025, 1, 2, 14, 0, 0, CalDateTime.UtcTzId));

        exDates.AddRange([  // Add date-time
            new CalDateTime(2025, 1, 1, 10, 11, 12, "Europe/Berlin"),
            new CalDateTime(2025, 1, 1, 10, 11, 13, "Europe/Berlin"),
            // duplicate
            new CalDateTime(2025, 1, 1, 10, 11, 13, "Europe/Berlin")
        ]);

        var serialized = new CalendarSerializer(cal).SerializeToString();

        Assert.Multiple(() =>
        {
            // 2 dedicate PeriodList objects
            Assert.That(evt.ExceptionDatesPeriodLists, Has.Count.EqualTo(3));

            // First PeriodList is date-only
            Assert.That(evt.ExceptionDatesPeriodLists[0], Has.Count.EqualTo(2));
            Assert.That(evt.ExceptionDatesPeriodLists[0].TzId, Is.Null);
            Assert.That(evt.ExceptionDatesPeriodLists[0].PeriodKind, Is.EqualTo(PeriodKind.DateOnly));

            // Second PeriodList is date-time UTC
            Assert.That(evt.ExceptionDatesPeriodLists[1], Has.Count.EqualTo(1));
            Assert.That(evt.ExceptionDatesPeriodLists[1].TzId, Is.EqualTo(CalDateTime.UtcTzId));
            Assert.That(evt.ExceptionDatesPeriodLists[1].PeriodKind, Is.EqualTo(PeriodKind.DateTime));

            // Second PeriodList is date-time
            Assert.That(evt.ExceptionDatesPeriodLists[2], Has.Count.EqualTo(2));
            Assert.That(evt.ExceptionDatesPeriodLists[2].TzId, Is.EqualTo("Europe/Berlin"));
            Assert.That(evt.ExceptionDatesPeriodLists[2].PeriodKind, Is.EqualTo(PeriodKind.DateTime));

            Assert.That(serialized,
                Does.Contain(
            "EXDATE;VALUE=DATE:20250101,20250102\r\n" +
                    "EXDATE:20250102T140000Z\r\n" +
                    "EXDATE;TZID=Europe/Berlin:20250101T101112,20250101T101113"));

            // A flattened list of all dates
            Assert.That(exDates.GetAllDates().Count(), Is.EqualTo(5));
        });
    }

    [Test]
    public void RemoveExDateTime_ShouldRemove_FromPeriodList()
    {
        var evt = new CalendarEvent();
        var exDates = evt.ExceptionDates;

        var dateOnly = new CalDateTime(2025, 1, 1);
        var dateTime = new CalDateTime(2025, 1, 1, 10, 11, 12, "Europe/Berlin");

        exDates.Add(dateOnly);
        exDates.Add(dateOnly.AddDays(1));
        exDates.Add(dateTime);

        var dateTimeSuccess = exDates.Remove(dateTime);
        var dateOnlySuccess = exDates.Remove(dateOnly);
        var dateOnlyFail = !exDates.Remove(dateOnly); // already removed

        Assert.Multiple(() =>
        {
            Assert.That(dateOnlySuccess, Is.True);
            Assert.That(dateTimeSuccess, Is.True);
            Assert.That(dateOnlyFail, Is.True);
            Assert.That(evt.ExceptionDatesPeriodLists[0], Has.Count.EqualTo(1));
            Assert.That(evt.ExceptionDatesPeriodLists[1], Is.Empty);
            // Empty lists should work as well
            evt.ExceptionDatesPeriodLists.Clear();
            Assert.That(() => exDates.Remove(dateTime), Is.False);
        });
    }

    #endregion

    #region ** RecurrenceDates **

    [Test]
    public void AddRDateTime_ShouldCreate_DedicatePeriodList()
    {
        var cal = new Calendar();
        var evt = new CalendarEvent();
        cal.Events.Add(evt);
        var recDates = evt.RecurrenceDates;

        recDates // Add date-only
            .Add(new CalDateTime(2025, 1, 1))
            .Add(new CalDateTime(2025, 1, 2))
            .Add(new CalDateTime(2025, 1, 2)); // duplicate

        recDates.AddRange([ // Add date-time
            new CalDateTime(2025, 1, 1, 10, 11, 12, "Europe/Berlin"),
            new CalDateTime(2025, 1, 1, 10, 11, 13, "Europe/Berlin"),
            new CalDateTime(2025, 1, 1, 10, 11, 13, "Europe/Berlin") // duplicate
        ]);

        var serialized = new CalendarSerializer(cal).SerializeToString();

        Assert.Multiple(() =>
        {
            // 2 dedicate PeriodList objects
            Assert.That(evt.RecurrenceDatesPeriodLists, Has.Count.EqualTo(2));

            // First PeriodList is date-only
            Assert.That(evt.RecurrenceDatesPeriodLists[0], Has.Count.EqualTo(2));
            Assert.That(evt.RecurrenceDatesPeriodLists[0].TzId, Is.Null);
            Assert.That(evt.RecurrenceDatesPeriodLists[0].PeriodKind, Is.EqualTo(PeriodKind.DateOnly));

            // Third PeriodList is date-time
            Assert.That(evt.RecurrenceDatesPeriodLists[1], Has.Count.EqualTo(2));
            Assert.That(evt.RecurrenceDatesPeriodLists[1].TzId, Is.EqualTo("Europe/Berlin"));
            Assert.That(evt.RecurrenceDatesPeriodLists[1].PeriodKind, Is.EqualTo(PeriodKind.DateTime));

            Assert.That(serialized,
                Does.Contain(
            "RDATE;VALUE=DATE:20250101,20250102\r\n" +
                    "RDATE;TZID=Europe/Berlin:20250101T101112,20250101T101113"));

            // A flattened list of all dates
            Assert.That(recDates.GetAllDates().Count(), Is.EqualTo(4));
        });
    }

    [Test]
    public void AddRPeriod_ShouldCreate_DedicatePeriodList()
    {
        var cal = new Calendar();
        var evt = new CalendarEvent();
        cal.Events.Add(evt);
        var recPeriod = evt.RecurrenceDates;

        recPeriod
            // Add date-only period
            .Add(Period.Create(new CalDateTime(2025, 1, 2), new CalDateTime(2025, 1, 5)))
            // Add zoned date-time period
            .Add(Period.Create(new CalDateTime(2025, 2, 2, 0, 0, 0, CalDateTime.UtcTzId),
                new CalDateTime(2025, 2, 2, 6, 0, 0, CalDateTime.UtcTzId)))
            // duplicate
            .Add(Period.Create(new CalDateTime(2025, 2, 2, 0, 0, 0, CalDateTime.UtcTzId),
                new CalDateTime(2025, 2, 2, 6, 0, 0, CalDateTime.UtcTzId)));

        recPeriod.AddRange([
            // Add date-only period with end time
            Period.Create(new CalDateTime(2025, 5, 1), new CalDateTime(2025, 5, 10)),
            // Add zoned date-time period with end time
            Period.Create(new CalDateTime(2025, 6, 1, 12, 0, 0, CalDateTime.UtcTzId), new CalDateTime(2025, 6, 1, 14, 0, 0, CalDateTime.UtcTzId)),
            // duplicate
            Period.Create(new CalDateTime(2025, 6, 1, 12, 0, 0, CalDateTime.UtcTzId), new CalDateTime(2025, 6, 1, 14, 0, 0, CalDateTime.UtcTzId)),
            // Add date-only with duration
            Period.Create(new CalDateTime(2025, 5, 1), duration: Duration.FromDays(9)),
            // Add zoned date-time period with duration
            Period.Create(new CalDateTime(2025, 6, 1, 12, 0, 0, "Europe/Vienna"), duration: Duration.FromHours(8))
        ]);

        var serializer = new CalendarSerializer(cal);
        var serialized = serializer.SerializeToString()!;
        // Assign the deserialized event
        cal = Calendar.Load(serialized)!;
        evt = cal.Events[0];

        // Assert the serialized string and the deserialized event
        Assert.Multiple(() =>
        {
            // 2 dedicate PeriodList objects
            Assert.That(evt!.RecurrenceDatesPeriodLists, Has.Count.EqualTo(3));

            // First PeriodList has date-only periods
            Assert.That(evt.RecurrenceDatesPeriodLists[0], Has.Count.EqualTo(3));
            Assert.That(evt.RecurrenceDatesPeriodLists[0].TzId, Is.Null);
            Assert.That(evt.RecurrenceDatesPeriodLists[0].PeriodKind, Is.EqualTo(PeriodKind.Period));

            // Second PeriodList has UTC date-time periods
            Assert.That(evt.RecurrenceDatesPeriodLists[1], Has.Count.EqualTo(2));
            Assert.That(evt.RecurrenceDatesPeriodLists[1].TzId, Is.EqualTo("UTC"));
            Assert.That(evt.RecurrenceDatesPeriodLists[1].PeriodKind, Is.EqualTo(PeriodKind.Period));

            // Third PeriodList has zoned date-time with duration
            Assert.That(evt.RecurrenceDatesPeriodLists[2], Has.Count.EqualTo(1));
            Assert.That(evt.RecurrenceDatesPeriodLists[2].TzId, Is.EqualTo("Europe/Vienna"));
            Assert.That(evt.RecurrenceDatesPeriodLists[2].PeriodKind, Is.EqualTo(PeriodKind.Period));

            Assert.That(serialized,
                Does.Contain(
            "RDATE;VALUE=PERIOD:20250102/20250105,20250501/20250510,20250501/P9D" + SerializationConstants.LineBreak +
                    "RDATE;VALUE=PERIOD:20250202T000000Z/20250202T060000Z,20250601T120000Z/20250" + SerializationConstants.LineBreak +
                    " 601T140000Z" + SerializationConstants.LineBreak +
                    "RDATE;TZID=Europe/Vienna;VALUE=PERIOD:20250601T120000/PT8H"));

            // A flattened list of all dates
            Assert.That(recPeriod.GetAllDates().Count(), Is.EqualTo(0));
            // A flattened list of all periods
            Assert.That(recPeriod.GetAllPeriods().Count(), Is.EqualTo(6));
        });
    }

    [Test]
    public void RemoveRDateTime_ShouldRemove_FromPeriodList()
    {
        var evt = new CalendarEvent();
        var recDates = evt.RecurrenceDates;

        var period1 = new Period(new CalDateTime(2025, 1, 1), Duration.FromDays(5));
        var period2 = new Period(new CalDateTime(2025, 1, 1, 10, 0, 0, "Europe/Berlin"), Duration.FromHours(6));

        recDates.Add(period1).Add(period2);
        recDates.Add(new Period(period1.StartTime.AddDays(1), Duration.FromDays(5)));

        var period1Success = recDates.Remove(period1);
        var period2Success = recDates.Remove(period2);
        var period2Fail = !recDates.Remove(period2); // already removed

        Assert.Multiple(() =>
        {
            Assert.That(period2Success, Is.True);
            Assert.That(period1Success, Is.True);
            Assert.That(period2Fail, Is.True);
            Assert.That(evt.RecurrenceDatesPeriodLists[0], Has.Count.EqualTo(1));
            Assert.That(evt.RecurrenceDatesPeriodLists[1], Is.Empty);
        });
    }

    [Test]
    public void Contains_ShouldReturnTrue_IfPeriodExists()
    {
        var evt = new CalendarEvent();
        var recDates = evt.RecurrenceDates;

        var period1 = new Period(new CalDateTime(2025, 1, 1), Duration.FromDays(5));
        var period2 = new Period(new CalDateTime(2025, 1, 1, 10, 0, 0, "Europe/Berlin"), Duration.FromHours(6));

        recDates.Add(period1).Add(period2);

        Assert.Multiple(() =>
        {
            Assert.That(recDates.Contains(period1), Is.True);
            Assert.That(recDates.Contains(period2), Is.True);
        });
    }

    [Test]
    public void Contains_ShouldReturnFalse_IfPeriodDoesNotExist()
    {
        var evt = new CalendarEvent();
        var recDates = evt.RecurrenceDates;

        var period1 = new Period(new CalDateTime(2025, 1, 1), Duration.FromDays(5));
        var period2 = new Period(new CalDateTime(2025, 1, 1, 10, 0, 0, "Europe/Berlin"), Duration.FromHours(6));

        recDates.AddRange([period1, period2]);
        
        Assert.Multiple(() =>
        {
            Assert.That(recDates.Contains(new Period(period1.StartTime.AddDays(1), Duration.FromDays(5))), Is.False);
            Assert.That(recDates.Contains(new Period(period2.StartTime.AddDays(1), Duration.FromHours(6))), Is.False);
        });
    }

    #endregion

    #region ** PeriodListWrapperBase **

    [Test]
    public void Clear_ShouldRemoveAllPeriods()
    {
        var evt = new CalendarEvent();
        var exDates = evt.ExceptionDates;

        exDates
            .Add(new CalDateTime(2025, 1, 1))
            .Add(new CalDateTime(2025, 1, 1, 10, 11, 12, "Europe/Berlin"));

        exDates.Clear();

        Assert.That(evt.ExceptionDatesPeriodLists, Is.Empty);
    }

    [Test]
    public void Contains_ShouldReturnTrue_IfDateExists()
    {
        var evt = new CalendarEvent();
        var exDates = evt.ExceptionDates;

        var dateOnly = new CalDateTime(2025, 1, 1);
        var dateTime = new CalDateTime(2025, 1, 1, 10, 11, 12, "Europe/Berlin");

        exDates.Add(dateOnly).Add(dateTime);
        
        Assert.Multiple(() =>
        {
            Assert.That(exDates.Contains(dateOnly), Is.True);
            Assert.That(exDates.Contains(dateTime), Is.True);
        });
    }

    [Test]
    public void Contains_ShouldReturnFalse_IfDateDoesNotExist()
    {
        var evt = new CalendarEvent();
        var exDates = evt.ExceptionDates;

        var dateOnly = new CalDateTime(2025, 1, 1);
        var dateTime = new CalDateTime(2025, 1, 1, 10, 11, 12, "Europe/Berlin");

        exDates.AddRange([dateOnly, dateTime]);

        Assert.Multiple(() =>
        {
            Assert.That(exDates.Contains(dateOnly.AddDays(1)), Is.False);
            Assert.That(exDates.Contains(dateTime.AddDays(1)), Is.False);
        });
    }

    #endregion
}
