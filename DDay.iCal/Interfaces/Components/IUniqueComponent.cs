using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DDay.Collections;

namespace DDay.iCal
{
    public interface IUniqueComponent :
        ICalendarComponent
    {
        event EventHandler<ObjectEventArgs<string, string>> UIDChanged;
        string UID { get; set; }

        IList<IAttendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DTStamp { get; set; }
        IOrganizer Organizer { get; set; }
        IList<IRequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}
