using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ICalendarTestError
    {
        string Name { get; }
        string Message { get; }
        string Source { get; }
        IValidationError[] Errors { get; }
    }
}
