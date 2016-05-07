using System;

namespace DDay.iCal
{
    public interface IWeekDay :
        IEncodableDataType,
        IComparable
    {
        int Offset { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}
