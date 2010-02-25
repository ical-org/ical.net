using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class iCalDateTimeSerializer : FieldSerializer
    {
        #region Private Fields

        private iCalDateTime m_DateTime;
        static private List<ICalendarParameter> m_DisallowedParameters;

        #endregion

        #region Public Properties

        public iCalDateTime DateTime
        {
            get { return m_DateTime; }
            set { m_DateTime = value; }
        }

        #endregion

        #region Constructors

        static iCalDateTimeSerializer()
        {
            m_DisallowedParameters = new List<ICalendarParameter>();

            m_DisallowedParameters.Add(new CalendarParameter("TZID"));
            m_DisallowedParameters.Add(new CalendarParameter("VALUE"));
        }

        public iCalDateTimeSerializer(iCalDateTime dt) : base(dt)
        {
            this.m_DateTime = dt;
        }

        #endregion

        #region ISerializable Members

        public override List<ICalendarParameter> DisallowedParameters
        {
            get
            {
                return m_DisallowedParameters;
            }
        }

        public override List<ICalendarParameter> Parameters
        {
            get
            {
                List<ICalendarParameter> Params = base.Parameters;

                if (m_DateTime.TZID != null)
                    Params.Add(new CalendarParameter("TZID", m_DateTime.TZID.ToString()));

                string valueType = null;
                if (!m_DateTime.HasTime)
                    valueType = "DATE";
                else valueType = "DATE-TIME";

                // Check to see if one of the value types is
                // disallowed; if so, then invert the value type
                foreach (object obj in m_DateTime.Attributes)
                {
                    if (obj is DisallowedTypesAttribute)
                    {
                        DisallowedTypesAttribute dt = (DisallowedTypesAttribute)obj;
                        if (dt.Types.Contains(valueType) && dt.Types.Contains(InvertType(valueType)))
                            valueType = null;
                        else if (dt.Types.Contains(valueType))
                            valueType = InvertType(valueType);
                    }
                    else if (obj is ForceUTCAttribute)
                    {
                        valueType = "DATE-TIME";
                        break;
                    }
                }

                // Check to see if the value type is the default value type for this item                
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
                    Params.Add(new CalendarParameter("VALUE", valueType));
                
                return Params;
            }
        }

        public override string SerializeToString()
        {
            Type type = GetType();            

            // Let's first see if we need to force this
            // date-time value into UTC time
            if (type != typeof(iCalDateTimeUTCSerializer) && !type.IsSubclassOf(typeof(iCalDateTimeUTCSerializer)))
            {
                foreach (object obj in m_DateTime.Attributes)
                {
                    if (obj is ForceUTCAttribute)
                    {
                        iCalDateTimeUTCSerializer serializer = new iCalDateTimeUTCSerializer(m_DateTime);
                        return serializer.SerializeToString();
                    }
                }
            }

            string value = string.Empty;
            value += string.Format("{0:0000}{1:00}{2:00}", m_DateTime.Year, m_DateTime.Month, m_DateTime.Day);
            if (m_DateTime.HasTime)
            {
                value += string.Format("T{0:00}{1:00}{2:00}", m_DateTime.Hour, m_DateTime.Minute, m_DateTime.Second);
                if (m_DateTime.IsUniversalTime)
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
