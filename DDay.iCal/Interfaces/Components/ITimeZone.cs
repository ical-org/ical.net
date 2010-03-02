using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITimeZone :
        IRecurringComponent
    {
        ITZID TZID { get; set; }
        IDateTime LastModified { get; set; }
        IURI TZUrl { get; set; }
        IList<ITimeZoneInfo> TimeZoneInfos { get; set; }

        ITimeZoneInfo GetTimeZoneInfo(IDateTime dt);
    }
}
