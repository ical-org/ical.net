using System;

namespace ical.net.Interfaces.DataTypes
{
    public interface IWeekDay : IEncodableDataType, IComparable
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}