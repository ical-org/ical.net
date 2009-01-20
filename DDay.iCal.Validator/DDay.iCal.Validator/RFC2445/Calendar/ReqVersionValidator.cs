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

        public IValidationError[] Validate()
        {
            List<IValidationError> errors = new List<IValidationError>();

            if (!iCalendar.Properties.ContainsKey("VERSION"))
            {
                errors.Add(new ValidationErrorWithLookup("versionRequiredError", ValidationErrorType.Warning, false));
            }
            else if (iCalendar.Properties.CountOf("VERSION") > 1)
            {
                errors.Add(new ValidationErrorWithLookup("versionOnlyOnceError", ValidationErrorType.Error, false));
            }
            else
            {
                try
                {
                    Version v = new Version(iCalendar.Version);
                    Version req = new Version(2, 0);
                    if (req > v)
                        errors.Add(new ValidationErrorWithLookup("versionGE2_0Error", ValidationErrorType.Error, false));
                }
                catch
                {
                    errors.Add(new ValidationErrorWithLookup("versionNumberError", ValidationErrorType.Error, false));                    
                }
            }

            return errors.ToArray();
        }

        #endregion
    }
}
