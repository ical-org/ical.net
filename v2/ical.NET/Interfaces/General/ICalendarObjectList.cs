using ical.net.collections.Interfaces;

namespace ical.net.Interfaces.General
{
    public interface ICalendarObjectList<TType> : IGroupedCollection<string, TType> where TType : class, ICalendarObject
    {
        TType this[int index] { get; }
    }
}