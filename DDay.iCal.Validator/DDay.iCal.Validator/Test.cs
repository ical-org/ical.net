using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Validator
{
    public class Test : 
        ITest
    {
        #region ICalendarTest Members

        virtual public TestType Type { get; protected set; }
        virtual public string iCalendarText { get; protected set; }
        virtual public string ExpectedError { get; protected set; }

        #endregion        
    }
}
