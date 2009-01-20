using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Validator.RFC2445
{
    public class LineEndingsValidator : 
        IValidator
    {
        #region Public Properties

        public string iCalText { get; set; }

        #endregion

        #region Constructors

        public LineEndingsValidator(string cal_text)
        {
            iCalText = cal_text;
        }

        #endregion

        #region IValidator Members

        public IValidationError[] Validate()
        {
            MatchCollection matches = Regex.Matches(iCalText, @"((\r(?=[^\n]))|((?<=[^\r])\n))");            
            if (matches.Count > 0)
                return new IValidationError[] { new ValidationErrorWithLookup("crlfPairError", ValidationErrorType.Warning) };

            return new IValidationError[0];
        }

        #endregion
    }
}
