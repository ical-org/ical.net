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

        public IValidationError[] Validate()
        {
            if (iCalText.Contains("\r\n\r\n") ||
                iCalText.Contains("\n\n") ||
                iCalText.Contains("\r\r"))
            {
                return new IValidationError[] { new ValidationErrorWithLookup("emptyLineError", ValidationErrorType.Warning) };
            }

            return new IValidationError[0];
        }

        #endregion
    }
}
