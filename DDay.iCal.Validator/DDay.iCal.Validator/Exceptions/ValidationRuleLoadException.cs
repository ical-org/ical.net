using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationRuleLoadException :
        Exception
    {
        public IValidationRule ValidationRule { get; set; }

        public ValidationRuleLoadException(IValidationRule rule) :
            base("The '" + rule.Name + "' validation rule failed to load.")
        {
            ValidationRule = rule;
        }
    }
}
