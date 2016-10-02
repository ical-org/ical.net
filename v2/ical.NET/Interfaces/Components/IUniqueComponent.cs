using System;
using System.Collections.Generic;
using ical.net.DataTypes;
using ical.net.Interfaces.DataTypes;

namespace ical.net.Interfaces.Components
{
    public interface IUniqueComponent : ICalendarComponent
    {
        string Uid { get; set; }

        IList<Attendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DtStamp { get; set; }
        Organizer Organizer { get; set; }
        IList<RequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}