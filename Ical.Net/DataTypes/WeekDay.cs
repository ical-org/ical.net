﻿using Ical.Net.Serialization.DataTypes;
using System;
using System.IO;

namespace Ical.Net.DataTypes
{
    /// <summary>
    /// Represents an RFC 5545 "BYDAY" value.
    /// </summary>
    public class WeekDay : EncodableDataType
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

        public WeekDay(DayOfWeek day, FrequencyOccurrence type) : this(day, (int)type) { }

        public WeekDay(string value)
        {
            var serializer = new WeekDaySerializer();
            CopyFrom(serializer.Deserialize(new StringReader(value)) as ICopyable);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is WeekDay))
            {
                return false;
            }

            var ds = (WeekDay)obj;
            return ds.Offset == Offset && ds.DayOfWeek == DayOfWeek;
        }

        public override int GetHashCode() => Offset.GetHashCode() ^ DayOfWeek.GetHashCode();

        /// <inheritdoc/>
        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is not WeekDay weekday) return;

            Offset = weekday.Offset;
            DayOfWeek = weekday.DayOfWeek;
        }

        public int CompareTo(object obj)
        {
            var weekday = obj switch
            {
                string => new WeekDay(obj.ToString()),
                WeekDay day => day,
                _ => null
            };

            if (weekday == null)
            {
                throw new ArgumentException();
            }

            var compare = DayOfWeek.CompareTo(weekday.DayOfWeek);
            if (compare == 0)
            {
                compare = Offset.CompareTo(weekday.Offset);
            }
            return compare;
        }
    }
}