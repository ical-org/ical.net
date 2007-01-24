using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Objects;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class Date_TimeSerializer : FieldSerializer
    {
        #region Private Fields

        private Date_Time m_DateTime;
        static private List<string> m_DisallowedParameters;

        #endregion

        #region Public Properties

        public Date_Time DateTime
        {
            get { return m_DateTime; }
            set { m_DateTime = value; }
        }

        #endregion

        #region Constructors

        static Date_TimeSerializer()
        {
            m_DisallowedParameters = new List<string>();
            m_DisallowedParameters.Add("TZID");
            m_DisallowedParameters.Add("VALUE");
        }

        public Date_TimeSerializer(Date_Time dt) : base(dt)
        {
            this.m_DateTime = dt;
        }

        #endregion

        #region ISerializable Members

        public override List<string> DisallowedParameters
        {
            get
            {
                return m_DisallowedParameters;
            }
        }

        public override List<string> Parameters
        {
            get
            {
                List<string> Params = base.Parameters;

                if (m_DateTime.TZID != null)
                    Params.Add("TZID=" + m_DateTime.TZID.ToString());

                string valueType = null;
                if (!m_DateTime.HasTime)
                    valueType = "DATE";
                else valueType = "DATE-TIME";

                foreach (object obj in m_DateTime.Attributes)
                {
                    if (obj is DefaultValueTypeAttribute)
                    {
                        if (((DefaultValueTypeAttribute)obj).Type == valueType)
                            valueType = null;
                    }
                }

                // If the value type is already the default value type, don't worry about displaying it
                if (valueType != null)
                    Params.Add("VALUE=" + valueType);
                
                return Params;
            }
        }

        public override string SerializeToString()
        {
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
    }
}
