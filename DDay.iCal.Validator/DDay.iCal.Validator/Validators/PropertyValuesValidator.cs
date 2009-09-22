using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Validator
{
    public class PropertyValuesValidator :
        IValidator
    {
        #region Public Properties

        public iCalObject iCalObject { get; set; }
        public string PropertyName { get; set; }
        public string ComponentName { get; set; }
        public List<string> PossibleValues { get; set; }

        #endregion

        #region Constructors

        public PropertyValuesValidator(iCalObject icalObject, string componentName, string propertyName, params string[] possibleValues)
        {
            iCalObject = icalObject;
            ComponentName = componentName;
            PropertyName = propertyName;
            PossibleValues = new List<string>(possibleValues);
        }
        
        public PropertyValuesValidator(iCalObject icalObject, string componentName, string propertyName, List<string> possibleValues)
        {
            iCalObject = icalObject;
            ComponentName = componentName;
            PropertyName = propertyName;
            PossibleValues = possibleValues;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult();
            result.Passed = true;

            List<IValidationError> errors = new List<IValidationError>();

            if (iCalObject.Properties.ContainsKey(PropertyName))
            {
                Property p = iCalObject.Properties[PropertyName];
                if (!PossibleValues.Contains(p.Value))
                {
                    result.Passed = false;
                    ValidationErrorWithLookup error = new ValidationErrorWithLookup("propertyInvalidValueWarning", ValidationErrorType.Error, false);
                    error.Message = string.Format(error.Message, PropertyName, ComponentName, p.Value, string.Join(", ", PossibleValues.ToArray()));
                    errors.Add(error);
                }
            }

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
