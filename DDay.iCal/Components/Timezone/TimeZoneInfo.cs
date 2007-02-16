using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using DDay.iCal.Objects;
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
            #region Public Fields

            [SerializedAttribute]
            public UTC_Offset TZOffsetFrom;
            [SerializedAttribute]
            public UTC_Offset TZOffsetTo;            
            [SerializedAttribute]
            public Text[] TZName;

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

            /// <summary>
            /// Returns a typed copy of the TimeZoneInfo object.
            /// </summary>
            /// <returns>A typed copy of the TimeZoneInfo object.</returns>
            public TimeZoneInfo Copy()
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
