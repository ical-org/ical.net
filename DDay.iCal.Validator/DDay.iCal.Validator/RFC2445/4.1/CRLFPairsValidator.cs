using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace DDay.iCal.Validator.RFC2445
{
    public class CRLFPairsValidator : 
        IValidator
    {
        #region Public Properties

        public string iCalText { get; set; }

        #endregion

        #region Constructors

        public CRLFPairsValidator(string cal_text)
        {
            iCalText = cal_text;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("crlfPairs");

            MatchCollection matches = Regex.Matches(iCalText, @"((\r(?=[^\n]))|((?<=[^\r])\n))");
            if (matches.Count > 0)
            {
                result.Passed = false;
                result.Errors = new IValidationError[] { new ValidationErrorWithLookup("crlfPairError", ValidationErrorType.Warning) };
            }
            else
            {
                result.Passed = true;
            }

            return new IValidationResult[] { result };
        }

        #endregion
    }
}
