using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationError : 
        IValidationError
    {
        #region IValidationError Members

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

        public ValidationError(string msg)
        {
            Message = msg;
        }

        public ValidationError(string msg, ValidationErrorType type) :
            this(msg)
        {
            Type = type;
        }

        public ValidationError(string msg, ValidationErrorType type, bool isFatal) :
            this(msg, type)
        {
            IsFatal = isFatal;
        }

        public ValidationError(string msg, ValidationErrorType type, bool isFatal, int line) :
            this(msg, type, isFatal)
        {
            Line = line;
        }

        public ValidationError(string msg, ValidationErrorType type, bool isFatal, int line, int col) :
            this(msg, type, isFatal, line)
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
