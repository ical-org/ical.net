using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationErrorWithLookup :
        ValidationError
    {
        public ValidationErrorWithLookup(string name) : base(name)
        {            
            Message = ResourceManager.GetError(name);
        }

        public ValidationErrorWithLookup(string name, ValidationErrorType type) :
            this(name)
        {
            Type = type;
        }

        public ValidationErrorWithLookup(string name, ValidationErrorType type, bool isFatal) :
            this(name, type)
        {            
            IsFatal = isFatal;
        }

        public ValidationErrorWithLookup(string name, ValidationErrorType type, bool isFatal, int line) :
            this(name, type, isFatal)
        {
            Line = line;
        }

        public ValidationErrorWithLookup(string name, ValidationErrorType type, bool isFatal, int line, int col) :
            this(name, type, isFatal, line)
        {
            Col = col;
        }
    }
}
