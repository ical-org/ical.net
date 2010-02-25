using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class iCalDateTimeUTCSerializer : iCalDateTimeSerializer
    {
        #region Constructors

        public iCalDateTimeUTCSerializer(iCalDateTime dt)
            : base(dt)
        {
            // Make a copy of the iCalDateTime object, so we don't alter
            // the original
            DateTime = dt.Copy();

            // Set the iCalDateTime object to UTC time
            DateTime = DateTime.UTC;

            // Ensure time is serialized
            DateTime.HasTime = true;
        }

        #endregion
    }
}
