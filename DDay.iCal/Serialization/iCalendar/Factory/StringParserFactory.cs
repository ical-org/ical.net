using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace DDay.iCal.Serialization
{
    public class StringParserFactory :
        IStringParserFactory
    {
        #region IStringParserFactory Members

        virtual public IStringParser Create(Type objectType)
        {
            if (typeof(ICalendarProperty).IsAssignableFrom(objectType))
                return new PropertyParser();
            return null;
        }

        #endregion
    }
}
