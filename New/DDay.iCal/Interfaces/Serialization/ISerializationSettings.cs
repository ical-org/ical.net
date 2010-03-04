using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ISerializationSettings
    {
        Type iCalendarType { get; set; }
        bool EnsureAccurateLineNumbers { get; set; }
        ParsingModeType ParsingMode { get; set; }
    }

    public enum ParsingModeType
    {
        Strict,
        Loose
    }
}
