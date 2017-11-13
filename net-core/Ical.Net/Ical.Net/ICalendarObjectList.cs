using Ical.Net.Collections.Interfaces;

namespace Ical.Net
{
    public interface ICalendarObjectList<TType> : IGroupedCollection<string, TType> where TType : class, ICalendarObject
    {
        TType this[int index] { get; }
    }
}