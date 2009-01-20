using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationError : 
        IValidationError
    {
        #region IValidationError Members

        public string Name { get; set; }
        public ValidationErrorType Type { get; set; }
        public string Message { get; set; }
        public bool IsFatal { get; set; }
        public int Line { get; set; }
        public int Col { get; set; }

        #endregion

        #region Constructors
        
        public ValidationError()
        {
        }

        public ValidationError(string name)
        {
            Name = name;
        }

        public ValidationError(string name, string msg) :
            this(name)
        {
            Message = msg;
        }

        public ValidationError(string name, string msg, ValidationErrorType type) :
            this(name, msg)
        {
            Type = type;
        }

        public ValidationError(string name, string msg, ValidationErrorType type, bool isFatal) :
            this(name, msg, type)
        {
            IsFatal = isFatal;
        }

        public ValidationError(string name, string msg, ValidationErrorType type, bool isFatal, int line) :
            this(name, msg, type, isFatal)
        {
            Line = line;
        }

        public ValidationError(string name, string msg, ValidationErrorType type, bool isFatal, int line, int col) :
            this(name, msg, type, isFatal, line)
        {
            Col = col;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(ResourceManager.GetString(Type.ToString()));

            if (!string.IsNullOrEmpty(Message))
            {
                sb.Append(": ");
                sb.Append(Message);
            }
            
            if (Line != default(int))
            {
                sb.Append(" (" + ResourceManager.GetString("Line") + " ");
                sb.Append(Line);
                if (Col != default(int))
                {
                    sb.Append(" " + ResourceManager.GetString("Column") + " ");
                    sb.Append(Col);
                }
                sb.Append(")");
            }

            return sb.ToString();
        }

        #endregion
    }
}
