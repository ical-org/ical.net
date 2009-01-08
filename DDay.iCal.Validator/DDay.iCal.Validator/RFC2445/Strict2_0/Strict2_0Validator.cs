using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Validator.RFC2445.Strict2_0.Calendar;
using System.IO;

namespace DDay.iCal.Validator.RFC2445
{
    /// <summary>
    /// Validates iCalendars against the RFC2445 in a strict
    /// manner for version 2.0 calendars.
    /// </summary>
    public class Strict2_0Validator :
        IValidator
    {
        #region Private Fields

        private IValidationError _RecognitionError = null;

        #endregion

        #region Public Properties

        public string iCalendarText { get; set; }
        public iCalendar iCalendar { get; set; }

        #endregion

        #region Constructors

        public Strict2_0Validator(string text)
        {
            iCalendarText = text;

            try
            {
                iCalendar = iCalendar.LoadFromStream(new StringReader(text));
            }
            catch (antlr.RecognitionException ex)
            {
                _RecognitionError = new CalendarParseError(ex.line, ex.column, ex.Message);
            }
        }

        #endregion

        #region IValidator Members

        public IValidationError[] Validate()
        {            
            List<IValidationError> errors = new List<IValidationError>();
            if (_RecognitionError != null)
            {
                errors.Add(_RecognitionError);
            }
            else
            {
                if (iCalendar == null)
                    errors.Add(new CalendarNotLoadedError());
                else
                {
                    errors.AddRange(new CalendarVersionValidator(iCalendar).Validate());
                }
            }

            return errors.ToArray();
        }

        #endregion
    }
}
