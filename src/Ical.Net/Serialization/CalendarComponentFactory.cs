using Ical.Net.CalendarComponents;

namespace Ical.Net.Serialization
{
    public class CalendarComponentFactory
    {
        public static ICalendarComponent Build(string objectName)
        {
            var name = objectName.ToUpper();
            var c = Create_(name);
            c.Name = name;
            return c;
        }

        static ICalendarComponent Create_(string name)
            => name switch
            {
                Components.Alarm => new Alarm(),
                EventStatus.Name => new CalendarEvent(),
                Components.Freebusy => new FreeBusy(),
                JournalStatus.Name => new Journal(),
                Components.Timezone => new VTimeZone(),
                TodoStatus.Name => new Todo(),
                Components.Calendar => new Calendar(),
                Components.Daylight => new VTimeZoneInfo(),
                Components.Standard => new VTimeZoneInfo(),
                _ => new CalendarComponent()
            };
    }
}