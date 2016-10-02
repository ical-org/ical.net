using System.Collections.Generic;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;

namespace ical.net.Interfaces.Evaluation
{
    public interface IGetFreeBusy
    {
        FreeBusy GetFreeBusy(FreeBusy freeBusyRequest);
        FreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive);
        FreeBusy GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, IDateTime fromInclusive, IDateTime toExclusive);
    }
}