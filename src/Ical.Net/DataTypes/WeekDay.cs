using System;
using System.IO;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.DataTypes
{
    /// <summary> <see cref="DayOfWeek"/> represents an RFC 5545 "BYDAY" value. </summary>
    public class WeekDay : EncodableDataType, IEquatable<WeekDay>, IComparable<WeekDay>
    {
        /// <summary> First day of Week Offset; <see cref="int.MinValue"/> indicate no Offset </summary>
        public int Offset { get; set; } = int.MinValue;

        public DayOfWeek DayOfWeek { get; set; }

        public WeekDay()
        {
            Offset = int.MinValue;
        }

        public WeekDay(DayOfWeek day) : this()
        {
            DayOfWeek = day;
        }

        public WeekDay(DayOfWeek day, int offset) : this(day)
        {
            Offset = offset;
        }

        public WeekDay(DayOfWeek day, FrequencyOccurrence type) : this(day, (int) type) {}

        public WeekDay(string value)
        {
            var serializer = new WeekDaySerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override bool Equals(object obj)
        {
	        if (!(obj is WeekDay weekDay))
            {
                return false;
            }

	        return Equals(weekDay);
        }

        public bool Equals(WeekDay weekDay)
        {
            return weekDay != null && weekDay.Offset == Offset && weekDay.DayOfWeek == DayOfWeek;
        }

        public override int GetHashCode() => Offset.GetHashCode() ^ DayOfWeek.GetHashCode();

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is WeekDay bd)
            {
                Offset = bd.Offset;
                DayOfWeek = bd.DayOfWeek;
            }
        }

        public int CompareTo(object obj)
        {
	        WeekDay wd = null;
	        if (obj is string)
            {
                wd = new WeekDay(obj.ToString());
            }
            else if (obj is WeekDay day)
            {
                wd = day;
            }

            return CompareTo(wd);
        }

        public int CompareTo(WeekDay wd)
        {
	        if (wd == null)
	        {
		        throw new ArgumentException();
	        }
	        var compare = DayOfWeek.CompareTo(wd.DayOfWeek);
	        if (compare == 0)
	        {
		        compare = Offset.CompareTo(wd.Offset);
	        }
	        return compare;
        }
    }
}