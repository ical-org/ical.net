using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITimeZoneInfo :
        ICalendarComponent,
        IRecurrable
    {
        string TimeZoneName { get; set; }
        IList<string> TimeZoneNames { get; set; }
        IUTCOffset OffsetFrom { get; set; }
        IUTCOffset OffsetTo { get; set; }
    }    
}
