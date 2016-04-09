using System;

namespace DDay.iCal
{
    public interface ITimeZone :
        ICalendarComponent
    {
        string ID { get; set; }
        string TZID { get; set; }
        IDateTime LastModified { get; set; }
        Uri Url { get; set; }
        ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos { get; set; }
        TimeZoneObservance? GetTimeZoneObservance(IDateTime dt);
    }
}
