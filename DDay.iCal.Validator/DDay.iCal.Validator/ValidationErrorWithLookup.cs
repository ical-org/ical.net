using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationErrorWithLookup :
        ValidationError
    {
        public ValidationErrorWithLookup(string errorName)
        {
            Message = ResourceManager.GetError(errorName);
        }

        public ValidationErrorWithLookup(string errorName, ValidationErrorType type) :
            this(errorName)
        {
            Type = type;
        }

        public ValidationErrorWithLookup(string errorName, ValidationErrorType type, bool isFatal) :
            this(errorName, type)
        {
            IsFatal = isFatal;
        }

        public ValidationErrorWithLookup(string errorName, ValidationErrorType type, bool isFatal, int line) :
            this(errorName, type, isFatal)
        {
            Line = line;
        }

        public ValidationErrorWithLookup(string errorName, ValidationErrorType type, bool isFatal, int line, int col) :
            this(errorName, type, isFatal, line)
        {
            Col = col;
        }
    }
}
