using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Components
{
    public partial class TimeZone : ComponentBase
    {
        /// <summary>
        /// A class that contains time zone information, and is usually accessed
        /// from an iCalendar object using the <see cref="DDay.iCal.iCalendar.GetTimeZone"/> method.        
        /// </summary>
        public class TimeZoneInfo : RecurringComponent
        {
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
            public string TimeZoneName
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
                    TZName[0] = new Text(value);
                    TZName[0].Name = "TZNAME";
                }
            }

            [Serialized]
            public UTC_Offset TZOffsetFrom
            {
                get { return m_TZOffsetFrom; }
                set { m_TZOffsetFrom = value; }
            }

            [Serialized]
            public UTC_Offset TZOffsetTo
            {
                get { return m_TZOffsetTo; }
                set { m_TZOffsetTo = value; }
            }

            [Serialized]
            public Text[] TZName
            {
                get { return m_TZName; }
                set { m_TZName = value; }
            }

            #region Overrides

            /// <summary>
            /// Force the DTSTART into a local date-time value.
            /// 
            /// From RFC 2445:
            /// The mandatory "DTSTART" property gives the effective onset date and 
            /// local time for the time zone sub-component definition. "DTSTART" in
            /// this usage MUST be specified as a local DATE-TIME value.
            /// 
            /// Also from RFC 2445:
            /// The date with local time form is simply a date-time value that does
            /// not contain the UTC designator nor does it reference a time zone. For
            /// example, the following represents Janurary 18, 1998, at 11 PM:
            /// 
            /// DTSTART:19980118T230000
            /// </summary>
            [Serialized, DefaultValueType("DATE-TIME"), DisallowedTypes("DATE", "DATE-TIME")]
            public override Date_Time DTStart
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

            public TimeZoneInfo() : base() { }
            public TimeZoneInfo(iCalObject parent) : base(parent) { }
            public TimeZoneInfo(string name, iCalObject parent)
                : base(parent)
            {
                this.Name = name;
            }

            #endregion

            #region Overrides

            public override List<Period> Evaluate(Date_Time FromDate, Date_Time ToDate)
            {
                List<Period> periods = base.Evaluate(FromDate, ToDate);
                // Add the initial specified date/time for the time zone entry
                periods.Insert(0, new Period(Start, null));
                return periods;
            }            

            /// <summary>
            /// Returns a typed copy of the TimeZoneInfo object.
            /// </summary>
            /// <returns>A typed copy of the TimeZoneInfo object.</returns>
            public new TimeZoneInfo Copy()
            {
                return (TimeZoneInfo)base.Copy();
            }

            /// <summary>
            /// Creates a copy of the <see cref="TimeZoneInfo"/> object.
            /// </summary>
            public override iCalObject Copy(iCalObject parent)
            {
                // Create a copy
                iCalObject obj = base.Copy(parent);

                // Copy the name, since the .ctor(iCalObject) constructor
                // doesn't handle it
                obj.Name = this.Name;
                
                return obj;
            }
                                  
            #endregion
        }
    }
}
