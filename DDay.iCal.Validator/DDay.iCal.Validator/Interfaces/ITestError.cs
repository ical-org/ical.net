using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ITestError
    {
        string Name { get; }
        string Message { get; }
        string Source { get; }
        IValidationResult[] ValidationResults { get; }
    }
}
