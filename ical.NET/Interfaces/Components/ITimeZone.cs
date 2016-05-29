using System;
using Ical.Net.Interfaces.General;

namespace Ical.Net.Interfaces.Components
{
    public interface ITimeZone : ICalendarComponent
    {
        string Id { get; set; }
        string TzId { get; set; }
        Uri Url { get; set; }
        ICalendarObjectList<ITimeZoneInfo> TimeZoneInfos { get; }
    }
}