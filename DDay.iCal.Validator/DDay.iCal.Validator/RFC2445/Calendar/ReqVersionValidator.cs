using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    public class ReqVersionValidator : 
        IValidator
    {
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public ReqVersionValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("reqVersion");
            List<IValidationError> errors = new List<IValidationError>();

            if (!iCalendar.Properties.ContainsKey("VERSION"))
            {
                result.Passed = false;
                errors.Add(new ValidationErrorWithLookup("versionRequiredError", ValidationErrorType.Warning, false));
            }
            else if (iCalendar.Properties.CountOf("VERSION") > 1)
            {
                result.Passed = false;
                errors.Add(new ValidationErrorWithLookup("versionOnlyOnceError", ValidationErrorType.Error, false));
            }
            else
            {
                try
                {
                    result.Passed = true;

                    Version v = new Version(iCalendar.Version);
                    Version req = new Version(2, 0);
                    if (req > v)
                    {
                        result.Passed = false;
                        errors.Add(new ValidationErrorWithLookup("versionGE2_0Error", ValidationErrorType.Error, false));
                    }
                }
                catch
                {
                    result.Passed = false;
                    errors.Add(new ValidationErrorWithLookup("versionNumberError", ValidationErrorType.Error, false));                    
                }
            }

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
