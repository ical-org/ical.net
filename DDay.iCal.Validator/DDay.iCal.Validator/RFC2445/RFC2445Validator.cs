using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization;
using System.IO;
using System.Xml;
using System.Reflection;

namespace DDay.iCal.Validator.RFC2445
{
    public class RFC2445Validator :
        IValidator
    {
        #region Private Fields

        private IValidationError _RecognitionError = null;
        private IValidationRuleset _Ruleset;

        #endregion

        #region Public Properties

        public string iCalendarText { get; set; }
        public iCalendar iCalendar { get; set; }
        public IValidationRuleset Ruleset { get { return _Ruleset; } }

        #endregion

        #region Constructors

        public RFC2445Validator(IValidationRuleset ruleset, string text)
        {
            _Ruleset = ruleset;
            iCalendarText = text;

            try
            {
                iCalendarSerializer serializer = new iCalendarSerializer();
                
                // Turn off speed optimization so line/col from
                // antlr are accurate.
                serializer.OptimizeForSpeed = false;

                iCalendar = serializer.Deserialize(new StringReader(text), typeof(iCalendar)) as iCalendar;                
            }
            catch (antlr.RecognitionException ex)
            {
                _RecognitionError = new ValidationErrorWithLookup(
                    "calendarParseError", 
                    ValidationErrorType.Error, 
                    true, 
                    ex.line, 
                    ex.column);
            }
        }

        #endregion

        #region IValidator Members

        public IValidationError[] Validate()
        {
            if (Ruleset != null)
            {
                List<IValidationError> errors = new List<IValidationError>();
                foreach (IValidationRule rule in Ruleset.Rules)
                {
                    IValidator validator = null;

                    Type validatorType = rule.ValidatorType;
                    if (validatorType != null)
                        validator = ValidatorActivator.Create(validatorType, iCalendar, iCalendarText);                    

                    if (validator == null)
                    {
                        errors.Add(new ValidationError("Validator for rule '" + rule.Name + "' could not be determined!"));
                    }
                    else
                    {
                        errors.AddRange(validator.Validate());
                    }
                }

                return errors.ToArray();
            }
            else return new IValidationError[]
            {
                new ValidationError("No ruleset was selected for validation.")
            };
        }

        #endregion
    }
}
