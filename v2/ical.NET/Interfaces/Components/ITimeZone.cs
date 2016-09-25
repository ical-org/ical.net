namespace ical.net.Interfaces.Components
{
    public interface ITimeZone : ICalendarComponent
    {
        string TzId { get; set; }
    }
}