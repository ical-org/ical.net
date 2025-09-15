using System;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using NUnit.Framework;

namespace Ical.Net.Tests;

[TestFixture]
public class OccurrenceIssue
{
    [Test, Explicit]
    public void MissingOccurrences()
    {
        var cal = Calendar.Load("""
            BEGIN:VCALENDAR
            PRODID:-//Google Inc//Google Calendar 70.9054//EN
            VERSION:2.0
            CALSCALE:GREGORIAN
            METHOD:PUBLISH
            X-WR-CALNAME:Non-Critical Support Roster
            X-WR-TIMEZONE:UTC
            BEGIN:VEVENT
            DTSTART;VALUE=DATE:20251103
            DTEND;VALUE=DATE:20251124
            RRULE:FREQ=WEEKLY;WKST=MO;INTERVAL=48;BYDAY=MO
            DTSTAMP:20250912T100327Z
            UID:00p0ja7t2446ja22sl02ah2uun@google.com
            CREATED:20250214T230308Z
            LAST-MODIFIED:20250409T173619Z
            SEQUENCE:1
            STATUS:CONFIRMED
            SUMMARY:QWERTY
            TRANSP:TRANSPARENT
            END:VEVENT
            BEGIN:VEVENT
            DTSTART;VALUE=DATE:20251103
            DTEND;VALUE=DATE:20251124
            DTSTAMP:20250912T100327Z
            UID:00p0ja7t2446ja22sl02ah2uun@google.com
            RECURRENCE-ID;VALUE=DATE:20251103
            CREATED:20250214T230308Z
            LAST-MODIFIED:20250428T170953Z
            SEQUENCE:1
            STATUS:CONFIRMED
            SUMMARY:QWERTY
            TRANSP:TRANSPARENT
            END:VEVENT
            END:VCALENDAR
            """)!;

        Console.WriteLine("Events:");
        foreach (var e in cal.Events.OrderBy(x=>x.Start))
        {
            Console.WriteLine($"\t{e.Uid.Substring(0,7)} {e.Start.Value} {e.End.Value} {e.Summary}");
        }

        var dt = new CalDateTime(DateOnly.FromDateTime(new DateTime(2026,1,1)));
        var occurrences = cal
            .GetOccurrences<CalendarEvent>()
            .TakeWhile(p => p.Period.StartTime <= dt);

        Console.WriteLine("Occurrences:");
        foreach (var o in occurrences)
        {
            var e = (CalendarEvent)o.Source;
            Console.WriteLine($"\t{e.Uid.Substring(0,7)} {o.Period.StartTime.Value} {o.Period.EffectiveEndTime.Value} {e.Summary}");
        }

        Assert.That(occurrences, Is.Not.Empty);
    }
}
