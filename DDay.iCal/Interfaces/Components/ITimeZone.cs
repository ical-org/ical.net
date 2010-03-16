using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITimeZone :
        ICalendarComponent
    {
        string ID { get; set; }
        string TZID { get; set; }
        IDateTime LastModified { get; set; }
        Uri TZUrl { get; set; }
        Uri Url { get; set; }
        IList<ITimeZoneInfo> TimeZoneInfos { get; set; }
        ITimeZoneInfo GetTimeZoneInfo(IDateTime dt);
    }
}
