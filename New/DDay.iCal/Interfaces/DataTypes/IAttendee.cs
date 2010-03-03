using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IAttendee :
        ICalendarAddress
    {
        ICalendarAddress SentBy { get; set; }
        IText CommonName { get; set; }
        IURI DirectoryEntry { get; set; }
        string Type { get; set; }
        ICalendarAddressCollection Member { get; }
        string Role { get; set; }
        string ParticipationStatus { get; set; }
        bool RSVP { get; set; }
        ICalendarAddressCollection DelegatedTo { get; }
        ICalendarAddressCollection DelegatedFrom { get; }
        string Language { get; set; }
        string EmailAddress { get; }
    }
}
