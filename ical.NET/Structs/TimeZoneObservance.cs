using System;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;

namespace Ical.Net.Structs
{
    [Serializable]
    public struct TimeZoneObservance
    {
        public ITimeZoneInfo TimeZoneInfo { get; private set; }

        public TimeZoneObservance(IPeriod period, ITimeZoneInfo tzi) : this()
        {
            TimeZoneInfo = tzi;
        }
    }
}