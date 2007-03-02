using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.Components
{
    public class PropertySerializer : ISerializable, IParameterSerializable
    {
        #region Private Fields

        private Property m_Property;

        #endregion

        #region Constructors

        public PropertySerializer(Property property)
        {
            this.m_Property = property;
        }

        #endregion

        #region ISerializable Members

        public string SerializeToString()
        {
            string value = m_Property.Name;            
            if (m_Property.Parameters.Count > 0)
                value += ";" + string.Join(";", Parameters.ToArray());            
            return value + ":" + m_Property.Value + "\r\n";
        }

        public void Serialize(Stream stream, Encoding encoding)
        {
            byte[] prop = encoding.GetBytes(SerializeToString());
            stream.Write(prop, 0, prop.Length);
        }

        #endregion

        #region IParameterSerializable Members

        public List<string> Parameters
        {
            get
            {
                List<string> Parameters = new List<string>();
                foreach (DictionaryEntry de in m_Property.Parameters)
                {
                    if (!DisallowedParameters.Contains(de.Key.ToString()))
                        Parameters.Add(de.Key + "=" + string.Join(",", ((Parameter)de.Value).Values.ToArray()));
                }
                return Parameters;
            }
        }

        public List<string> DisallowedParameters
        {
            get { return new List<string>(); }
        }

        #endregion
    }
}
