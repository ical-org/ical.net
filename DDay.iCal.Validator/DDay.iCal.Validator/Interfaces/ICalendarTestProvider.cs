using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ICalendarTestProvider
    {
        ICalendarTest[] Tests { get; }
    }
}
