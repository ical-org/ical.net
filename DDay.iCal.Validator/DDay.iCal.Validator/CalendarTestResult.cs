using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class CalendarTestResult :
        ICalendarTestResult
    {
        #region Constructors

        public CalendarTestResult(string source, ICalendarTest test)
        {
            Source = source;
            Test = test;
        }

        public CalendarTestResult(string source, ICalendarTest test, ICalendarTestError error) :
            this(source, test)
        {
            Error = error;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Source ?? "Unknown");
            sb.Append(": ");
            sb.Append(Passed ? ResourceManager.GetString("pass") : ResourceManager.GetString("fail"));
            if (Error != null)
                sb.Append(Environment.NewLine + Error.ToString());

            return sb.ToString();
        }

        #endregion

        #region ICalendarTestResult Members

        virtual public string Source { get; protected set; }
        virtual public bool Passed { get; set; }
        virtual public ICalendarTest Test { get; protected set; }
        virtual public ICalendarTestError Error { get; set; }

        #endregion
    }
}
