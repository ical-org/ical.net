using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    public class VersionValidator : 
        IValidator
    {
        #region Public Properties

        public string iCalText { get; set; }
        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public VersionValidator(string icalText, iCalendar cal)
        {
            iCalText = icalText;
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = ValidationResult.GetCompositeResults("version",
                new PropertyCountValidator(iCalendar, "VCALENDAR", "VERSION", 1, 1)
            );

            List<IValidationError> errors = new List<IValidationError>();

            if (result.Passed.Value)
            {
                try
                {
                    // Ensure the "VERSION" property can be parsed as a version number.
                    Version v = new Version(iCalendar.Version);
                    Version req = new Version(2, 0);
                    if (req > v)
                    {
                        // Ensure the "VERSION" is at least version 2.0.
                        result.Passed = false;
                        errors.Add(new ValidationErrorWithLookup("versionGE2_0Error", ValidationErrorType.Error, false));
                    }

                    // Ensure the VERSION property is the first property encountered on the calendar.
                    StringReader sr = new StringReader(iCalText);
                    string line = sr.ReadLine();
                    bool hasBegun = false;
                    while (line != null)
                    {
                        if (hasBegun)
                        {
                            if (!line.StartsWith("VERSION", StringComparison.InvariantCultureIgnoreCase))
                            {
                                result.Passed = false;
                                errors.Add(new ValidationErrorWithLookup("versionFirstError", ValidationErrorType.Warning, false));
                            }
                            break;
                        }

                        if (line.StartsWith("BEGIN:VCALENDAR", StringComparison.InvariantCultureIgnoreCase))
                            hasBegun = true;

                        line = sr.ReadLine();
                    }
                }
                catch
                {
                    result.Passed = false;
                    errors.Add(new ValidationErrorWithLookup("versionNumberError", ValidationErrorType.Error, false));
                }

                result.Errors = errors.ToArray();
            }
            
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
