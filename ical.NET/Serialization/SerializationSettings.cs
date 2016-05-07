using System;
using Ical.Net.Interfaces.Serialization;

namespace Ical.Net.Serialization
{
    public class SerializationSettings :
        ISerializationSettings
    {
        #region Private Fields

        #endregion

        #region ISerializationSettings Members

        public virtual Type CalendarType { get; set; } = typeof(Net.Calendar);

        public virtual bool EnsureAccurateLineNumbers { get; set; }

        public virtual ParsingModeType ParsingMode { get; set; } = ParsingModeType.Strict;

        public virtual bool StoreExtraSerializationData { get; set; }

        #endregion
    }
}
