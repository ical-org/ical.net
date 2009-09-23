using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;

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

            foreach (Event evt in iCalendar.Events)
            {
                if (evt.Properties != null)
                {
                    foreach (Property p in evt.Properties)
                    {
                        if (p.Name.Equals("COMMENT", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ValidationResult evtResult = ValidationResult.GetCompositeResults(
                                "eventCommentProperty",
                                new TextValueValidator(p)
                            );

                            if (!evtResult.Passed.Value)
                            {
                                result.Passed = false;
                                errors.AddRange(evtResult.Errors);
                            }
                        }
                    }
                }                
            }

            result.Errors = errors.ToArray();
            return new IValidationResult[] { result };
        }

        #endregion
	}
}
