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

        public IValidationError[] Validate()
        {       
            List<IValidationError> errors = new List<IValidationError>();
            
            if (!iCalendar.Properties.ContainsKey("PRODID"))
            {
                errors.Add(new ValidationErrorWithLookup("prodIDRequiredError", ValidationErrorType.Warning, false));
            }
            else if (iCalendar.Properties.CountOf("PRODID") > 1)
            {
                errors.Add(new ValidationErrorWithLookup("prodIDOnlyOnceError", ValidationErrorType.Error, false));
            }

            return errors.ToArray();
        }

        #endregion
    }
}
