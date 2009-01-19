using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface IValidationRuleset
    {
        string Name { get; }
        string Description { get; }
        IValidationRule[] Rules { get; }
    }
}
