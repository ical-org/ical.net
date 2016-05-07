using System.Collections.Generic;

namespace DDay.iCal
{
    public interface ITimeZoneInfo : ICalendarComponent, IRecurrable
    {
        string TzId { get; }
        string TimeZoneName { get; set; }
        IList<string> TimeZoneNames { get; set; }
        IUTCOffset OffsetFrom { get; set; }
        IUTCOffset OffsetTo { get; set; }
    }
}
