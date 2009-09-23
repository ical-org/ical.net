using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.Validator.RFC2445
{
	public class EventCommentPropertyValidator :
        IValidator
	{
        #region Public Properties

        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public EventCommentPropertyValidator(iCalendar cal)
        {
            iCalendar = cal;
        }

        #endregion

        #region IValidator Members

        public IValidationResult[] Validate()
        {
            ValidationResult result = new ValidationResult("eventCommentProperty");
            result.Passed = true;

            List<IValidationError> errors = new List<IValidationError>();

            // FIXME: do some validation here

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
	}
}
