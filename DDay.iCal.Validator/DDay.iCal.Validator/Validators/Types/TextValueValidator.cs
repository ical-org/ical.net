using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using System.Text.RegularExpressions;

namespace DDay.iCal.Validator
{
    public class TextValueValidator :
        IValidator
    {
        #region Public Properties

        public Property Property { get; set; }        

        #endregion

        #region Constructors

        public TextValueValidator(Property property)
        {
            Property = property;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult();
            List<IValidationError> errors = new List<IValidationError>();

            if (Property != null)
            {
                result.Passed = true;

                if (Property.Value != null)
                {
                    // Find single commas
                    if (Regex.IsMatch(Property.Value, @"(?<!\\),"))
                    {
                        result.Passed = false;
                        ValidationErrorWithLookup error = new ValidationErrorWithLookup("textEscapeCommasError", ValidationErrorType.Error, false);
                        error.Message = string.Format(error.Message, Property.Name);
                        errors.Add(error);
                    }

                    // Find single semicolons
                    if (Regex.IsMatch(Property.Value, @"(?<!\\);"))
                    {
                        result.Passed = false;
                        ValidationErrorWithLookup error = new ValidationErrorWithLookup("textEscapeSemicolonsError", ValidationErrorType.Error, false);
                        error.Message = string.Format(error.Message, Property.Name);
                        errors.Add(error);
                    }

                    // Find backslashes that are not escaped
                    if (Regex.IsMatch(Property.Value, @"\\([^\\nN;,])"))
                    {
                        result.Passed = false;
                        ValidationErrorWithLookup error = new ValidationErrorWithLookup("textEscapeBackslashesError", ValidationErrorType.Error, false);
                        error.Message = string.Format(error.Message, Property.Name);
                        errors.Add(error);
                    }
                }
            }

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
