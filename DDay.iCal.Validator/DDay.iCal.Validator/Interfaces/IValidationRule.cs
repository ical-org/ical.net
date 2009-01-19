using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface IValidationRule :
        IValidator // Rules can self-validate (i.e. Pass/Fail tests)
    {
        string Name { get; }
        Type ValidatorType { get; }
    }
}
