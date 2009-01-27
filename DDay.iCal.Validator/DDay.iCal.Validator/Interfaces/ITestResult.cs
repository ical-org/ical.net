using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ITestResult
    {
        /// <summary>
        /// Gets the name of the rule that initiated the test.
        /// </summary>
        string Rule { get; }

        /// <summary>
        /// Gets a reference to the test itself.
        /// </summary>
        ITest Test { get; }

        /// <summary>
        /// Gets whether or not the test passed.  Possible values are:
        /// True - the test passed.
        /// False - the test failed.
        /// Null - the test was not run.
        /// </summary>
        bool? Passed { get; }

        /// <summary>
        /// Gets the error (if any) that occurred during the test.
        /// </summary>
        ITestError Error { get; }
    }
}
