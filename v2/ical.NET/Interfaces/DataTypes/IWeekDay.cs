using System;

namespace Ical.Net.Interfaces.DataTypes
{
    public interface IWeekDay : IEncodableDataType
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}