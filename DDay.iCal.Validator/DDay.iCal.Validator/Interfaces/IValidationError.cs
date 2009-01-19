using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface IValidationError
    {
        /// <summary>
        /// Gets the type of validation error.
        /// </summary>
        ValidationErrorType Type { get; }
        
        /// <summary>
        /// Gets a message describing the validation error.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets whether or not the validation error is
        /// fatal, and must be corrected for the rest of
        /// the validation to proceed.
        /// </summary>
        bool IsFatal { get; }

        /// <summary>
        /// The line number in the code that is related
        /// to this validation error.
        /// </summary>
        int Line { get; }

        /// <summary>
        /// The column number in the code that is related
        /// to this validation error.
        /// </summary>
        int Col { get; }
    }

    public enum ValidationErrorType
    {
        Warning,
        Error
    }
}
