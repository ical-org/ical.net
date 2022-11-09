using System;
using System.Collections.Generic;
using Ical.Net.DataTypes;

namespace Ical.Net.CalendarComponents
{
    /// <summary> Calendar-Component with a <see cref="Uid"/> </summary>
    public interface IUniqueComponent : ICalendarComponent
    {
        /// <summary> Unique identifier for this </summary>
        string Uid { get; set; }

        IList<Attendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DtStamp { get; set; }
        Organizer Organizer { get; set; }
        IList<RequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}