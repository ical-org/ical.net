using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using System.Collections.Generic;

namespace Ical.Net
{
    public interface IGetFreeBusy
    {
        FreeBusy GetFreeBusy(FreeBusy freeBusyRequest);
        FreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive);
        FreeBusy GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, IDateTime fromInclusive, IDateTime toExclusive);
    }
}