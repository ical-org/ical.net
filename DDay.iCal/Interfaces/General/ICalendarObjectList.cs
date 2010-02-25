using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarObjectList :
        IList<ICalendarObject>,
        ICopyable,
        IMergeable
    {
        ICalendarObject Parent { get; set; }
    }
}
