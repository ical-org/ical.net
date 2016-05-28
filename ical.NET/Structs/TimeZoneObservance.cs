using System;
using Ical.Net.Interfaces.Components;

namespace Ical.Net.Structs
{
    [Serializable]
    public struct TimeZoneObservance
    {
        public ITimeZoneInfo TimeZoneInfo { get; }
    }
}