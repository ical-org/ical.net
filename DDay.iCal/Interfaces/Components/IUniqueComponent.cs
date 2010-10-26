using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    public delegate void UIDChangedEventHandler(object sender, string OldUID, string NewUID);

    public interface IUniqueComponent :
        ICalendarComponent
    {
        /// <summary>
        /// Fires when the UID of the component has changed.
        /// </summary>
        event UIDChangedEventHandler UIDChanged;
        string UID { get; set; }

        IList<IAttendee> Attendees { get; set; }
        IList<string> Comments { get; set; }
        IDateTime DTStamp { get; set; }
        IOrganizer Organizer { get; set; }
        IList<IRequestStatus> RequestStatuses { get; set; }
        Uri Url { get; set; }
    }
}
