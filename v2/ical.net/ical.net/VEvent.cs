using NodaTime;

namespace ical.net
{
    public class VEvent
    {
        public ZonedDateTime Start { get; private set; }
        public ZonedDateTime End { get; private set; }
        public Duration Duration => End.ToInstant() - Start.ToInstant();
    }
}
