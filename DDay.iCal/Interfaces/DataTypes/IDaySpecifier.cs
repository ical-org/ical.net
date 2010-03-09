using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface IDaySpecifier :
        IEncodableDataType,
        IComparable
    {
        int Num { get; set; }
        DayOfWeek DayOfWeek { get; set; }
    }
}
