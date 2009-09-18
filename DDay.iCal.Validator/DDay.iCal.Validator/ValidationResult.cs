using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class ValidationResult : 
        IValidationResult
    {
        #region Static Public Methods

        static public ValidationResult GetCompositeResults(string rule, params IValidator[] validators)
        {
            ValidationResult result = new ValidationResult(rule);
            result.Passed = true;

            List<IValidationError> errors = new List<IValidationError>();

            foreach (IValidator validator in validators)
            {
                foreach (IValidationResult r in validator.Validate())
                {
                    if (r.Passed != null && r.Passed.HasValue && !r.Passed.Value)
                    {
                        result.Passed = false;
                        errors.AddRange(r.Errors);
                    }
                }
            }

            result.Errors = errors.ToArray();
            return result;
        }

        #endregion

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

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Rule);
            sb.Append(": ");
            if (Passed != null)
            {
                if (BoolUtil.IsTrue(Passed))
                    sb.Append(ResourceManager.GetString("pass"));
                else
                    sb.Append(ResourceManager.GetString("fail"));
            }
            else sb.Append(ResourceManager.GetString("didNotRun"));

            return sb.ToString();
        }

        #endregion

        #region IValidationResult Members

        virtual public string Rule { get; set; }
        virtual public bool? Passed { get; set; }
        virtual public IValidationError[] Errors { get; set; }

        #endregion
    }
}
