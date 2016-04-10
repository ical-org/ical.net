using System;
using NodaTime;
using NodaTime.TimeZones;

namespace DDay.iCal
{
    public class NodaUtility
    {
        public static ZonedDateTime ToTimeZone(DateTime datetime, string fromTzId, string toTzId, TimeSpan offset)
        {
            var oldZone = GetTimeZone(fromTzId);
            var oldZonedTime = new ZonedDateTime(LocalDateTime.FromDateTime(datetime), oldZone, Offset.FromHoursAndMinutes(offset.Hours, offset.Minutes));
            var newZone = GetTimeZone(toTzId);
            var convertedTime = oldZonedTime.WithZone(newZone);
            return convertedTime;
        }

        public static DateTimeZone GetTimeZone(string tzId)
        {
            var iana = DateTimeZoneProviders.Tzdb.GetZoneOrNull(tzId);
            if (iana != null)
            {
                return iana;
            }

            var bcl = DateTimeZoneProviders.Bcl.GetZoneOrNull(tzId);
            if (bcl != null)
            {
                return bcl;
            }

            var serialized = DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            if (serialized != null)
            {
                return serialized;
            }

            tzId = tzId.Replace("/", "-");
            serialized = DateTimeZoneProviders.Serialization.GetZoneOrNull(tzId);
            if (serialized != null)
            {
                return serialized;
            }

            throw new ArgumentException($"{tzId} is not a recognized time zone");
        }

        //public static Offset GetOffsetFromIDateTime(IDateTime dt)
        //{
        //    if (Parent == null)
        //    {
        //        throw new Exception("Cannot get the Offset from an IDateTime whose Parent is null");
        //    }

        //    var offset = OffsetTo.Offset;
        //    return Offset.FromTicks(offset.Ticks);
        //}
    }
}
