using System;
using System.Collections.Generic;
using ical.NET.Collections;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Interfaces.Components
{
    public interface IUniqueComponent : ICalendarComponent
    {
        event EventHandler<ObjectEventArgs<string, string>> UidChanged;
        string Uid { get; set; }

        IList<IAttendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DtStamp { get; set; }
        IOrganizer Organizer { get; set; }
        IList<IRequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}