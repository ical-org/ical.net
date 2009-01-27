using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface IValidationResult
    {
        /// <summary>
        /// Gets the rule associated with this validation result.
        /// </summary>
        string Rule { get; }        

        /// <summary>
        /// Gets whether or not the validation was successful.
        /// Possible values are:
        /// True - validation succeeded.
        /// False - validation failed.
        /// Null - validation did not occur.
        /// </summary>
        bool? Passed { get; }

        /// <summary>
        /// Gets a list of errors that occurred during validation.
        /// </summary>
        IValidationError[] Errors { get; }
    }
}
