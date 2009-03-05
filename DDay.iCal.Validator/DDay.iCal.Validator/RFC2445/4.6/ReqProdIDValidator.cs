using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    public class ReqProdIDValidator : 
        IValidator
    {
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public ReqProdIDValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("reqProdID");
            List<IValidationError> errors = new List<IValidationError>();
            
            if (!iCalendar.Properties.ContainsKey("PRODID"))
            {
                result.Passed = false;
                errors.Add(new ValidationErrorWithLookup("prodIDRequiredError", ValidationErrorType.Warning, false));
            }
            else if (iCalendar.Properties.CountOf("PRODID") > 1)
            {
                result.Passed = false;
                errors.Add(new ValidationErrorWithLookup("prodIDOnlyOnceError", ValidationErrorType.Error, false));
            }
            else
            {
                result.Passed = true;
            }

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
