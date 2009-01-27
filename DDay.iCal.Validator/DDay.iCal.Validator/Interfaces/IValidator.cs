using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface IValidator
    {
        /// <summary>
        /// Validates, returning a list of IValidationResults that
        /// describe the validation that has taken place.
        /// </summary>
        IValidationResult[] Validate();
    }
}
