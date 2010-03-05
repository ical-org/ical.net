using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ITimeZoneInfo :
        ICalendarComponent
    {
        string TimeZoneName { get; set; }
        IUTCOffset OffsetFrom { get; set; }
        IUTCOffset OffsetTo { get; set; }
        string[] TimeZoneNames { get; set; }
    }
}
