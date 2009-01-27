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
        ITestable
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

        public IValidationResult[] Validate()
        {
            if (Ruleset != null)
            {
                List<IValidationResult> results = new List<IValidationResult>();
                foreach (IValidationRule rule in Ruleset.Rules)
                {
                    IValidator validator = null;

                    Type validatorType = rule.ValidatorType;
                    if (validatorType != null)
                        validator = ValidatorActivator.Create(validatorType, iCalendar, iCalendarText);

                    if (validator == null)
                    {
                        results.Add(
                            new ValidationResult(
                                rule.Name,
                                false,
                                new IValidationError[] { 
                                    new ValidationError(null, "Validator for rule '" + rule.Name + "' could not be determined!")
                                }
                            )
                        );
                    }
                    else
                    {
                        results.AddRange(validator.Validate());
                    }
                }

                return results.ToArray();
            }
            else return new IValidationResult[0];            
        }

        #endregion

        #region ICalendarTestable Members

        public ITest[] Tests
        {
            get
            {
                if (Ruleset != null)
                {
                    List<ITest> tests = new List<ITest>();
                    foreach (IValidationRule rule in Ruleset.Rules)
                    {
                        foreach (ITest test in rule.Tests)
                            tests.Add(test);
                    }

                    return tests.ToArray();
                }
                return new ITest[0];
            }
        }

        public ITestResult[] Test()
        {
            if (Ruleset != null)
            {
                List<ITestResult> results = new List<ITestResult>();
                foreach (IValidationRule rule in Ruleset.Rules)
                {
                    foreach (ITest test in rule.Tests)
                    {
                        RulesetValidator validator = new RulesetValidator(Ruleset, test.iCalendarText);

                        List<IValidationResult> validationResults = new List<IValidationResult>();
                        validationResults.AddRange(validator.Validate());

                        TestResult result = new TestResult(rule.Name, test, false);
                        if (test.Type == TestType.Fail)
                        {
                            List<IValidationResult> badResults = validationResults.FindAll(
                                delegate(IValidationResult r)
                                {
                                    return !BoolUtil.IsTrue(r.Passed);
                                });

                            // On a failing test, there should always be bad results.
                            if (badResults.Count > 0)
                            {
                                // Get a list of errors from our results
                                List<IValidationError> errors = new List<IValidationError>();
                                foreach (IValidationResult r in badResults)
                                    errors.AddRange(r.Errors);

                                if (errors.Count == 1 && !string.Equals(errors[0].Name, test.ExpectedError))
                                    result.Error = new TestError("failWithIncorrectError", rule.Name, validationResults.ToArray());
                                else if (errors.Count == 0)
                                    result.Error = new TestError("failExpectedError", rule.Name, validationResults.ToArray());
                                else if (errors.Count > 1)
                                    result.Error = new TestError("failWithMoreThanOneError", rule.Name, validationResults.ToArray());
                                else
                                    result.Passed = true;
                            }
                        }
                        else 
                        {
                            result.Passed = true;
                            if (validationResults.FindIndex(
                                delegate(IValidationResult r)
                                {
                                    return !BoolUtil.IsTrue(r.Passed);
                                }) >= 0)
                            {
                                result.Passed = false;
                                result.Error = new TestError("passExpectedError", rule.Name, validationResults.ToArray());
                            }
                        }

                        results.Add(result);
                    }
                }

                return results.ToArray();
            }
            // FIXME: else throw exception?
            else return new ITestResult[0];            
        }

        #endregion
    }
}
