using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Build(string objectName, bool uninitialized);
    }
}
