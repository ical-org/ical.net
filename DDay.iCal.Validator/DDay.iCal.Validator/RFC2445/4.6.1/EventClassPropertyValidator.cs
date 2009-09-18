using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Validator.RFC2445
{
	public class EventClassPropertyValidator :
        IValidator
	{
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public EventClassPropertyValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("eventClassProperty");
            result.Passed = true;

            List<IValidationError> errors = new List<IValidationError>();

            foreach (Event evt in iCalendar.Events)
            {                
                ValidationResult evtResult = ValidationResult.GetCompositeResults(
                    "eventClassProperty",
                    new PropertyCountValidator(evt, "VEVENT", "CLASS")                    
                );

                if (!evtResult.Passed.Value)
                {
                    result.Passed = false;
                    errors.AddRange(evtResult.Errors);
                }
                else if (evt.Class != null)
                {
                    switch (evt.Class.Value)
                    {
                        case "PUBLIC":
                        case "PRIVATE":
                        case "CONFIDENTIAL":
                            break;
                        default:
                            result.Passed = false;
                            errors.Add(new ValidationErrorWithLookup("eventClassValueWarning", ValidationErrorType.Warning, false));
                            break;
                    }
                }
            }

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
	}
}
