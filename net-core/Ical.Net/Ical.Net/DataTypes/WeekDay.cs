using System;
using System.IO;
using Ical.Net.Interfaces.General;
using Ical.Net.Serialization.iCalendar.Serializers.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// Represents an RFC 5545 "BYDAY" value.
    /// </summary>
    public class WeekDay : EncodableDataType, IComparable<WeekDay>, IEquatable<WeekDay>
    {
        public virtual int Offset { get; set; } = int.MinValue;

        public virtual DayOfWeek DayOfWeek { get; set; }

        public WeekDay()
        {
            Offset = int.MinValue;
        }

        public WeekDay(DayOfWeek day) : this()
        {
            DayOfWeek = day;
        }

        public WeekDay(DayOfWeek day, int num) : this(day)
        {
            Offset = num;
        }

        public WeekDay(DayOfWeek day, FrequencyOccurrence type) : this(day, (int) type) {}

        public WeekDay(string value)
        {
            var serializer = new WeekDaySerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public bool Equals(WeekDay other) => other.Offset == Offset && other.DayOfWeek == DayOfWeek;

        public override bool Equals(object obj)
        {
            if (!(obj is WeekDay))
            {
                return false;
            }

            var ds = (WeekDay) obj;
            return ds.Offset == Offset && ds.DayOfWeek == DayOfWeek;
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode() ^ DayOfWeek.GetHashCode();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is WeekDay)
            {
                var bd = (WeekDay) obj;
                Offset = bd.Offset;
                DayOfWeek = bd.DayOfWeek;
            }
        }

        public int CompareTo(WeekDay other)
        {
            if (ReferenceEquals(other, null))
            {
                return 1;
            }
            var compare = DayOfWeek.CompareTo(other.DayOfWeek);
            if (compare == 0)
            {
                compare = Offset.CompareTo(other.Offset);
            }
            return compare;
        }
    }
}