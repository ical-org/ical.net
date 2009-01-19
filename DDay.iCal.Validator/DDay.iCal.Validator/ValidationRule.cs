using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationRule : 
        IValidationRule
    {
        #region IValidationRule Members

        virtual public string Name { get; protected set; }
        virtual public Type ValidatorType { get; protected set; }

        #endregion

        #region IValidator Members

        virtual public IValidationError[] Validate()
        {
            if (ValidatorType != null)
            {
                // FIXME: how do we get the ICS contents here for validation?
                //IValidator validator = ValidatorActivator.Create(ValidatorType);
            }            
            return new IValidationError[0];
        }

        #endregion
    }
}
