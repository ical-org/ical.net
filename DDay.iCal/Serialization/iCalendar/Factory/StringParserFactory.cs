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
            return null;
        }

        #endregion
    }
}
