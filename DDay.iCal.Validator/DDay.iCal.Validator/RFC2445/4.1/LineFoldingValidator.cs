using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    public class LineFoldingValidator :
        IValidator
    {
        #region Public Properties

        public string iCalText { get; set; }

        #endregion

        #region Constructors

        public LineFoldingValidator(string cal_text)
        {
            iCalText = cal_text;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("lineFolding");

            try
            {
                StringReader sr = new StringReader(iCalText);
                iCalendar calendar = iCalendar.LoadFromStream(sr);

                result.Passed = true;
            }
            catch (antlr.MismatchedTokenException)
            {
                result.Passed = false;
                result.Errors = new IValidationError[] { new ValidationErrorWithLookup("calendarParseError", ValidationErrorType.Error, true) };
            }

            return new IValidationResult[] { result };
        }

        #endregion
    }
}
