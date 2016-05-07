using System;

namespace DDay.iCal.Serialization
{
    public interface ISerializationSettings
    {
        Type iCalendarType { get; set; }
        bool EnsureAccurateLineNumbers { get; set; }
        ParsingModeType ParsingMode { get; set; }
        bool StoreExtraSerializationData { get; set; }
    }

    public enum ParsingModeType
    {
        Strict,
        Loose
    }
}
