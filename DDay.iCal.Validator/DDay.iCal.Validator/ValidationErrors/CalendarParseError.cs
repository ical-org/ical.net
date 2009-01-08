using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class CalendarParseError :
        ValidationError
    {
        public CalendarParseError(int line, int col, string msg)
        {
            ErrorNumber = Errors.CalendarParseError;
            Message = ResourceManager.GetString("CalendarParseError") + Environment.NewLine + msg;
            Type = ValidationErrorType.Error;
            Line = line;
            Col = col;
            IsFatal = true;
        }
    }
}
