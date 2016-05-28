using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Structs
{
    [Serializable]
    public struct TimeZoneObservance
    {
        public IPeriod Period { get; private set; }
        public ITimeZoneInfo TimeZoneInfo { get; private set; }

        public TimeZoneObservance(IPeriod period, ITimeZoneInfo tzi) : this()
        {
            Period = period;
            TimeZoneInfo = tzi;
        }

        public bool Contains(IDateTime dt)
        {
            if (Period != null)
            {
                return Period.Contains(dt);
            }
            return false;
        }
    }
}