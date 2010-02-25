using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class PropertySerializer : ISerializable, IParameterSerializable
    {
        #region Private Fields

        private CalendarProperty m_Property;
        private ISerializationContext m_SerializationContext;

        #endregion

        #region Constructors

        public PropertySerializer(CalendarProperty property)
        {
            this.m_SerializationContext = DDay.iCal.Serialization.SerializationContext.Default;
            this.m_Property = property;
        }

        #endregion

        #region ISerializable Members

        public ISerializationContext SerializationContext
        {
            get { return m_SerializationContext; }
            set { m_SerializationContext = value; }
        }

        public string SerializeToString()
        {
            StringBuilder sb = new StringBuilder(m_Property.Name);
            if (m_Property.Parameters.Count > 0)
            {
                List<string> parameters = new List<string>();

                // Serialize parameters
                foreach (CalendarParameter p in m_Property.Parameters)
                {
                    ParameterSerializer paramSerializer = new ParameterSerializer(p);
                    parameters.Add(paramSerializer.SerializeToString());
                }

                sb.Append(";");
                sb.Append(string.Join(";", parameters.ToArray()));
            }
            sb.Append(":");
            sb.Append(m_Property.Value);            

            // FIXME: serialize the line
            //ContentLineSerializer serializer = new ContentLineSerializer(sb.ToString());
            //return serializer.SerializeToString();
            return null;
        }

        public void Serialize(Stream stream, Encoding encoding)
        {
            byte[] prop = encoding.GetBytes(SerializeToString());
            stream.Write(prop, 0, prop.Length);
        }

        public object Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IParameterSerializable Members

        public List<ICalendarParameter> Parameters
        {
            get
            {
                List<ICalendarParameter> Parameters = new List<ICalendarParameter>();
                foreach (ICalendarParameter p in m_Property.Parameters)
                {
                    if (!DisallowedParameters.Contains(p))
                        Parameters.Add(p);
                }
                return Parameters;
            }
        }

        public List<ICalendarParameter> DisallowedParameters
        {
            get { return new List<ICalendarParameter>(); }
        }

        #endregion        
    }
}
