//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Utility;
using NodaTime;
using Period = Ical.Net.DataTypes.Period;

namespace Ical.Net.CalendarComponents;

public class FreeBusy : UniqueComponent, IMergeable
{
    public static FreeBusy? Create(ICalendarObject obj, DateTimeZone timeZone, FreeBusy freeBusyRequest, EvaluationOptions? options = null)
    {
        if (obj is not IGetOccurrencesTyped occ)
        {
            return null;
        }

        var occurrences = occ.GetOccurrences<CalendarEvent>(timeZone, freeBusyRequest.Start?.ToZonedDateTime(timeZone).ToInstant(), options)
            .TakeWhile(p => (freeBusyRequest.End == null) || (p.Start.ToInstant() < freeBusyRequest.End.ToInstant()));

        var attendeeContacts = BuildAttendeeContacts(freeBusyRequest);
        var isFilteredByAttendees = attendeeContacts.Count > 0;

        var fb = freeBusyRequest;
        fb.Uid = Guid.NewGuid().ToString();
        fb.Entries.Clear();
        fb.DtStamp = CalDateTime.Now;

        foreach (var o in occurrences)
        {
            // Ignore transparent events. Only opaque events are considered as busy time.
            if (o.Source is not CalendarEvent evt || evt.Transparency == TransparencyType.Transparent)
            {
                continue;
            }

            var status = isFilteredByAttendees
                ? GetStatusByAttendees(evt, attendeeContacts)
                : FreeBusyStatus.Busy;

            if (status == null)
            {
                continue;
            }

            fb.Entries.Add(new FreeBusyEntry(new Period(o.Start.ToInstant(), o.End.ToInstant()), status.Value));
        }

        return fb;
    }

    private static HashSet<string> BuildAttendeeContacts(FreeBusy freeBusyRequest)
    {
        var contacts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var attendee in freeBusyRequest.Attendees)
        {
            var value = attendee.Value?.OriginalString.Trim();
            if (!string.IsNullOrEmpty(value))
            {
                contacts.Add(value);
            }
        }
        return contacts;
    }

    private static FreeBusyStatus? GetStatusByAttendees(CalendarEvent evt, HashSet<string> contacts)
    {
        foreach (var attendee in evt.Attendees)
        {
            var trimmed = attendee.Value?.OriginalString.Trim();
            if (string.IsNullOrEmpty(trimmed) || attendee.ParticipationStatus == null || !contacts.Contains(trimmed))
            {
                continue;
            }

            switch (attendee.ParticipationStatus.ToUpperInvariant())
            {
                case EventParticipationStatus.Tentative:
                    return FreeBusyStatus.BusyTentative;
                case EventParticipationStatus.Accepted:
                    return FreeBusyStatus.Busy;
            }
        }

        return null;
    }

    public static FreeBusy CreateRequest(CalDateTime fromInclusive, CalDateTime toExclusive, Organizer? organizer, IEnumerable<Attendee>? contacts)
    {
        var fb = new FreeBusy
        {
            DtStamp = CalDateTime.Now,
            DtStart = fromInclusive,
            DtEnd = toExclusive
        };

        if (organizer != null)
        {
            fb.Organizer = organizer;
        }

        if (contacts == null)
        {
            return fb;
        }
        foreach (var attendee in contacts)
        {
            fb.Attendees.Add(attendee);
        }

        return fb;
    }

    public FreeBusy()
    {
        Name = Components.Freebusy;
    }

    public virtual IList<FreeBusyEntry> Entries
    {
        get => Properties.GetMany<FreeBusyEntry>("FREEBUSY");
        set => Properties.Set("FREEBUSY", value);
    }

    public virtual CalDateTime? DtStart
    {
        get => Properties.Get<CalDateTime>("DTSTART");
        set => Properties.Set("DTSTART", value);
    }

    public virtual CalDateTime? DtEnd
    {
        get => Properties.Get<CalDateTime>("DTEND");
        set => Properties.Set("DTEND", value);
    }

    public virtual CalDateTime? Start
    {
        get => Properties.Get<CalDateTime>("DTSTART");
        set => Properties.Set("DTSTART", value);
    }

    public virtual CalDateTime? End
    {
        get => Properties.Get<CalDateTime>("DTEND");
        set => Properties.Set("DTEND", value);
    }

    /// <summary>
    /// All DATE-TIME values must be in the UTC time zone.
    /// </summary>
    /// <param name="period"></param>
    /// <returns></returns>
    public virtual FreeBusyStatus GetFreeBusyStatus(DataTypes.Period? period)
    {
        var status = FreeBusyStatus.Free;
        if (period == null)
        {
            return status;
        }

        if (period.StartTime.IsFloating)
        {
            throw new ArgumentException("Period start time must be in UTC");
        }

        if (period.EndTime is { } endTime && endTime.IsFloating)
        {
            throw new ArgumentException("Period end time must be in UTC");
        }

        foreach (var fbe in Entries.Where(fbe => fbe.CollidesWith(period) && status < fbe.Status))
        {
            status = fbe.Status;
        }
        return status;
    }

    /// <summary>
    /// Value must be in the UTC time zone.
    /// </summary>
    /// <param name="dt"></param>
    /// <returns></returns>
    public virtual FreeBusyStatus GetFreeBusyStatus(CalDateTime? dt)
    {
        return GetFreeBusyStatus(dt?.ToInstant());
    }

    public virtual FreeBusyStatus GetFreeBusyStatus(Instant? dt)
    {
        var status = FreeBusyStatus.Free;
        if (dt == null)
        {
            return status;
        }

        // FREEBUSY entries MUST always be UTC time, so compare as instant
        var matches = Entries.Where(fbe => status < fbe.Status && fbe.Contains(dt.Value));

        foreach (var fbe in matches)
        {
            status = fbe.Status;
        }
        return status;
    }

    public virtual void MergeWith(IMergeable obj)
    {
        if (obj is not FreeBusy fb)
        {
            return;
        }

        Entries.AddRange(fb.Entries.Where(entry => !Entries.Contains(entry)));
    }
}
