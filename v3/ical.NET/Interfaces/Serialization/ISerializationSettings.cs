using System;

namespace Ical.Net.Interfaces.Serialization
{
    internal interface ISerializationSettings
    {
        Type CalendarType { get; set; }
        bool EnsureAccurateLineNumbers { get; set; }
        ParsingModeType ParsingMode { get; set; }
        bool StoreExtraSerializationData { get; set; }
    }

    internal enum ParsingModeType
    {
        Strict,
        Loose
    }
}