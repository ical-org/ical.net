using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public interface ITest
    {
        /// <summary>
        /// The type of test.  'Pass' indicates that the
        /// test is expected to pass without any errors,
        /// and 'Fail' indicates that the test is expected
        /// to fail with exactly 1 error (<see cref="ExpectedError"/>).
        /// </summary>
        TestType Type { get; }

        /// <summary>
        /// The iCalendar text associated with the test.
        /// </summary>
        string iCalendarText { get; }

        /// <summary>
        /// Used when <see cref="Type"/> is 'Fail' to indicate
        /// the error that is expected when validating the
        /// iCalendar associated with the test.
        /// </summary>
        string ExpectedError { get; }
    }

    public enum TestType
    {
        Pass,
        Fail
    }
}
