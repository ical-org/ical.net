using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarTimeZone :
        ICalendarComponent
    {
        TZID TZID { get; set; }
        iCalDateTime Last_Modified { get; set; }
        URI TZUrl { get; set; }
        IList<iCalTimeZoneInfo> TimeZoneInfos { get; set; }

        iCalTimeZoneInfo GetTimeZoneInfo(iCalDateTime dt);
    }
}
