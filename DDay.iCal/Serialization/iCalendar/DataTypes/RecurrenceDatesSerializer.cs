using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    /// <summary>
    /// FIXME: Add "VALUE=PERIOD" when serializing a
    /// RecurrenceDate that contains periods.
    /// </summary>
    public class RecurrenceDatesSerializer : FieldSerializer
    {
        #region Private Fields

        private RecurrenceDates m_RDate;

        #endregion

        #region Constructors

        public RecurrenceDatesSerializer(RecurrenceDates rdate)
            : base(rdate)
        {
            this.m_RDate = rdate;
        }

        #endregion

        #region Overrides

        public override string SerializeToString()
        {
            List<string> values = new List<string>();                        
            foreach (Period p in m_RDate.Periods)
            {
                ISerializable serializer = SerializerFactory.Create(p, SerializationContext);
                if (serializer != null)
                    values.Add(serializer.SerializeToString());
            }

            return string.Join(",", values.ToArray());            
        }

        #endregion
    }
}
