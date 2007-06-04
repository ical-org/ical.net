using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.Components
{
    public class iCalObjectSerializer : ISerializable
    {
        #region Private Fields

        private DDay.iCal.Components.iCalObject m_object;

        #endregion

        #region Constructors

        public iCalObjectSerializer() { }
        public iCalObjectSerializer(DDay.iCal.Components.iCalObject iCalObject)
        {
            this.m_object = iCalObject;
        }

        #endregion

        #region ISerializable Members

        virtual public string SerializeToString()
        {
            return string.Empty;
        }
        
        virtual public List<string> DisallowedParameters
        {
            get
            {
                return new List<string>();
            }
        }

        virtual public void Serialize(Stream stream, Encoding encoding)
        {
            // Serialize "VERSION" before any other properties
            if (m_object.Properties.ContainsKey("VERSION"))
            {
                Property p = (Property)m_object.Properties["VERSION"];
                ISerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DictionaryEntry de in m_object.Properties)
            {
                // Don't serialize "VERSION" again, we've already done it above.
                if (de.Key.Equals("VERSION"))
                    continue; 

                Property p = (Property)de.Value;
                ISerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DictionaryEntry de in m_object.Parameters)
            {
                Parameter p = (Parameter)de.Value;
                ISerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DDay.iCal.Components.iCalObject obj in m_object.Children)
            {
                ISerializable serializer = SerializerFactory.Create(obj);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }
        }

        virtual public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            TextReader tr = new StreamReader(stream, encoding);
            return Deserialize(tr, iCalendarType);            
        }

        virtual public iCalObject Deserialize(TextReader tr, Type iCalendarType)
        {
            return null;
        }

        #endregion        
    }
}
