using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITimeZone :
        IRecurringComponent
    {
        ITimeZoneID TZID { get; set; }
        ITimeZoneID TimeZoneID { get; set; }

        iCalDateTime LastModified { get; set; }

        Uri TZUrl { get; set; }
        Uri TimeZoneUrl { get; set; }

        IList<ITimeZoneInfo> TimeZoneInfos { get; set; }

        ITimeZoneInfo GetTimeZoneInfo(iCalDateTime dt);
    }
}
