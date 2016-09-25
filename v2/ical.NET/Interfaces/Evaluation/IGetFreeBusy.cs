using ical.net.Interfaces.Components;
using ical.net.Interfaces.DataTypes;

namespace ical.net.Interfaces.Evaluation
{
    public interface IGetFreeBusy
    {
        IFreeBusy GetFreeBusy(IFreeBusy freeBusyRequest);
        IFreeBusy GetFreeBusy(IDateTime fromInclusive, IDateTime toExclusive);
        IFreeBusy GetFreeBusy(IOrganizer organizer, IAttendee[] contacts, IDateTime fromInclusive, IDateTime toExclusive);
    }
}