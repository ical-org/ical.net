using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class Date_TimeSerializer : FieldSerializer
    {
        #region Private Fields

        private Date_Time m_DateTime;

        #endregion

        #region Public Properties

        public Date_Time DateTime
        {
            get { return m_DateTime; }
            set { m_DateTime = value; }
        }

        #endregion

        #region Constructors

        public Date_TimeSerializer(Date_Time dt) : base(dt)
        {
            this.m_DateTime = dt;
        }

        #endregion

        #region ISerializable Members

        public override string SerializeToString()
        {
            Type type = GetType();            

            // Let's first see if we need to force this
            // date-time value into UTC time
            if (type != typeof(Date_TimeUTCSerializer) && !type.IsSubclassOf(typeof(Date_TimeUTCSerializer)))
            {
                foreach (object obj in m_DateTime.Attributes)
                {
                    if (obj is ForceUTCAttribute)
                    {
                        Date_TimeUTCSerializer serializer = new Date_TimeUTCSerializer(m_DateTime);
                        return serializer.SerializeToString();
                    }
                }
            }

            string value = string.Empty;
            value += string.Format("{0:0000}{1:00}{2:00}", m_DateTime.Year, m_DateTime.Month, m_DateTime.Day);
            if (m_DateTime.HasTime)
            {
                value += string.Format("T{0:00}{1:00}{2:00}", m_DateTime.Hour, m_DateTime.Minute, m_DateTime.Second);
                if (m_DateTime.Kind == DateTimeKind.Utc)
                    value += "Z";
            }
            return value;
        }        

        #endregion

        #region Private Methods

        private string InvertType(string type)
        {
            switch (type)
            {
                case "DATE": return "DATE-TIME";
                case "DATE-TIME": return "DATE";
                default: return "DATE-TIME";
            }
        }

        #endregion
    }
}
