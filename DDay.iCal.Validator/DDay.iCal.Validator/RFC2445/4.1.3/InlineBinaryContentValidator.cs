using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;

namespace DDay.iCal.Validator.RFC2445
{
    public class InlineBinaryContentValidator : 
        IValidator
    {
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public InlineBinaryContentValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("inlineBinaryContent");

            result.Passed = true;
            if (iCalendar != null)
            {                
                foreach (UniqueComponent uc in iCalendar.UniqueComponents)
                {
                    if (uc.Attach != null)
                    {
                        foreach (Binary b in uc.Attach)
                        {
                            // Inline binary content (i.e. BASE64-encoded attachments) are not
                            // recommended, and should only be used when absolutely necessary.
                            if (string.Equals(b.Encoding, "BASE64", StringComparison.InvariantCultureIgnoreCase))
                            {
                                result.Passed = false;
                                result.Errors = new IValidationError[]
                                { 
                                    new ValidationErrorWithLookup("inlineBinaryContentError", ValidationErrorType.Warning, false)
                                };
                            }
                        }

                        if (result.Passed != null)
                            break;
                    }                    
                }
            }

            return new IValidationResult[] { result };
        }

        #endregion
    }
}
