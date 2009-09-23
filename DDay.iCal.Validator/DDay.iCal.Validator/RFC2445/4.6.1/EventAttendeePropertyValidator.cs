using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Validator.RFC2445
{
	public class EventAttendeePropertyValidator :
        IValidator
	{
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public EventAttendeePropertyValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("eventAttendeeProperty");
            result.Passed = true;

            List<IValidationError> errors = new List<IValidationError>();

            // FIXME: do some validation here

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
	}
}
