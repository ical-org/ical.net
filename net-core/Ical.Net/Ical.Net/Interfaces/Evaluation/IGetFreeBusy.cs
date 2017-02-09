using System.Collections.Generic;
using Ical.Net.DataTypes;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Evaluation
{
    public interface IGetFreeBusy
    {
        FreeBusy GetFreeBusy(FreeBusy freeBusyRequest);
        FreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive);
        FreeBusy GetFreeBusy(Organizer organizer, IEnumerable<Attendee> contacts, IDateTime fromInclusive, IDateTime toExclusive);
    }
}