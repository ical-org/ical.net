//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using Ical.Net.Utility;

namespace Ical.Net.CalendarComponents;

public class FreeBusy : UniqueComponent, IMergeable
{
    public static FreeBusy? Create(ICalendarObject obj, FreeBusy freeBusyRequest, EvaluationOptions? options = null)
    {
        if ((obj is not IGetOccurrencesTyped occ))
        {
            return null;
        }

        var occurrences = occ.GetOccurrences<CalendarEvent>(freeBusyRequest.Start, options)
            .TakeWhile(p => (freeBusyRequest.End == null) || (p.Period.StartTime < freeBusyRequest.End));

        var contacts = new List<string>();
        var isFilteredByAttendees = false;

        if (freeBusyRequest.Attendees.Count > 0)
        {
            isFilteredByAttendees = true;
            var attendees = freeBusyRequest.Attendees
                .Where(a => a.Value != null)
                .Select(a => a.Value!.OriginalString.Trim());
            contacts.AddRange(attendees);
        }

        var fb = freeBusyRequest;
        fb.Uid = Guid.NewGuid().ToString();
        fb.Entries.Clear();
        fb.DtStamp = CalDateTime.Now;

        foreach (var o in occurrences)
        {
            if (o.Source is not IUniqueComponent uc)
                continue;

            var evt = uc as CalendarEvent;
            var accepted = false;
            var type = FreeBusyStatus.Busy;

            // We only accept events, and only "opaque" events.
            if (evt != null && evt.Transparency != TransparencyType.Transparent)
            {
                accepted = true;
            }

            // If the result is filtered by attendees, then
            // we won't accept it until we find an event
            // that is being attended by this person.
            if (accepted && isFilteredByAttendees)
            {
                accepted = false;

                var participatingAttendeeQuery = uc.Attendees
                    .Where(attendee =>
                        attendee.Value != null
                        && attendee.ParticipationStatus != null
                        && contacts.Contains(attendee.Value.OriginalString.Trim()))
                    .Select(pa => pa.ParticipationStatus!.ToUpperInvariant());

                foreach (var participatingAttendee in participatingAttendeeQuery)
                {
                    switch (participatingAttendee)
                    {
                        case EventParticipationStatus.Tentative:
                            accepted = true;
                            type = FreeBusyStatus.BusyTentative;
                            break;
                        case EventParticipationStatus.Accepted:
                            accepted = true;
                            type = FreeBusyStatus.Busy;
                            break;
                    }
                }
            }

            if (accepted)
            {
                // If the entry was accepted, add it to our list!
                fb.Entries.Add(new FreeBusyEntry(o.Period, type));
            }
        }

        return fb;
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

    public virtual FreeBusyStatus GetFreeBusyStatus(Period? period)
    {
        var status = FreeBusyStatus.Free;
        if (period == null)
        {
            return status;
        }

        foreach (var fbe in Entries.Where(fbe => fbe.CollidesWith(period) && status < fbe.Status))
        {
            status = fbe.Status;
        }
        return status;
    }

    public virtual FreeBusyStatus GetFreeBusyStatus(CalDateTime? dt)
    {
        var status = FreeBusyStatus.Free;
        if (dt == null)
        {
            return status;
        }

        foreach (var fbe in Entries.Where(fbe => fbe.Contains(dt) && status < fbe.Status))
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
