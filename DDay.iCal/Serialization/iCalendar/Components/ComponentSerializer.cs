using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DDay.iCal.Serialization
{
    public class ComponentSerializer :
        SerializableBase
    {
        #region Overrides

        public override string SerializeToString(ICalendarObject obj)
        {
            throw new NotImplementedException();
        }

        public override object Deserialize(TextReader tr, Type iCalendarType)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
