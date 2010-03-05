using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public interface ICalendarComponentFactory
    {
        ICalendarComponent Create(string objectName);
    }
}
