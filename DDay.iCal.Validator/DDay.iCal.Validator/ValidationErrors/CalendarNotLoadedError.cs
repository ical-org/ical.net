using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Validator.RFC2445;

namespace DDay.iCal.Validator
{
    public class CalendarNotLoadedError : ValidationError
    {
        public CalendarNotLoadedError()
        {
            ErrorNumber = Errors.CalendarNotLoaded;
            Message = ResourceManager.GetString("CalendarNotLoadedError");
            Type = ValidationErrorType.Error;
            IsFatal = true;
        }
    }
}
