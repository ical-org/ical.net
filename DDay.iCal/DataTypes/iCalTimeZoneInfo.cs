using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using DDay.iCal;
using DDay.iCal;
using DDay.iCal.Serialization;
using System.Runtime.Serialization;

namespace DDay.iCal
{    
    /// <summary>
    /// A class that contains time zone information, and is usually accessed
    /// from an iCalendar object using the <see cref="DDay.iCal.iCalendar.GetTimeZone"/> method.        
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "iCalTimeZoneInfo", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class iCalTimeZoneInfo : RecurringComponent
    {
        #region Disabled Properties and Methods

        public override List<Alarm> Alarms
        {
            get
            {
                return base.Alarms;
            }
            set
            {
            }
        }

        public override Binary[] Attach
        {
            get
            {
                return base.Attach;
            }
            set
            {
            }
        }

        public override Attendee[] Attendee
        {
            get
            {
                return base.Attendee;
            }
            set
            {
            }
        }

        public override TextCollection[] Categories
        {
            get
            {
                return base.Categories;
            }
            set
            {
            }
        }

        public override string Category
        {
            get
            {
                return base.Category;
            }
            set
            {
            }
        }

        public override Text Class
        {
            get
            {
                return base.Class;
            }
            set
            {
            }
        }

        public override Text[] Comment
        {
            get
            {
                return base.Comment;
            }
            set
            {
            }
        }

        public override Text[] Contact
        {
            get
            {
                return base.Contact;
            }
            set
            {
            }
        }

        public override iCalDateTime Created
        {
            get
            {
                return base.Created;
            }
            set
            {
            }
        }

        public override Text Description
        {
            get
            {
                return base.Description;
            }
            set
            {
            }
        }

        public override RecurrenceDates[] ExDate
        {
            get
            {
                return base.ExDate;
            }
            set
            {
            }
        }

        public override RecurrencePattern[] ExRule
        {
            get
            {
                return base.ExRule;
            }
            set
            {
            }
        }

        public override iCalDateTime LastModified
        {
            get
            {
                return base.LastModified;
            }
            set
            {
            }
        }

        public override Organizer Organizer
        {
            get
            {
                return base.Organizer;
            }
            set
            {
            }
        }

        public override Integer Priority
        {
            get
            {
                return base.Priority;
            }
            set
            {
            }
        }

        public override iCalDateTime RecurrenceID
        {
            get
            {
                return base.RecurrenceID;
            }
            set
            {
            }
        }

        public override Text[] RelatedTo
        {
            get
            {
                return base.RelatedTo;
            }
            set
            {
            }
        }

        public override RequestStatus[] RequestStatus
        {
            get
            {
                return base.RequestStatus;
            }
            set
            {
            }
        }

        public override Integer Sequence
        {
            get
            {
                return base.Sequence;
            }
            set
            {
            }
        }

        public override Text Summary
        {
            get
            {
                return base.Summary;
            }
            set
            {
            }
        }

        public override Text UID
        {
            get
            {
                return base.UID;
            }
            set
            {
            }
        }

        public override URI Url
        {
            get
            {
                return base.Url;
            }
            set
            {
            }
        }

        public override List<AlarmOccurrence> PollAlarms()
        {
            return null;
        }

        public override List<AlarmOccurrence> PollAlarms(iCalDateTime Start, iCalDateTime End)
        {
            return null;
        }

        public override void AddAlarm(Alarm alarm)
        {
        }

        public override void AddAttendee(Attendee attendee)
        {
        }

        public override void AddCategory(string categoryName)
        {
        }

        public override void AddComment(string comment)
        {
        }

        public override void AddExceptionPattern(RecurrencePattern recur)
        {
        }

        public override void AddSingleException(iCalDateTime dt)
        {
        }

        protected override void EvaluateExDate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
        }

        protected override void EvaluateExRule(iCalDateTime FromDate, iCalDateTime ToDate)
        {
        }

        #endregion

        #region Private Fields

        private UTC_Offset m_TZOffsetFrom;            
        private UTC_Offset m_TZOffsetTo;            
        private Text[] m_TZName;

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the name of the current Time Zone.
        /// <example>
        ///     The following are examples:
        ///     <list type="bullet">
        ///         <item>EST</item>
        ///         <item>EDT</item>
        ///         <item>MST</item>
        ///         <item>MDT</item>
        ///     </list>
        /// </example>
        /// </summary>
        virtual public string TimeZoneName
        {
            get
            {
                if (TZName.Length > 0)
                    return TZName[0].Value;
                return string.Empty;
            }
            set
            {
                if (TZName == null)
                    TZName = new Text[1];
                TZName[0] = (Text)value;
                TZName[0].Name = "TZNAME";
            }
        }

        virtual public UTC_Offset TZOffsetFrom
        {
            get { return Properties.Get<UTC_Offset>("TZOFFSETFROM"); }
            set { Properties.Set("TZOFFSETFROM", value); }
        }

        virtual public UTC_Offset TZOffsetTo
        {
            get { return Properties.Get<UTC_Offset>("TZOFFSETTO"); }
            set { Properties.Set("TZOFFSETTO", value); }
        }

        virtual public Text[] TZName
        {
            get { return Properties.Get<Text[]>("TZNAME"); }
            set { Properties.Set("TZNAME", value); }
        }

        #region Overrides

        /// <summary>
        /// Force the DTSTART into a local date-time value.
        /// 
        /// From RFC 5545:
        /// The mandatory "DTSTART" property gives the effective onset date and 
        /// local time for the time zone sub-component definition. "DTSTART" in
        /// this usage MUST be specified as a local DATE-TIME value.
        /// 
        /// Also from RFC 5545:
        /// The date with local time form is simply a date-time value that does
        /// not contain the UTC designator nor does it reference a time zone. For
        /// example, the following represents Janurary 18, 1998, at 11 PM:
        /// 
        /// DTSTART:19980118T230000
        /// </summary>        
        public override iCalDateTime DTStart
        {
            get
            {
                return base.DTStart;
            }
            set
            {
                base.DTStart = value;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public iCalTimeZoneInfo() : base()
        {
            base.Sequence = null; // iCalTimeZoneInfo does not allow sequence numbers
        }
        public iCalTimeZoneInfo(string name) : this()
        {
            this.Name = name;
        }

        #endregion

        #region Overrides

        internal override List<Period> Evaluate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            List<Period> periods = base.Evaluate(FromDate, ToDate);
            // Add the initial specified date/time for the time zone entry
            periods.Insert(0, new Period(Start, null));
            return periods;
        }

        public override bool Equals(object obj)
        {
            iCalTimeZoneInfo tzi = obj as iCalTimeZoneInfo;
            if (tzi != null)
            {
                return object.Equals(TZName, tzi.TZName) &&
                    object.Equals(TZOffsetFrom, tzi.TZOffsetFrom) &&
                    object.Equals(TZOffsetTo, tzi.TZOffsetTo);
            }
            return base.Equals(obj);
        }
                              
        #endregion
    }    
}
