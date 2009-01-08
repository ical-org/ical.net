using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class VersionMismatchError : ValidationError
    {
        public VersionMismatchError()
        {
            ErrorNumber = Errors.VersionMismatchError;
            Message = ResourceManager.GetString("VersionMismatchError");
            Type = ValidationErrorType.Warning;
            IsFatal = false;
        }
    }
}
