using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.Evaluation;

namespace Ical.Net.Interfaces.Components
{
    public interface ITimeZoneInfo : ICalendarComponent, IRecurrable
    {
        string TzId { get; }
        IUtcOffset OffsetTo { get; }
    }
}