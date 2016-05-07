using System;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Interfaces.Components
{
    public interface ITimeZone :
        ICalendarComponent
    {
        string ID { get; set; }
        string TZID { get; set; }
        IDateTime LastModified { get; set; }
        Uri Url { get; set; }
        ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos { get; set; }
    }
}
