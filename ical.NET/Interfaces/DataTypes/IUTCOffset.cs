using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IUTCOffset : IEncodableDataType
    {
        TimeSpan Offset { get; set; }
        bool Positive { get; }
        int Hours { get; }
        int Minutes { get; }
        int Seconds { get; }

        DateTime ToUTC(DateTime dt);
        DateTime ToLocal(DateTime dt);
    }
}
