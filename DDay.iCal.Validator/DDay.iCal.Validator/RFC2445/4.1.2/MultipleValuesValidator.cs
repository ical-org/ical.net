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
    public class MultipleValuesValidator : 
        IValidator
    {
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public MultipleValuesValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("multipleValues");

            if (iCalendar != null)
                result.Passed = true;
            else
                result.Passed = false;

            return new IValidationResult[] { result };
        }

        #endregion
    }
}
