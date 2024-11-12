//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Ical.Net.Utility;
using NUnit.Framework;

namespace Ical.Net.Tests;

public class EqualityAndHashingTests
{
    private const string _someTz = "America/Los_Angeles";
    private static readonly DateTime _nowTime = DateTime.Parse("2016-07-16T16:47:02.9310521-04:00");
    private static readonly DateTime _later = _nowTime.AddHours(1);

    [Test, TestCaseSource(nameof(CalDateTime_TestCases))]
    public void CalDateTime_Tests(CalDateTime incomingDt, CalDateTime expectedDt)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expectedDt.Value, Is.EqualTo(incomingDt.Value));
            Assert.That(expectedDt.GetHashCode(), Is.EqualTo(incomingDt.GetHashCode()));
            Assert.That(expectedDt.TzId, Is.EqualTo(incomingDt.TzId));
            Assert.That(incomingDt.Equals(expectedDt), Is.True);
        });
    }

    public static IEnumerable CalDateTime_TestCases()
    {
        var nowCalDt = new CalDateTime(_nowTime);
        yield return new TestCaseData(nowCalDt, new CalDateTime(_nowTime)).SetName("Now, no time zone");

        var nowCalDtWithTz = new CalDateTime(_nowTime, _someTz);
        yield return new TestCaseData(nowCalDtWithTz, new CalDateTime(_nowTime, _someTz)).SetName("Now, with time zone");
    }

    [Test]
    public void RecurrencePatternTests()
    {
        var patternA = GetSimpleRecurrencePattern();
        var patternB = GetSimpleRecurrencePattern();

        Assert.That(patternB, Is.EqualTo(patternA));
        Assert.That(patternB.GetHashCode(), Is.EqualTo(patternA.GetHashCode()));
    }

    [Test, TestCaseSource(nameof(Event_TestCases))]
    public void Event_Tests(CalendarEvent incoming, CalendarEvent expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expected.DtStart, Is.EqualTo(incoming.DtStart));
            Assert.That(expected.DtEnd, Is.EqualTo(incoming.DtEnd));
            Assert.That(expected.Location, Is.EqualTo(incoming.Location));
            Assert.That(expected.Status, Is.EqualTo(incoming.Status));
            Assert.That(expected.IsActive, Is.EqualTo(incoming.IsActive));
            Assert.That(expected.Duration, Is.EqualTo(incoming.Duration));
            Assert.That(expected.Transparency, Is.EqualTo(incoming.Transparency));
            Assert.That(expected.GetHashCode(), Is.EqualTo(incoming.GetHashCode()));
            Assert.That(incoming.Equals(expected), Is.True);
        });
    }

    private static RecurrencePattern GetSimpleRecurrencePattern() => new RecurrencePattern(FrequencyType.Daily, 1)
    {
        Count = 5
    };

    private static CalendarEvent GetSimpleEvent() => new CalendarEvent
    {
        DtStart = new CalDateTime(_nowTime),
        DtEnd = new CalDateTime(_later),
        Duration = TimeSpan.FromHours(1),
    };

    private static string SerializeEvent(CalendarEvent e) => new CalendarSerializer().SerializeToString(new Calendar { Events = { e } });


    public static IEnumerable Event_TestCases()
    {
        var outgoing = GetSimpleEvent();
        var expected = GetSimpleEvent();
        yield return new TestCaseData(outgoing, expected).SetName("Events with start, end, and duration");

        var fiveA = GetSimpleRecurrencePattern();
        var fiveB = GetSimpleRecurrencePattern();

        outgoing = GetSimpleEvent();
        expected = GetSimpleEvent();
        outgoing.RecurrenceRules = new List<RecurrencePattern> { fiveA };
        expected.RecurrenceRules = new List<RecurrencePattern> { fiveB };
        yield return new TestCaseData(outgoing, expected).SetName("Events with start, end, duration, and one recurrence rule");
    }

    [Test]
    public void Calendar_Tests()
    {
        var rruleA = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5
        };

        var e = new CalendarEvent
        {
            DtStart = new CalDateTime(_nowTime),
            DtEnd = new CalDateTime(_later),
            Duration = TimeSpan.FromHours(1),
            RecurrenceRules = new List<RecurrencePattern> { rruleA },
        };

        var actualCalendar = new Calendar();
        actualCalendar.Events.Add(e);

        //Work around referential equality...
        var rruleB = new RecurrencePattern(FrequencyType.Daily, 1)
        {
            Count = 5
        };

        var expectedCalendar = new Calendar();
        expectedCalendar.Events.Add(new CalendarEvent
        {
            DtStart = new CalDateTime(_nowTime),
            DtEnd = new CalDateTime(_later),
            Duration = TimeSpan.FromHours(1),
            RecurrenceRules = new List<RecurrencePattern> { rruleB },
        });

        Assert.Multiple(() =>
        {
            Assert.That(expectedCalendar.GetHashCode(), Is.EqualTo(actualCalendar.GetHashCode()));
            Assert.That(actualCalendar.Equals(expectedCalendar), Is.True);
        });
    }

    [Test, TestCaseSource(nameof(VTimeZone_TestCases))]
    public void VTimeZone_Tests(VTimeZone actual, VTimeZone expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(expected.Url, Is.EqualTo(actual.Url));
            Assert.That(expected.TzId, Is.EqualTo(actual.TzId));
            Assert.That(expected, Is.EqualTo(actual));
            Assert.That(expected.GetHashCode(), Is.EqualTo(actual.GetHashCode()));
        });
    }

    public static IEnumerable VTimeZone_TestCases()
    {
        const string nzSt = "New Zealand Standard Time";
        var first = new VTimeZone
        {
            TzId = nzSt,
        };
        var second = new VTimeZone(nzSt);
        yield return new TestCaseData(first, second);

        first.Url = new Uri("http://example.com/");
        second.Url = new Uri("http://example.com");
        yield return new TestCaseData(first, second);
    }

    [Test, TestCaseSource(nameof(Attendees_TestCases))]
    public void Attendees_Tests(Attendee actual, Attendee expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.GetHashCode(), Is.EqualTo(expected.GetHashCode()));
            Assert.That(actual, Is.EqualTo(expected));
        });
    }

    public static IEnumerable Attendees_TestCases()
    {
        var tentative1 = new Attendee("MAILTO:james@example.com")
        {
            CommonName = "James Tentative",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Tentative
        };
        var tentative2 = new Attendee("MAILTO:james@example.com")
        {
            CommonName = "James Tentative",
            Role = ParticipationRole.RequiredParticipant,
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Tentative
        };
        yield return new TestCaseData(tentative1, tentative2).SetName("Simple attendee test case");

        var complex1 = new Attendee("MAILTO:mary@example.com")
        {
            CommonName = "Mary Accepted",
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Accepted,
            SentBy = new Uri("mailto:someone@example.com"),
            DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"),
            Type = "CuType",
            Members = new List<string> { "Group A", "Group B" },
            Role = ParticipationRole.Chair,
            DelegatedTo = new List<string> { "Peon A", "Peon B" },
            DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B" }
        };
        var complex2 = new Attendee("MAILTO:mary@example.com")
        {
            CommonName = "Mary Accepted",
            Rsvp = true,
            ParticipationStatus = EventParticipationStatus.Accepted,
            SentBy = new Uri("mailto:someone@example.com"),
            DirectoryEntry = new Uri("ldap://example.com:6666/o=eDABC Industries,c=3DUS??(cn=3DBMary Accepted)"),
            Type = "CuType",
            Members = new List<string> { "Group A", "Group B" },
            Role = ParticipationRole.Chair,
            DelegatedTo = new List<string> { "Peon A", "Peon B" },
            DelegatedFrom = new List<string> { "Bigwig A", "Bigwig B" }
        };
        yield return new TestCaseData(complex1, complex2).SetName("Complex attendee test");
    }

    [Test, TestCaseSource(nameof(CalendarCollection_TestCases))]
    public void CalendarCollection_Tests(string rawCalendar)
    {
        var a = Calendar.Load(IcsFiles.UsHolidays);
        var b = Calendar.Load(IcsFiles.UsHolidays);

        Assert.That(a, Is.Not.Null);
        Assert.That(b, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(b.GetHashCode(), Is.EqualTo(a.GetHashCode()));
            Assert.That(b, Is.EqualTo(a));
        });
    }

    public static IEnumerable CalendarCollection_TestCases()
    {
        yield return new TestCaseData(IcsFiles.Google1).SetName("Google calendar test case");
        yield return new TestCaseData(IcsFiles.Parse1).SetName("Weird file parse test case");
        yield return new TestCaseData(IcsFiles.UsHolidays).SetName("US Holidays (quite large)");
    }

    [Test]
    public void Resources_Tests()
    {
        var origContents = new[] { "Foo", "Bar" };
        var e = GetSimpleEvent();
        e.Resources = new List<string>(origContents);
        Assert.That(e.Resources.Count == 2, Is.True);

        e.Resources.Add("Baz");
        Assert.That(e.Resources.Count == 3, Is.True);
        var serialized = SerializeEvent(e);
        Assert.That(serialized.Contains("Baz"), Is.True);

        e.Resources.Remove("Baz");
        Assert.That(e.Resources.Count == 2, Is.True);
        serialized = SerializeEvent(e);
        Assert.That(serialized.Contains("Baz"), Is.False);

        e.Resources.Add("Hello");
        Assert.That(e.Resources.Contains("Hello"), Is.True);
        serialized = SerializeEvent(e);
        Assert.That(serialized.Contains("Hello"), Is.True);

        e.Resources.Clear();
        e.Resources.AddRange(origContents);
        Assert.That(origContents, Is.EquivalentTo(e.Resources));
        serialized = SerializeEvent(e);
        Assert.Multiple(() =>
        {
            Assert.That(serialized.Contains("Foo"), Is.True);
            Assert.That(serialized.Contains("Bar"), Is.True);
            Assert.That(serialized.Contains("Baz"), Is.False);
            Assert.That(serialized.Contains("Hello"), Is.False);
        });
    }

    internal static (byte[] original, byte[] copy) GetAttachments()
    {
        var payload = Encoding.UTF8.GetBytes("This is an attachment!");
        var payloadCopy = new byte[payload.Length];
        Array.Copy(payload, payloadCopy, payload.Length);
        return (payload, payloadCopy);
    }

    [Test, TestCaseSource(nameof(RecurringComponentAttachment_TestCases))]
    public void RecurringComponentAttachmentTests(RecurringComponent noAttachment, RecurringComponent withAttachment)
    {
        var attachments = GetAttachments();

        Assert.That(withAttachment, Is.Not.EqualTo(noAttachment));
        Assert.That(withAttachment.GetHashCode(), Is.Not.EqualTo(noAttachment.GetHashCode()));

        noAttachment.Attachments.Add(new Attachment(attachments.copy));

        Assert.That(withAttachment, Is.EqualTo(noAttachment));
        Assert.That(withAttachment.GetHashCode(), Is.EqualTo(noAttachment.GetHashCode()));
    }

    public static IEnumerable RecurringComponentAttachment_TestCases()
    {
        var attachments = GetAttachments();

        var journalNoAttach = new Journal { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
        var journalWithAttach = new Journal { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
        journalWithAttach.Attachments.Add(new Attachment(attachments.original));
        yield return new TestCaseData(journalNoAttach, journalWithAttach).SetName("Journal recurring component attachment");

        var todoNoAttach = new Todo { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
        var todoWithAttach = new Todo { Start = new CalDateTime(_nowTime), Summary = "A summary!", Class = "Some class!" };
        todoWithAttach.Attachments.Add(new Attachment(attachments.original));
        yield return new TestCaseData(todoNoAttach, todoWithAttach).SetName("Todo recurring component attachment");

        var eventNoAttach = GetSimpleEvent();
        var eventWithAttach = GetSimpleEvent();
        eventWithAttach.Attachments.Add(new Attachment(attachments.original));
        yield return new TestCaseData(eventNoAttach, eventWithAttach).SetName("Event recurring component attachment");
    }

    [Test, TestCaseSource(nameof(PeriodTestCases))]
    public void PeriodTests(Period a, Period b)
    {
        Assert.Multiple(() =>
        {
            Assert.That(b.GetHashCode(), Is.EqualTo(a.GetHashCode()));
            Assert.That(b, Is.EqualTo(a));
        });
    }

    public static IEnumerable PeriodTestCases()
    {
        yield return new TestCaseData(new Period(new CalDateTime(_nowTime)), new Period(new CalDateTime(_nowTime)))
            .SetName("Two identical CalDateTimes are equal");
    }

    [Test]
    public void PeriodListTests()
    {
        var startTimesA = new List<DateTime>
            {
                new DateTime(2017, 03, 02, 06, 00, 00),
                new DateTime(2017, 03, 03, 06, 00, 00),
                new DateTime(2017, 03, 06, 06, 00, 00),
                new DateTime(2017, 03, 07, 06, 00, 00),
                new DateTime(2017, 03, 08, 06, 00, 00),
                new DateTime(2017, 03, 09, 06, 00, 00),
                new DateTime(2017, 03, 10, 06, 00, 00),
                new DateTime(2017, 03, 13, 06, 00, 00),
                new DateTime(2017, 03, 14, 06, 00, 00),
                new DateTime(2017, 03, 17, 06, 00, 00),
                new DateTime(2017, 03, 20, 06, 00, 00),
                new DateTime(2017, 03, 21, 06, 00, 00),
                new DateTime(2017, 03, 22, 06, 00, 00),
                new DateTime(2017, 03, 23, 06, 00, 00),
                new DateTime(2017, 03, 24, 06, 00, 00),
                new DateTime(2017, 03, 27, 06, 00, 00),
                new DateTime(2017, 03, 28, 06, 00, 00),
                new DateTime(2017, 03, 29, 06, 00, 00),
                new DateTime(2017, 03, 30, 06, 00, 00),
                new DateTime(2017, 03, 31, 06, 00, 00),
                new DateTime(2017, 04, 03, 06, 00, 00),
                new DateTime(2017, 04, 05, 06, 00, 00),
                new DateTime(2017, 04, 06, 06, 00, 00),
                new DateTime(2017, 04, 07, 06, 00, 00),
                new DateTime(2017, 04, 10, 06, 00, 00),
                new DateTime(2017, 04, 11, 06, 00, 00),
                new DateTime(2017, 04, 12, 06, 00, 00),
                new DateTime(2017, 04, 13, 06, 00, 00),
                new DateTime(2017, 04, 17, 06, 00, 00),
                new DateTime(2017, 04, 18, 06, 00, 00),
                new DateTime(2017, 04, 19, 06, 00, 00),
                new DateTime(2017, 04, 20, 06, 00, 00),
                new DateTime(2017, 04, 21, 06, 00, 00),
                new DateTime(2017, 04, 24, 06, 00, 00),
                new DateTime(2017, 04, 25, 06, 00, 00),
                new DateTime(2017, 04, 27, 06, 00, 00),
                new DateTime(2017, 04, 28, 06, 00, 00),
                new DateTime(2017, 05, 01, 06, 00, 00),
            }
            .Select(dt => new Period(new CalDateTime(dt))).ToList();
        var a = new PeriodList();
        foreach (var period in startTimesA)
        {
            a.Add(period);
        }

        //Difference from A: first element became the second, and last element became the second-to-last element
        var startTimesB = new List<DateTime>
            {
                new DateTime(2017, 03, 03, 06, 00, 00),
                new DateTime(2017, 03, 02, 06, 00, 00),
                new DateTime(2017, 03, 06, 06, 00, 00),
                new DateTime(2017, 03, 07, 06, 00, 00),
                new DateTime(2017, 03, 08, 06, 00, 00),
                new DateTime(2017, 03, 09, 06, 00, 00),
                new DateTime(2017, 03, 10, 06, 00, 00),
                new DateTime(2017, 03, 13, 06, 00, 00),
                new DateTime(2017, 03, 14, 06, 00, 00),
                new DateTime(2017, 03, 17, 06, 00, 00),
                new DateTime(2017, 03, 20, 06, 00, 00),
                new DateTime(2017, 03, 21, 06, 00, 00),
                new DateTime(2017, 03, 22, 06, 00, 00),
                new DateTime(2017, 03, 23, 06, 00, 00),
                new DateTime(2017, 03, 24, 06, 00, 00),
                new DateTime(2017, 03, 27, 06, 00, 00),
                new DateTime(2017, 03, 28, 06, 00, 00),
                new DateTime(2017, 03, 29, 06, 00, 00),
                new DateTime(2017, 03, 30, 06, 00, 00),
                new DateTime(2017, 03, 31, 06, 00, 00),
                new DateTime(2017, 04, 03, 06, 00, 00),
                new DateTime(2017, 04, 05, 06, 00, 00),
                new DateTime(2017, 04, 06, 06, 00, 00),
                new DateTime(2017, 04, 07, 06, 00, 00),
                new DateTime(2017, 04, 10, 06, 00, 00),
                new DateTime(2017, 04, 11, 06, 00, 00),
                new DateTime(2017, 04, 12, 06, 00, 00),
                new DateTime(2017, 04, 13, 06, 00, 00),
                new DateTime(2017, 04, 17, 06, 00, 00),
                new DateTime(2017, 04, 18, 06, 00, 00),
                new DateTime(2017, 04, 19, 06, 00, 00),
                new DateTime(2017, 04, 20, 06, 00, 00),
                new DateTime(2017, 04, 21, 06, 00, 00),
                new DateTime(2017, 04, 24, 06, 00, 00),
                new DateTime(2017, 04, 25, 06, 00, 00),
                new DateTime(2017, 04, 27, 06, 00, 00),
                new DateTime(2017, 05, 01, 06, 00, 00),
                new DateTime(2017, 04, 28, 06, 00, 00),
            }
            .Select(dt => new Period(new CalDateTime(dt))).ToList();
        var b = new PeriodList();
        foreach (var period in startTimesB)
        {
            b.Add(period);
        }

        var collectionEqual = CollectionHelpers.Equals(a, b);
        Assert.Multiple(() =>
        {
            Assert.That(collectionEqual, Is.EqualTo(true));
            Assert.That(b.GetHashCode(), Is.EqualTo(a.GetHashCode()));
        });

        var listOfListA = new List<PeriodList> { a };
        var listOfListB = new List<PeriodList> { b };
        Assert.That(CollectionHelpers.Equals(listOfListA, listOfListB), Is.True);

        var aThenB = new List<PeriodList> { a, b };
        var bThenA = new List<PeriodList> { b, a };
        Assert.That(CollectionHelpers.Equals(aThenB, bThenA), Is.True);
    }

    [Test]
    public void CalDateTimeTests()
    {
        var nowLocal = DateTime.Now;
        var nowUtc = nowLocal.ToUniversalTime();

        var asLocal = new CalDateTime(nowLocal, "America/New_York");
        var asUtc = new CalDateTime(nowUtc, "UTC");

        Assert.That(asUtc, Is.Not.EqualTo(asLocal));
    }

    private void TestComparison(Func<CalDateTime, IDateTime, bool> calOp, Func<int?, int?, bool> intOp)
    {
        int? intSome = 1;
        int? intGreater = 2;

        var dtSome = new CalDateTime(2018, 1, 1);
        var dtGreater = new CalDateTime(2019, 1, 1);

        Assert.Multiple(() =>
        {
            Assert.That(calOp(null, null), Is.EqualTo(intOp(null, null)));
            Assert.That(calOp(null, dtSome), Is.EqualTo(intOp(null, intSome)));
            Assert.That(calOp(dtSome, null), Is.EqualTo(intOp(intSome, null)));
            Assert.That(calOp(dtSome, dtSome), Is.EqualTo(intOp(intSome, intSome)));
            Assert.That(calOp(dtSome, dtGreater), Is.EqualTo(intOp(intSome, intGreater)));
            Assert.That(calOp(dtGreater, dtSome), Is.EqualTo(intOp(intGreater, intSome)));
        });
    }

    [Test]
    public void CalDateTimeComparisonOperatorTests()
    {
        // Assumption: comparison operators on CalDateTime are expected to
        // work like operators on Nullable<int>.
        TestComparison((dt1, dt2) => dt1 == dt2, (i1, i2) => i1 == i2);
        TestComparison((dt1, dt2) => dt1 != dt2, (i1, i2) => i1 != i2);
        TestComparison((dt1, dt2) => dt1 > dt2, (i1, i2) => i1 > i2);
        TestComparison((dt1, dt2) => dt1 >= dt2, (i1, i2) => i1 >= i2);
        TestComparison((dt1, dt2) => dt1 < dt2, (i1, i2) => i1 < i2);
        TestComparison((dt1, dt2) => dt1 <= dt2, (i1, i2) => i1 <= i2);
    }
}