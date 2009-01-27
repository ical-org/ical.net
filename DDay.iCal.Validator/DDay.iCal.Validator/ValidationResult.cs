using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationResult : 
        IValidationResult
    {
        #region Constructors

        public ValidationResult()
        {
        }

        public ValidationResult(string rule) : this()
        {
            Rule = rule;
        }

        public ValidationResult(string rule, bool? passed, IValidationError[] errors) :
            this(rule)
        {
            Passed = passed;
            Errors = errors;
        }

        #endregion

        #region IValidationResult Members

        virtual public string Rule { get; set; }
        virtual public bool? Passed { get; set; }
        virtual public IValidationError[] Errors { get; set; }

        #endregion
    }
}
