using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationRule : 
        IValidationRule
    {
        #region Private Fields

        private ITest[] _Tests;        

        #endregion

        #region IValidationRule Members

        virtual public string Name { get; protected set; }
        virtual public Type ValidatorType { get; protected set; }

        #endregion        
    
        #region ICalendarTestProvider Members

        virtual public ITest[] Tests
        {
            get { return _Tests; }
            protected set { _Tests = value; }
        }

        #endregion
    }
}
