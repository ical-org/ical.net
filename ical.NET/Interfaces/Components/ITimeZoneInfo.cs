using System.Collections.Generic;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Interfaces.Components
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
