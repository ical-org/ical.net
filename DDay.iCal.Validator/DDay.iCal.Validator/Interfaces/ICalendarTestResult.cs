using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ICalendarTestResult
    {
        string Source { get; }
        ICalendarTest Test { get; }
        bool Passed { get; }
        ICalendarTestError Error { get; }
    }
}
