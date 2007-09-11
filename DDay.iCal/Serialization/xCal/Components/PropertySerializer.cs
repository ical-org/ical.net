using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using DDay.iCal.Components;
using System.IO;
using System.Collections;

namespace DDay.iCal.Serialization.xCal.Components
{
    public class PropertySerializer : ISerializable, IXCalSerializable, IParameterSerializable
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
            return m_Property.Value;            
        }

        public void Serialize(Stream stream, Encoding encoding)
        {
            XmlTextWriter xtw = new XmlTextWriter(stream, encoding);
            xtw.Formatting = Formatting.Indented;
            xtw.IndentChar = ' ';
            Serialize(xtw);
            xtw.Close();
        }

        public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
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
        
        #region IXCalSerializable Members

        public void Serialize(System.Xml.XmlTextWriter xtw)
        {
            xtw.WriteStartElement(m_Property.Name.ToLower());

            foreach (Parameter param in m_Property.Parameters)
                xtw.WriteAttributeString(param.Name.ToLower(), string.Join(",", param.Values.ToArray()));

            xtw.WriteString(SerializeToString());

            xtw.WriteEndElement();
        }

        public DDay.iCal.Components.iCalObject Deserialize(System.Xml.XmlTextReader xtr, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion        
    }
}
