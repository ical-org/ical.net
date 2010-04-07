using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal
{
    #region Alarms

    public enum AlarmAction
    {
        Audio,
        Display,
        Email,
        Procedure
    };

    public enum TriggerRelation
    {
        Start,
        End
    }

    #endregion

    #region Components

    public class Components
    {
        #region Constants

        public const string ALARM = "VALARM";
        public const string CALENDAR = "VCALENDAR";
        public const string EVENT = "VEVENT";
        public const string FREEBUSY = "VFREEBUSY";
        public const string TODO = "VTODO";
        public const string JOURNAL = "VJOURNAL";
        public const string TIMEZONE = "VTIMEZONE";
        public const string DAYLIGHT = "DAYLIGHT";
        public const string STANDARD = "STANDARD";

        #endregion
    }

    #endregion

    #region Status Constants

    /// <summary>
    /// Status codes available to an <see cref="Event"/> item
    /// </summary>
    public enum EventStatus
    {
        Tentative,
        Confirmed,
        Cancelled
    };

    /// <summary>
    /// Status codes available to a <see cref="Todo"/> item.
    /// </summary>
    public enum TodoStatus
    {
        NeedsAction,
        Completed,
        InProcess,
        Cancelled
    };

    /// <summary>
    /// Status codes available to a <see cref="Journal"/> entry.
    /// </summary>    
    public enum JournalStatus
    {
        Draft,      // Indicates journal is draft.
        Final,      // Indicates journal is final.
        Cancelled   // Indicates journal is removed.
    };

    #endregion

    #region Occurrence Evaluation

    public enum FrequencyType
    {
        None,
        Secondly,
        Minutely,
        Hourly,
        Daily,
        Weekly,
        Monthly,
        Yearly
    };

    /// <summary>
    /// Indicates the occurrence of the specific day within a
    /// MONTHLY or YEARLY recurrence frequency. For example, within
    /// a MONTHLY frequency, consider the following:
    /// 
    /// Recur r = new Recur();
    /// r.Frequency = FrequencyType.Monthly;
    /// r.ByDay.Add(new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.First));
    /// 
    /// The above example represents the first Monday within the month,
    /// whereas if FrequencyOccurrence.Last were specified, it would 
    /// represent the last Monday of the month.
    /// 
    /// For a YEARLY frequency, consider the following:
    /// 
    /// Recur r = new Recur();
    /// r.Frequency = FrequencyType.Yearly;
    /// r.ByDay.Add(new WeekDay(DayOfWeek.Monday, FrequencyOccurrence.Second));
    /// 
    /// The above example represents the second Monday of the year.  This can
    /// also be represented with the following code:
    /// 
    /// r.ByDay.Add(new WeekDay(DayOfWeek.Monday, 2));
    /// </summary>
    public enum FrequencyOccurrence
    {
        None = int.MinValue,
        Last = -1,
        SecondToLast = -2,
        ThirdToLast = -3,
        FourthToLast = -4,
        First = 1,
        Second = 2,
        Third = 3,
        Fourth = 4
    }

    public enum RecurrenceRestrictionType
    {
        /// <summary>
        /// Same as RestrictSecondly.
        /// </summary>
        Default,

        /// <summary>
        /// Does not restrict recurrence evaluation - WARNING: this may cause very slow performance!
        /// </summary>
        NoRestriction,

        /// <summary>
        /// Disallows use of the SECONDLY frequency for recurrence evaluation
        /// </summary>
        RestrictSecondly,

        /// <summary>
        /// Disallows use of the MINUTELY and SECONDLY frequencies for recurrence evaluation
        /// </summary>
        RestrictMinutely,

        /// <summary>
        /// Disallows use of the HOURLY, MINUTELY, and SECONDLY frequencies for recurrence evaluation
        /// </summary>
        RestrictHourly
    }

    public enum RecurrenceEvaluationModeType
    {
        /// <summary>
        /// Same as ThrowException.
        /// </summary>
        Default,

        /// <summary>
        /// Automatically adjusts the evaluation to the next-best frequency based on the restriction type.
        /// For example, if the restriction were IgnoreSeconds, and the frequency were SECONDLY, then
        /// this would cause the frequency to be adjusted to MINUTELY, the next closest thing.
        /// </summary>
        AdjustAutomatically,

        /// <summary>
        /// This will throw an exception if a recurrence rule is evaluated that does not meet the minimum
        /// restrictions.  For example, if the restriction were IgnoreSeconds, and a SECONDLY frequency
        /// were evaluated, an exception would be thrown.
        /// </summary>
        ThrowException
    } 

    #endregion

    #region Calendar Properties

    public class CalendarProductIDs
    {
        public const string Default = "-//ddaysoftware.com//NONSGML DDay.iCal 1.0//EN";
    }

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

    #endregion

    #region EventArgs

    public class ObjectEventArgs<T> :
        EventArgs
    {
        public T Object { get; set; }

        public ObjectEventArgs(T obj)
        {
            Object = obj;
        }
    }

    public class ValueChangedEventArgs :
        EventArgs
    {
        public object OldValue { get; set; }
        public object NewValue { get; set; }

        public ValueChangedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    #endregion
}
