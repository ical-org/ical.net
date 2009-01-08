using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class MissingVersionError : ValidationError
    {
        public MissingVersionError(ValidationErrorType type, bool isFatal)
        {
            ErrorNumber = Errors.MissingVersionError;
            Message = ResourceManager.GetString("MissingVersionError");
            Type = type;
            IsFatal = isFatal;            
        }
    }
}
