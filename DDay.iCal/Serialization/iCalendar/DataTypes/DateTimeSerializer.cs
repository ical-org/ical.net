using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal.Serialization
{
    public class DateTimeSerializer :
        PropertySerializer
    {
        #region Overrides

        public override string SerializeToString(ICalendarObject obj)
        {
            return base.SerializeToString(obj);
        } 

        #endregion
    }
}
