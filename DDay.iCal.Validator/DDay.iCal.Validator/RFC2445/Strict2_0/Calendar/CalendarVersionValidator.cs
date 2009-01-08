using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DDay.iCal.Validator.RFC2445.Strict2_0.Calendar
{
    public class CalendarVersionValidator : 
        IValidator
    {
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public CalendarVersionValidator(iCalendar cal)
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
                errors.Add(new MissingVersionError(ValidationErrorType.Error, false));
            }
            else if (!iCalendar.Version.Equals("2.0"))
            {
                errors.Add(new VersionMismatchError());
            }            

            return errors.ToArray();
        }

        #endregion
    }
}
