using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Validator
{
    public class PropertyCountValidator :
        IValidator
    {
        #region Public Properties

        public iCalObject iCalObject { get; set; }
        public string PropertyName { get; set; }
        public string ComponentName { get; set; }
        public int MinCount { get; set; }
        public int MaxCount { get; set; }

        #endregion

        #region Constructors

        public PropertyCountValidator(iCalObject icalObject, string componentName, string propertyName)
        {
            MinCount = 0;
            MaxCount = 1;

            iCalObject = icalObject;
            ComponentName = componentName;
            PropertyName = propertyName;            
        }

        public PropertyCountValidator(iCalObject icalObject, string componentName, string propertyName, int minOccurrences) :
            this(icalObject, componentName, propertyName, minOccurrences, int.MaxValue)
        {            
        }

        public PropertyCountValidator(iCalObject icalObject, string componentName, string propertyName, int minOccurrences, int maxOccurrences) :
            this(icalObject, componentName, propertyName)
        {
            MinCount = minOccurrences;
            MaxCount = maxOccurrences;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult();
            List<IValidationError> errors = new List<IValidationError>();

            if (!iCalObject.Properties.ContainsKey(PropertyName))
            {
                if (MinCount > 0)
                {
                    result.Passed = false;
                    ValidationErrorWithLookup error = new ValidationErrorWithLookup("propertyRequiredError", ValidationErrorType.Error, false);
                    error.Message = string.Format(error.Message, PropertyName, ComponentName);
                    errors.Add(error);
                }
            }
            else
            {
                if (MaxCount == 1 && iCalObject.Properties.CountOf(PropertyName) > 1)
                {
                    result.Passed = false;
                    ValidationErrorWithLookup error = new ValidationErrorWithLookup("propertyOnlyOnceError", ValidationErrorType.Error, false);
                    error.Message = string.Format(error.Message, PropertyName, ComponentName);
                    errors.Add(error);
                }
                else if (iCalObject.Properties.CountOf(PropertyName) > MaxCount)
                {
                    result.Passed = false;
                    ValidationErrorWithLookup error = new ValidationErrorWithLookup("propertyCountRangeError", ValidationErrorType.Error, false);
                    error.Message = string.Format(error.Message, PropertyName, ComponentName, MinCount, MaxCount);
                    errors.Add(error);
                }
                else
                {
                    result.Passed = true;
                }
            }            

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
