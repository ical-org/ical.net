using System;
using DDay.iCal.Serialization.iCalendar;

namespace DDay.iCal
{
    /// <summary>
    /// Represents a time offset from UTC (Coordinated Universal Time).
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class UTCOffset : EncodableDataType, IUTCOffset
    {
        public TimeSpan Offset { get; set; }

        public bool Positive => Offset >= TimeSpan.Zero;

        public int Hours => Math.Abs(Offset.Hours);

        public int Minutes => Math.Abs(Offset.Minutes);

        public int Seconds => Math.Abs(Offset.Seconds);

        public UTCOffset() { }

        public UTCOffset(string value) : this()
        {
            Offset = UTCOffsetSerializer.GetOffset(value);
        }

        public UTCOffset(TimeSpan ts)
        {
            Offset = ts;
        }

        static public implicit operator UTCOffset(TimeSpan ts) => new UTCOffset(ts);

        static public explicit operator TimeSpan(UTCOffset o) => o.Offset;

        virtual public DateTime ToUTC(DateTime dt) => DateTime.SpecifyKind(dt.Add(-Offset), DateTimeKind.Utc);

        virtual public DateTime ToLocal(DateTime dt) => DateTime.SpecifyKind(dt.Add(Offset), DateTimeKind.Local);

        protected bool Equals(UTCOffset other)
        {
            return Offset == other.Offset;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((UTCOffset)obj);
        }

        public override int GetHashCode()
        {
            return Offset.GetHashCode();
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is IUTCOffset)
            {
                var utco = (IUTCOffset)obj;
                Offset = utco.Offset;
            }
        }

        public override string ToString()
        {
            return (Positive ? "+" : "-") + Hours.ToString("00") + Minutes.ToString("00") +
                   (Seconds != 0 ? Seconds.ToString("00") : string.Empty);
        }
    }
}