using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface IValidationRule :
        ITestProvider
    {
        string Name { get; }
        Type ValidatorType { get; }
    }
}
