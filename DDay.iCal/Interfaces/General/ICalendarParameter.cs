using System;
using System.Collections.Generic;
using System.Text;
using DDay.Collections;

namespace DDay.iCal
{
    public interface ICalendarParameter :
        ICalendarObject,
        IValueObject<string>
    {
        string Value { get; set; }
    }
}
