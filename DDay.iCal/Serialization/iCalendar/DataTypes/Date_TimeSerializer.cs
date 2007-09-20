using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class Date_TimeSerializer : FieldSerializer
    {
        #region Private Fields

        private Date_Time m_DateTime;
        static private List<Parameter> m_DisallowedParameters;

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
            m_DisallowedParameters = new List<Parameter>();

            m_DisallowedParameters.Add(new Parameter("TZID"));
            m_DisallowedParameters.Add(new Parameter("VALUE"));
        }

        public Date_TimeSerializer(Date_Time dt) : base(dt)
        {
            this.m_DateTime = dt;
        }

        #endregion

        #region ISerializable Members

        public override List<Parameter> DisallowedParameters
        {
            get
            {
                return m_DisallowedParameters;
            }
        }

        public override List<Parameter> Parameters
        {
            get
            {
                List<Parameter> Params = base.Parameters;

                if (m_DateTime.TZID != null)
                    Params.Add(new Parameter("TZID", m_DateTime.TZID.ToString()));

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
                    Params.Add(new Parameter("VALUE", valueType));
                
                return Params;
            }
        }

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
