using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    public class CalendarVersions
    {
        public const string v2_0 = "2.0";
    }

    public class CalendarScales
    {
        public const string Gregorian = "GREGORIAN";
    }

    public class CalendarMethods
    {
        /// <summary>
        /// Used to publish an iCalendar object to one or
        /// more "Calendar Users".  There is no interactivity
        /// between the publisher and any other "Calendar User".
        /// An example might include a baseball team publishing
        /// its schedule to the public.
        /// </summary>
        public const string Publish = "PUBLISH";

        /// <summary>
        /// Used to schedule an iCalendar object with other
        /// "Calendar Users".  Requests are interactive in
        /// that they require the receiver to respond using
        /// the reply methods.  Meeting requests, busy-time
        /// requests, and the assignment of tasks to other
        /// "Calendar Users" are all examples.  Requests are
        /// also used by the Organizer to update the status
        /// of an iCalendar object. 
        /// </summary>
        public const string Request = "REQUEST";

        /// <summary>
        /// A reply is used in response to a request to
        /// convey Attendee status to the Organizer.
        /// Replies are commonly used to respond to meeting
        /// and task requests.     
        /// </summary>
        public const string Reply = "REPLY";

        /// <summary>
        /// Add one or more new instances to an existing
        /// recurring iCalendar object. 
        /// </summary>
        public const string Add = "ADD";

        /// <summary>
        /// Cancel one or more instances of an existing
        /// iCalendar object.
        /// </summary>
        public const string Cancel = "CANCEL";

        /// <summary>
        /// Used by an Attendee to request the latest
        /// version of an iCalendar object.
        /// </summary>
        public const string Refresh = "REFRESH";

        /// <summary>
        /// Used by an Attendee to negotiate a change in an
        /// iCalendar object.  Examples include the request
        /// to change a proposed event time or change the
        /// due date for a task.
        /// </summary>
        public const string Counter = "COUNTER";

        /// <summary>
        /// Used by the Organizer to decline the proposed
        /// counter-proposal.
        /// </summary>
        public const string DeclineCounter = "DECLINECOUNTER";
    }
}
