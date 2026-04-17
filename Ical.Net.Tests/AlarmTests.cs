//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Serialization;
using NodaTime;
using NUnit.Framework;
using Duration = Ical.Net.DataTypes.Duration;
using Period = Ical.Net.DataTypes.Period;

namespace Ical.Net.Tests;

[TestFixture]
public class AlarmTests
{
    #region Examples from RFC 5545

    [Test]
    public void ExactTimeAlarmWithRepeat()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(1997, 3, 18)
        };

        var valarm = """
            BEGIN:VALARM
            TRIGGER;VALUE=DATE-TIME:19970317T133000Z
            REPEAT:4
            DURATION:PT15M
            ACTION:AUDIO
            ATTACH;FMTTYPE=audio/basic:ftp://example.com/pub/
                sounds/bell-01.aud
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        e.Alarms.Add(alarm);

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(1997, 3, 17, 13, 30, 0),
            Instant.FromUtc(1997, 3, 17, 13, 45, 0),
            Instant.FromUtc(1997, 3, 17, 14,  0, 0),
            Instant.FromUtc(1997, 3, 17, 14, 15, 0),
            Instant.FromUtc(1997, 3, 17, 14, 30, 0),
        }));
    }

    [Test]
    public void RelativeAlarmBeforeStart()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(1997, 3, 17, 13, 30, 0, CalDateTime.UtcTzId),
            Duration = new Duration(hours: 1)
        };

        var valarm = """
            BEGIN:VALARM
            TRIGGER:-PT30M
            REPEAT:2
            DURATION:PT15M
            ACTION:AUDIO
            ATTACH;FMTTYPE=audio/basic:ftp://example.com/pub/
                sounds/bell-01.aud
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        e.Alarms.Add(alarm);

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(1997, 3, 17, 13,  0, 0),
            Instant.FromUtc(1997, 3, 17, 13, 15, 0),
            Instant.FromUtc(1997, 3, 17, 13, 30, 0),
        }));
    }

    [Test]
    public void RelativeAlarmAfterEnd()
    {
        CalendarEvent e = new()
        {
            Start    = new CalDateTime(1997, 3, 17, 13, 30, 0, CalDateTime.UtcTzId),
            Duration = new Duration(hours: 1)
        };

        var valarm = """
            BEGIN:VALARM
            TRIGGER;RELATED=END:PT5M
            REPEAT:2
            DURATION:PT5M
            ACTION:AUDIO
            ATTACH;FMTTYPE=audio/basic:ftp://example.com/pub/
                sounds/bell-01.aud
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        e.Alarms.Add(alarm);

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(1997, 3, 17, 14, 35, 0),
            Instant.FromUtc(1997, 3, 17, 14, 40, 0),
            Instant.FromUtc(1997, 3, 17, 14, 45, 0),
        }));
    }

    [Test]
    public void EmailAlarmBeforeEnd()
    {
        var valarm = """
            BEGIN:VALARM
            TRIGGER;RELATED=END:-P2D
            ACTION:EMAIL
            END:VALARM
            """;

        var alarm = SimpleDeserializer.Default
            .Deserialize(new StringReader(valarm))
            .Cast<Alarm>()
            .Single();

        CalendarEvent e = new()
        {
            Start    = new CalDateTime(1998, 1,  1, 0, 0, 0, CalDateTime.UtcTzId),
            Duration = new Duration(days: 5)
        };
        e.Alarms.Add(alarm);

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        // end = Jan 6, trigger = end - 2 days = Jan 4
        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(1998, 1, 4, 0, 0, 0)
        }));
    }

    #endregion

    #region Recurring component alarms

    [Test]
    public void RecurringAlarm_RelativeStart_ReturnsAlarmsForEachOccurrence()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Weekly, 1) { Count = 4 }
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(minutes: -15))
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  7, 8, 45, 0),
            Instant.FromUtc(2026, 4, 14, 8, 45, 0),
            Instant.FromUtc(2026, 4, 21, 8, 45, 0),
            Instant.FromUtc(2026, 4, 28, 8, 45, 0),
        }));
    }

    [Test]
    public void RecurringAlarm_WithRepeatDuration_ProducesRepetitions()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Weekly, 1) { Count = 2 }
        };

        e.Alarms.Add(new Alarm
        {
            Trigger  = new Trigger(new Duration(minutes: -15)),
            Repeat   = 1,
            Duration = new Duration(minutes: 5)
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  7, 8, 45, 0),
            Instant.FromUtc(2026, 4,  7, 8, 50, 0),
            Instant.FromUtc(2026, 4, 14, 8, 45, 0),
            Instant.FromUtc(2026, 4, 14, 8, 50, 0),
        }));
    }

    [Test]
    public void RecurringAlarm_MultipleAlarms_ReturnsMergedOrdered()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Weekly, 1) { Count = 2 }
        };

        e.Alarms.Add(new Alarm { Trigger = new Trigger(new Duration(minutes: -30)) });
        e.Alarms.Add(new Alarm { Trigger = new Trigger(new Duration(minutes: -10)) });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  7, 8, 30, 0),
            Instant.FromUtc(2026, 4,  7, 8, 50, 0),
            Instant.FromUtc(2026, 4, 14, 8, 30, 0),
            Instant.FromUtc(2026, 4, 14, 8, 50, 0),
        }));
    }

    [Test]
    public void GetAlarmOccurrences_EndTime_BoundsResults()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Weekly, 1)
        };

        e.Alarms.Add(new Alarm { Trigger = new Trigger(new Duration(minutes: -15)) });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc,
                endTime: Instant.FromUtc(2026, 4, 22, 0, 0, 0))
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  7, 8, 45, 0),
            Instant.FromUtc(2026, 4, 14, 8, 45, 0),
            Instant.FromUtc(2026, 4, 21, 8, 45, 0),
        }));
    }

    [Test]
    public void RecurringAlarm_RelatedEnd_ReturnsAlarmsRelativeToOccurrenceEnd()
    {
        CalendarEvent e = new()
        {
            Start    = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId),
            Duration = new Duration(hours: 1),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Weekly, 1) { Count = 3 }
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(minutes: -30))
            {
                Related = TriggerRelation.End
            }
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  7, 9, 30, 0),
            Instant.FromUtc(2026, 4, 14, 9, 30, 0),
            Instant.FromUtc(2026, 4, 21, 9, 30, 0),
        }));
    }

    [Test]
    public void RecurringAlarm_RelatedEnd_Indefinite()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId),
            Duration = new Duration(hours: 1),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Daily)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(minutes: -30))
            {
                Related = TriggerRelation.End
            }
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .Take(3)
            .ToList();

        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  7, 9, 30, 0),
            Instant.FromUtc(2026, 4, 14, 9, 30, 0),
            Instant.FromUtc(2026, 4, 21, 9, 30, 0),
        }));
    }

    [Test]
    public void AlarmWithRepeatButNoDuration_ShouldNotProduceDuplicates()
    {
        // RFC 5545 §3.8.6.2: REPEAT and DURATION must appear together.
        // When Duration is absent, GetFireTimes yields only the base fire time.
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(minutes: -15)),
            Repeat  = 3
            // Duration intentionally omitted
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0], Is.EqualTo(Instant.FromUtc(2026, 4, 7, 8, 45, 0)));
    }

    [Test]
    public void ComponentWithNoAlarms_GetAlarmOccurrences_IsEmpty()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId)
        };

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc).ToList();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void GetAlarmOccurrences_StartTime_FiltersAlarmFireTimes()
    {
        // startTime is a lower bound on alarm FIRE TIMES, not component start times.
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(days: -1)) // fires Apr 6 for Apr 7 occurrence
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc,
                startTime: Instant.FromUtc(2026, 4, 7, 9, 0, 0),  // Apr 7 09:00 – after alarm fire time
                endTime:   Instant.FromUtc(2026, 4, 8, 9, 0, 0))
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void RDatePeriods_TriggerRelatedEnd_AlarmsOrderedByEndNotStart()
    {
        // Three occurrences (DTSTART + 2 RDATEs). Ordered by start: DTSTART, A, B.
        // RDATE A has a longer duration than RDATE B, so A ends after B despite starting before it.
        // With TRIGGER;RELATED=END:-PT2H the alarm order follows end times, not start times.
        CalendarEvent e = new()
        {
            Start    = new CalDateTime(2026, 4, 1, 9, 0, 0, CalDateTime.UtcTzId),
            Duration = new Duration(hours: 1)   // DTSTART ends Apr 1 10:00
        };

        // RDATE A: starts Apr 2, ends Apr 10 (8 days)
        e.RecurrenceDates.Add(new Period(
            new CalDateTime(2026, 4, 2, 9, 0, 0, CalDateTime.UtcTzId),
            new CalDateTime(2026, 4, 10, 9, 0, 0, CalDateTime.UtcTzId)));

        // RDATE B: starts Apr 5, ends Apr 7 (2 days) — starts after A but ends before A
        e.RecurrenceDates.Add(new Period(
            new CalDateTime(2026, 4, 5, 9, 0, 0, CalDateTime.UtcTzId),
            new CalDateTime(2026, 4, 7, 9, 0, 0, CalDateTime.UtcTzId)));

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(hours: -2))
            {
                Related = TriggerRelation.End
            }
        });

        var results = e.GetAlarmOccurrences(DateTimeZone.Utc)
            .Select(x => x.Start.ToInstant())
            .ToList();

        // Alarm fires 2 hours before each occurrence END:
        //   DTSTART => end Apr 1  10:00 → alarm Apr 1  08:00
        //   RDATE B => end Apr 7  09:00 → alarm Apr 7  07:00  (starts later than A, fires earlier)
        //   RDATE A => end Apr 10 09:00 → alarm Apr 10 07:00  (starts earlier than B, fires later)
        Assert.That(results, Is.EqualTo(new[]
        {
            Instant.FromUtc(2026, 4,  1, 8, 0, 0),
            Instant.FromUtc(2026, 4,  7, 7, 0, 0),
            Instant.FromUtc(2026, 4, 10, 7, 0, 0),
        }));
    }

    [Test]
    public void AbsoluteTrigger_RecurringEventWithoutEnd_ProducesSingleAlarm()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, "UTC"),
            RecurrenceRule = new(FrequencyType.Weekly)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger()
            {
                DateTime = new CalDateTime(2026, 4, 6, 9, 0, 0, "UTC")
            }
        });

        var results = e.GetAlarmOccurrences(
                DateTimeZone.Utc,
                Instant.FromUtc(2026, 4, 5, 9, 0),
                Instant.FromUtc(2026, 5, 5, 9, 0))
            .Select(x => x.Start.ToInstant())
            .ToList();

        var expectedAlarms = new[]
        {
            Instant.FromUtc(2026, 4, 6, 9, 0)
        };

        Assert.That(results, Is.EquivalentTo(expectedAlarms));
    }

    [Test]
    public void AbsoluteTrigger_RecurringEventWithoutEnd_IsEmptyAfterTriggerDate()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, "UTC"),
            RecurrenceRule = new(FrequencyType.Weekly)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger()
            {
                DateTime = new CalDateTime(2026, 4, 6, 9, 0, 0, "UTC")
            }
        });

        var results = e.GetAlarmOccurrences(
                DateTimeZone.Utc,
                Instant.FromUtc(2026, 4, 7, 9, 0),
                Instant.FromUtc(2026, 4, 8, 9, 0))
            .Select(x => x.Start.ToInstant())
            .ToList();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void MultipleRelativeAlarmsWithRecurringEvent()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Daily, 2)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(days: -1))
        });

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(days: 2))
        });

        var results = e.GetAlarmOccurrences(
                DateTimeZone.Utc,
                Instant.FromUtc(2026, 4, 7, 0, 0),
                Instant.FromUtc(2026, 4, 12, 0, 0))
            .Select(x => x.Start.ToInstant())
            .ToList();

        var expectedAlarms = new[]
        {
            Instant.FromUtc(2026, 4, 8, 0, 0),
            Instant.FromUtc(2026, 4, 9, 0, 0),
            Instant.FromUtc(2026, 4, 10, 0, 0),
            Instant.FromUtc(2026, 4, 11, 0, 0),
        };

        Assert.That(results, Is.EquivalentTo(expectedAlarms));
    }

    [Test]
    public void RelativeAndAbsoluteAlarmsWithRecurringEvent()
    {
        CalendarEvent e = new()
        {
            Start = new CalDateTime(2026, 4, 7, 9, 0, 0, "UTC"),
            RecurrenceRule = new RecurrenceRule(FrequencyType.Weekly)
        };

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger(new Duration(days: -1))
        });

        e.Alarms.Add(new Alarm
        {
            Trigger = new Trigger()
            {
                DateTime = new CalDateTime(2026, 4, 15, 9, 0, 0, "UTC")
            }
        });

        var results = e.GetAlarmOccurrences(
                DateTimeZone.Utc,
                Instant.FromUtc(2026, 4, 6, 9, 0),
                Instant.FromUtc(2026, 4, 21, 9, 0))
            .Select(x => x.Start.ToInstant())
            .ToList();

        var expectedAlarms = new[]
        {
            Instant.FromUtc(2026, 4, 6, 9, 0),
            Instant.FromUtc(2026, 4, 13, 9, 0),
            Instant.FromUtc(2026, 4, 15, 9, 0),
            Instant.FromUtc(2026, 4, 20, 9, 0),
        };

        Assert.That(results, Is.EquivalentTo(expectedAlarms));
    }

    #endregion
}
