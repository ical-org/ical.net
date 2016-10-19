using Ical.Net.Interfaces.Components;

namespace Ical.Net.Serialization.iCalendar.Processors
{
    /// <summary>
    /// FIXME: implement this.
    /// </summary>
    public class ComponentPropertyConsolidator : CompositeProcessor<ICalendarComponent>
    {
        public virtual void PreSerialization(ICalendarComponent obj) {}

        public virtual void PostSerialization(ICalendarComponent obj) {}

        public virtual void PreDeserialization(ICalendarComponent obj) {}

        public virtual void PostDeserialization(ICalendarComponent obj) {}
    }
}