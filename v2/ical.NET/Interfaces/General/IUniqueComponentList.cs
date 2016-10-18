using ical.net.Interfaces.Components;

namespace ical.net.Interfaces.General
{
    public interface IUniqueComponentList<TComponentType> : ICalendarObjectList<TComponentType> where TComponentType : class, IUniqueComponent
    {
        TComponentType this[string uid] { get; set; }
    }
}