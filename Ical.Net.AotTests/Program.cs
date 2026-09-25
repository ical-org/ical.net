//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.AotTests;
using Ical.Net.Serialization;
using NodaTime;

// Smoke test for the library published with PublishAot and TrimMode=full, deliberately without a
// TrimmerRootAssembly or an ILLink descriptor. Asserts on values, not on the exit code.

const string Tz = "America/New_York";

var serializer = new CalendarSerializer();

Console.WriteLine("# ical.net NativeAOT smoke test");
Console.WriteLine();
Console.WriteLine("## parsed-uid-survives");

// RECURRENCE-ID matching is keyed on (Uid, Instant), so the parsed UID has to survive verbatim for
// the override to suppress the occurrence it replaces. Assert the UID itself, not just the
// occurrence count: a wrong UID still yields correct times, so a count-only check can pass by luck.
var overridden = Smoke.Load(
    "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//ical.net//aot//EN\r\n"
    + "BEGIN:VEVENT\r\nUID:evt1\r\n"
    + $"DTSTART;TZID={Tz}:20260316T090000\r\nDTEND;TZID={Tz}:20260316T100000\r\n"
    + "RRULE:FREQ=DAILY;COUNT=3\r\nSUMMARY:Recurring\r\nEND:VEVENT\r\n"
    + "BEGIN:VEVENT\r\nUID:evt1\r\n"
    + $"RECURRENCE-ID;TZID={Tz}:20260317T090000\r\n"
    + $"DTSTART;TZID={Tz}:20260317T140000\r\nDTEND;TZID={Tz}:20260317T150000\r\n"
    + "SUMMARY:Moved\r\nEND:VEVENT\r\nEND:VCALENDAR\r\n");

Smoke.Check("event.uids", overridden.Events.Select(e => e.Uid).OrderBy(u => u, StringComparer.Ordinal),
    new[] { "evt1", "evt1" });

// 3 from the RRULE, with the 17th replaced - not appended - by the override. Resolving them at all
// requires NodaTime's embedded TZDB.
var occurrences = overridden
    .GetOccurrences(CalendarTimeZoneProviders.TzdbWithAliases[Tz], Instant.FromUtc(2026, 3, 1, 0, 0))
    .TakeWhileBefore(Instant.FromUtc(2026, 4, 1, 0, 0))
    .OrderBy(o => o.Start.ToInstant())
    .ToList();

Smoke.Check("occurrence.count", occurrences.Count, 3);
Smoke.Check("occurrence.summaries", occurrences.Select(o => (o.Source as CalendarEvent)?.Summary ?? "?"),
    new[] { "Recurring", "Moved", "Recurring" });
Smoke.Check("occurrence.overridden.start",
    occurrences[1].Start.LocalDateTime.ToString("uuuu-MM-dd HH:mm", null), "2026-03-17 14:00");

Console.WriteLine();
Console.WriteLine("## serialize-round-trip");

// Covers most of the property pipeline: the DataTypeMapper, the serializer factory, the
// CreateTargetInstance() overrides that have a property mapping, GenericListSerializer, and all
// four service lookups.
var source = "BEGIN:VCALENDAR\r\nVERSION:2.0\r\nPRODID:-//ical.net//aot//EN\r\n"
    + "BEGIN:VEVENT\r\nUID:round-trip-1\r\nDTSTAMP:20260101T000000Z\r\n"
    + $"DTSTART;TZID={Tz}:20260105T090000\r\nDTEND;TZID={Tz}:20260105T100000\r\n"
    + "RRULE:FREQ=WEEKLY;BYDAY=MO,WE;COUNT=4\r\n"
    + $"EXDATE;TZID={Tz}:20260107T090000\r\n"
    + "SUMMARY:Everything\r\nCATEGORIES:one,two,three\r\nPRIORITY:5\r\nSTATUS:CONFIRMED\r\n"
    + "GEO:48.210033;16.363449\r\nURL:https://example.com/\r\n"
    + "ORGANIZER;CN=The Organizer:mailto:organizer@example.com\r\n"
    + "ATTENDEE;CN=An Attendee;RSVP=TRUE:mailto:attendee@example.com\r\n"
    + "ATTACH;FMTTYPE=text/plain:https://example.com/file.txt\r\n"
    + "REQUEST-STATUS:2.0;Success\r\n"
    + "BEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:Reminder\r\nTRIGGER:-PT15M\r\nEND:VALARM\r\n"
    + "END:VEVENT\r\nEND:VCALENDAR\r\n";

var once = Smoke.Serialize(serializer, Smoke.Load(source));
var twice = Smoke.Serialize(serializer, Smoke.Load(once));

Smoke.Check("roundtrip.stable", twice == once, true);

var evt = Smoke.Load(once).Events.First();

Smoke.Check("roundtrip.uid", evt.Uid, "round-trip-1");
Smoke.Check("roundtrip.categories", evt.Categories.OrderBy(c => c, StringComparer.Ordinal),
    new[] { "one", "three", "two" });
Smoke.Check("roundtrip.priority", evt.Priority, 5);
Smoke.Check("roundtrip.status", evt.Status, "CONFIRMED");
Smoke.Check("roundtrip.geo", $"{evt.GeographicLocation?.Latitude};{evt.GeographicLocation?.Longitude}",
    "48.210033;16.363449");
Smoke.Check("roundtrip.url", evt.Url?.ToString(), "https://example.com/");
Smoke.Check("roundtrip.organizer", evt.Organizer?.CommonName, "The Organizer");
Smoke.Check("roundtrip.attendee", evt.Attendees.FirstOrDefault()?.CommonName, "An Attendee");
Smoke.Check("roundtrip.attachment", evt.Attachments.FirstOrDefault()?.Uri?.ToString(),
    "https://example.com/file.txt");
Smoke.Check("roundtrip.requestStatus", evt.RequestStatuses.FirstOrDefault()?.Description, "Success");
Smoke.Check("roundtrip.alarm.trigger", evt.Alarms.FirstOrDefault()?.Trigger?.Duration?.ToString(), "-PT15M");
Smoke.Check("roundtrip.rrule", evt.RecurrenceRule?.ToString(), "FREQ=WEEKLY;COUNT=4;BYDAY=MO,WE");

// A copy must land on the exact runtime type. The other 27 copyable types are covered by
// CopyAllTypesTests.
Smoke.Check("copy.type", evt.Copy<object>()?.GetType().FullName, "Ical.Net.CalendarComponents.CalendarEvent");

Console.WriteLine();
Console.WriteLine($"## failures = {Smoke.Failures}");
return Smoke.ExitCode;

namespace Ical.Net.AotTests
{
    /// <summary>Assertions and the failure tally for the smoke test.</summary>
    internal static class Smoke
    {
        private static int _failures;

        /// <summary>Number of assertions that did not hold.</summary>
        internal static int Failures => _failures;

        /// <summary>0 when every assertion held, 1 otherwise.</summary>
        internal static int ExitCode => _failures == 0 ? 0 : 1;

        internal static void Check(string what, object? actual, object? expected)
        {
            var a = Format(actual);
            var e = Format(expected);
            Console.WriteLine($"{what} = {a}");
            if (!string.Equals(a, e, StringComparison.Ordinal))
            {
                _failures++;
                Console.Error.WriteLine($"FAIL {what}: expected <{e}> but was <{a}>");
            }
        }

        /// <summary>A null here is a fixture bug, not a result, so fail loudly rather than dereference.</summary>
        internal static Calendar Load(string ics)
            => Calendar.Load(ics) ?? throw new InvalidOperationException("Calendar.Load returned null.");

        internal static string Serialize(CalendarSerializer serializer, Calendar calendar)
            => serializer.SerializeToString(calendar) ?? throw new InvalidOperationException("SerializeToString returned null.");

        private static string Format(object? value) => value switch
        {
            null => "<null>",
            string s => s,
            bool b => b ? "true" : "false",
            IEnumerable<string> items => string.Join(",", items),
            _ => value.ToString() ?? "<null>",
        };
    }
}
