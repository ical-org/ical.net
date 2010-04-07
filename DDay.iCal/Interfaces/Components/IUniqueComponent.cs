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

        IList<IAttachment> Attachments { get; set; }
        IList<IAttendee> Attendees { get; set; }
        IList<string> Categories { get; set; }
        string Class { get; set; }
        IList<string> Comments { get; set; }
        IList<string> Contacts { get; set; }
        IDateTime Created { get; set; }
        string Description { get; set; }
        IDateTime DTStamp { get; set; }
        IDateTime LastModified { get; set; }
        IOrganizer Organizer { get; set; }
        int Priority { get; set; }
        IList<string> RelatedComponents { get; set; }
        IList<IRequestStatus> RequestStatuses { get; set; }
        int Sequence { get; set; }
        string Summary { get; set; }
        Uri Url { get; set; }
    }
}
