using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public interface ICalendarParameter :
        ICalendarObject,
        IKeyedObject<string>
    {
        IList<string> Values { get; set; }
    }
}
