using System;
using System.Collections.Generic;
using System.Text;
using System.Resources;
using System.Reflection;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    public class EventEndDurationConflictValidator : 
        IValidator
    {
        #region Public Properties

        public string iCalText { get; set; }

        #endregion

        #region Constructors

        public EventEndDurationConflictValidator(string cal_text)
        {
            iCalText = cal_text;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            bool started = false;
            bool hasDuration = false;
            bool hasEnd = false;
            bool errorAdded = false;
            int lineNumber = 0;

            ValidationResult result = new ValidationResult("eventEndDurationConflict");
            List<IValidationError> errors = new List<IValidationError>();

            StringReader sr = new StringReader(iCalText);
            string line = sr.ReadLine();
            while (line != null)
            {
                lineNumber++;

                if (!started)
                {
                    if (line.StartsWith("BEGIN:VEVENT", StringComparison.InvariantCultureIgnoreCase))
                        started = true;
                }
                else
                {
                    if (line.StartsWith("END:VEVENT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        started = false;
                        hasDuration = false;
                        hasEnd = false;
                        errorAdded = false;
                    }
                    else if (
                        line.StartsWith("DTEND;", StringComparison.InvariantCultureIgnoreCase) ||
                        line.StartsWith("DTEND:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasEnd = true;
                    }
                    else if (
                        line.StartsWith("DURATION;", StringComparison.InvariantCultureIgnoreCase) ||
                        line.StartsWith("DURATION:", StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasDuration = true;
                    }

                    if (!errorAdded && hasEnd && hasDuration)
                    {
                        errors.Add(new ValidationErrorWithLookup("eventEndDurationConflictError", ValidationErrorType.Error, false, lineNumber, 0));
                        errorAdded = true;
                    }
                }
                line = sr.ReadLine();
            }

            if (errors.Count == 0)
            {
                result.Passed = true;
            }
            else
            {
                result.Passed = false;
                result.Errors = errors.ToArray();
            }
            return new IValidationResult[] { result };
        }

        #endregion
    }
}
