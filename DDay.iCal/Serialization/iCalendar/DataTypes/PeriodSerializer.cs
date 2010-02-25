using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class PeriodSerializer : FieldSerializer
    {
        #region Private Fields

        private Period m_Period;

        #endregion

        #region Constructors

        public PeriodSerializer(Period p)
            : base(p)
        {
            this.m_Period = p;
        }

        #endregion

        #region ISerializable Members

        public override string SerializeToString()
        {
            string value = string.Empty;

            ISerializable serializer = SerializerFactory.Create(m_Period.StartTime, SerializationContext);
            if (serializer != null)
                value += serializer.SerializeToString();

            if (m_Period.Duration != null)
            {
                value += "/";

                serializer = SerializerFactory.Create(m_Period.Duration, SerializationContext);
                if (serializer != null)
                    value += serializer.SerializeToString();
            }

            return value;
        }        

        #endregion
    }
}
