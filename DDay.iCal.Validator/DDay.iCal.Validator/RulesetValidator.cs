using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Serialization;
using System.IO;
using System.Xml;
using System.Reflection;

namespace DDay.iCal.Validator.RFC2445
{
    public class RulesetValidator :
        IValidator,
        ICalendarTestable
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

        public RulesetValidator(IValidationRuleset ruleset)
        {
            _Ruleset = ruleset;
        }

        public RulesetValidator(IValidationRuleset ruleset, string text) :
            this(ruleset)
        {
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

        #region ICalendarTestable Members

        public ICalendarTest[] Tests
        {
            get
            {
                if (Ruleset != null)
                {
                    List<ICalendarTest> tests = new List<ICalendarTest>();
                    foreach (IValidationRule rule in Ruleset.Rules)
                    {
                        foreach (ICalendarTest test in rule.Tests)
                            tests.Add(test);
                    }

                    return tests.ToArray();
                }
                return new ICalendarTest[0];
            }
        }

        public ICalendarTestResult[] Test()
        {
            if (Ruleset != null)
            {
                List<ICalendarTestResult> results = new List<ICalendarTestResult>();
                foreach (IValidationRule rule in Ruleset.Rules)
                {
                    foreach (ICalendarTest test in rule.Tests)
                    {
                        RulesetValidator validator = new RulesetValidator(Ruleset, test.iCalendarText);

                        List<IValidationError> errors = new List<IValidationError>();
                        errors.AddRange(validator.Validate());

                        CalendarTestResult result = new CalendarTestResult(rule.Name, test);

                        if (test.Type == TestType.Fail)
                        {
                            if (errors.Count == 1 && !string.Equals(errors[0].Name, test.ExpectedError))
                                result.Error = new CalendarTestError("failWithIncorrectError", rule.Name, errors.ToArray());
                            else if (errors.Count == 0)
                                result.Error = new CalendarTestError("failExpectedError", rule.Name, errors.ToArray());
                            else if (errors.Count > 1)
                                result.Error = new CalendarTestError("failWithMoreThanOneError", rule.Name, errors.ToArray());
                            else
                                result.Passed = true;
                        }
                        else 
                        {
                            if (errors.Count > 0)
                                result.Error = new CalendarTestError("passExpectedError", rule.Name, errors.ToArray());
                            else
                                result.Passed = true;
                        }

                        results.Add(result);
                    }
                }

                return results.ToArray();
            }
            // FIXME: else throw exception?
            else return new ICalendarTestResult[0];            
        }

        #endregion
    }
}
