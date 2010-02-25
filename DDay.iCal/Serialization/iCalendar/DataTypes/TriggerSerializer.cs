using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class TriggerSerializer : FieldSerializer
    {
        #region Private Fields

        private Trigger m_Trigger;

        #endregion

        #region Constructors

        public TriggerSerializer(Trigger t)
            : base(t)
        {
            this.m_Trigger = t;
        }

        #endregion

        #region ISerializable Members

        public override List<ICalendarParameter> Parameters
        {
            get
            {
                List<ICalendarParameter> Parameters = base.Parameters;
                if (m_Trigger.ValueType() == typeof(iCalDateTime))
                    Parameters.Add(new CalendarParameter("VALUE", "DATE-TIME"));
                if (m_Trigger.Related == Trigger.TriggerRelation.End)
                    Parameters.Add(new CalendarParameter("RELATED", "END"));
                return Parameters;
            }
        }

        public override List<ICalendarParameter> DisallowedParameters
        {
            get
            {
                List<ICalendarParameter> disallowed = base.DisallowedParameters;
                disallowed.Add(new CalendarParameter("VALUE"));
                disallowed.Add(new CalendarParameter("RELATED"));
                return disallowed;
            }
        }

        public override string SerializeToString()
        {
            string value = string.Empty;
            ISerializable serializer = null;
            if (m_Trigger.ValueType() == typeof(iCalDateTime))            
                serializer = new iCalDateTimeSerializer(m_Trigger.DateTime);                
            else
                serializer = new FieldSerializer(m_Trigger.Duration);

            if (serializer != null)
                value += serializer.SerializeToString();

            return value;
        }        

        #endregion
    }
}
