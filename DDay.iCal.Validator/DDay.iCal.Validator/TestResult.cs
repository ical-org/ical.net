using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class TestResult :
        ITestResult
    {
        #region Constructors

        public TestResult(string rule, ITest test)
        {
            Rule = rule;
            Test = test;
        }

        public TestResult(string rule, ITest test, bool? passed)
            : this(rule, test)
        {
            Passed = passed;
        }

        public TestResult(string rule, ITest test, bool? passed, ITestError error) :
            this(rule, test, passed)
        {
            Error = error;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Rule ?? "Unknown");
            sb.Append(": ");

            if (Passed != null && Passed.HasValue)
            {
                sb.Append(Passed.Value ? ResourceManager.GetString("pass") : ResourceManager.GetString("fail"));
            }
            else
            {
                sb.Append(ResourceManager.GetString("didNotRun"));
            }

            if (Error != null)
                sb.Append(Environment.NewLine + Error.ToString());

            return sb.ToString();
        }

        #endregion

        #region ICalendarTestResult Members

        virtual public string Rule { get; protected set; }
        virtual public bool? Passed { get; set; }
        virtual public ITest Test { get; protected set; }
        virtual public ITestError Error { get; set; }

        #endregion
    }
}
