using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Objects;

namespace DDay.iCal.Serialization.iCalendar.Objects
{
    public class iCalObjectSerializer : ISerializable
    {
        #region Private Fields

        private DDay.iCal.Objects.iCalObject m_object;

        #endregion

        #region Constructors

        public iCalObjectSerializer(DDay.iCal.Objects.iCalObject iCalObject)
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
            foreach (DictionaryEntry de in m_object.Properties)
            {
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

            foreach (DDay.iCal.Objects.iCalObject obj in m_object.Children)
            {
                ISerializable serializer = SerializerFactory.Create(obj);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }
        }

        #endregion
    }
}
