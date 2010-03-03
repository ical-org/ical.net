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
    [DataContract(Name = "TimeZoneInfo", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class TimeZoneInfo : 
        RecurringComponent,
        ITimeZoneInfo
    {
        #region Private Fields

        private IUTCOffset m_TZOffsetFrom;            
        private IUTCOffset m_TZOffsetTo;            
        private IText[] m_TZName;

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

        virtual public IUTCOffset TZOffsetFrom
        {
            get { return Properties.Get<UTCOffset>("TZOFFSETFROM"); }
            set { Properties.Set("TZOFFSETFROM", value); }
        }

        virtual public IUTCOffset TZOffsetTo
        {
            get { return Properties.Get<UTCOffset>("TZOFFSETTO"); }
            set { Properties.Set("TZOFFSETTO", value); }
        }

        virtual public IText[] TZName
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

        public TimeZoneInfo() : base()
        {
            base.Sequence = null; // iCalTimeZoneInfo does not allow sequence numbers
        }
        public TimeZoneInfo(string name) : this()
        {
            this.Name = name;
        }

        #endregion

        #region Overrides

        internal override IList<Period> Evaluate(iCalDateTime FromDate, iCalDateTime ToDate)
        {
            List<IPeriod> periods = base.Evaluate(FromDate, ToDate);
            // Add the initial specified date/time for the time zone entry
            periods.Insert(0, new Period(Start, null));
            return periods;
        }

        public override bool Equals(object obj)
        {
            ITimeZoneInfo tzi = obj as ITimeZoneInfo;
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
