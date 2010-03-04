using System;
using System.Collections.Generic;
using System.Text;

namespace DDay.iCal
{
    public class PropertyParser :
        IStringParser
    {
        #region IStringParser Members

        public void Parse(string value, object obj)
        {
            ICalendarProperty p = obj as ICalendarProperty;
            if (p != null)
            {
                // FIXME: this doesn't really do much, but at least
                // the value gets parsed into an object model.
                p.Value = value;
            }
        }

        #endregion
    }
}
