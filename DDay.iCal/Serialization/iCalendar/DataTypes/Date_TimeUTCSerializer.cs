using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class Date_TimeUTCSerializer : Date_TimeSerializer
    {
        #region Constructors

        public Date_TimeUTCSerializer(Date_Time dt)
            : base(dt)
        {
            // Make a copy of the Date_Time object, so we don't alter
            // the original
            DateTime = dt.Copy();

            // Set the Date_Time object to UTC time
            DateTime = DateTime.UTC;

            // Ensure time is serialized
            DateTime.HasTime = true;

            // FIXME: this is the old way we did it; remove when verified
            //DateTime.SetKind(DateTimeKind.Utc);            
        }

        #endregion
    }
}
