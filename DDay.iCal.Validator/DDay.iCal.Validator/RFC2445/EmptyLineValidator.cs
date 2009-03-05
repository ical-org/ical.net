using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    public class EmptyLineValidator : 
        IValidator
    {
        #region Public Properties

        public string iCalText { get; set; }

        #endregion

        #region Constructors

        public EmptyLineValidator(string cal_text)
        {
            iCalText = cal_text;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("emptyLine");

            // Convert all variations of newlines to a simple "\n" so
            // we can easily detect empty lines.
            string simpleNewlineCalendar = iCalText.Replace("\r\n", "\n");
            simpleNewlineCalendar = simpleNewlineCalendar.Replace("\r", "\n");

            if (simpleNewlineCalendar.Contains("\n\n"))
            {
                result.Passed = false;
                result.Errors = new IValidationError[] { new ValidationErrorWithLookup("emptyLineError", ValidationErrorType.Warning) };
            }
            else result.Passed = true;

            return new IValidationResult[] { result };
        }

        #endregion
    }
}
