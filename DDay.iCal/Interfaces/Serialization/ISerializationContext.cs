using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface ISerializationContext
    {
        bool EnsureAccurateLineNumbers { get; set; }
        bool CollectLineNumbers { get; set; }
        ParsingModeType ParsingMode { get; set; }
    }

    public enum ParsingModeType
    {
        Strict,
        Loose
    }
}
