using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization.iCalendar.Components;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class DataTypeSerializer : ISerializable, IParameterSerializable
    {
        #region Private Fields

        private DDay.iCal.DataTypes.iCalDataType m_dataType;

        #endregion

        #region Protected Properties

        protected DDay.iCal.DataTypes.iCalDataType DataType
        {
            get { return m_dataType; }
            set { m_dataType = value; }
        }

        #endregion

        #region Constructors

        public DataTypeSerializer(DDay.iCal.DataTypes.iCalDataType dataType)
        {
            this.m_dataType = dataType;
        }

        #endregion

        #region ISerializable Members

        virtual public void Serialize(Stream stream, Encoding encoding)
        {
            Type type = m_dataType.GetType();
            ISerializable serializer = null;

            Type serializerType = Type.GetType(GetType().Namespace + "." + type.Name + "Serializer", false, true);
            if (serializerType != null)
                serializer = (ISerializable)Activator.CreateInstance(serializerType, new object[] { m_dataType });

            if (serializer == null)
                serializer = new FieldSerializer(m_dataType);

            if (serializer != null)
            {
                string value = m_dataType.Name;
                                
                if (serializer is IParameterSerializable)
                {
                    IParameterSerializable paramSerializer = (IParameterSerializable)serializer;
                    List<string> Parameters = paramSerializer.Parameters;
                    if (Parameters.Count > 0)
                        value += ";" + string.Join(";", Parameters.ToArray());
                }

                value += ":";
                value += serializer.SerializeToString();
                value += "\r\n";

                ISerializable clSerializer = new ContentLineSerializer(value);
                if (clSerializer != null)
                    clSerializer.Serialize(stream, encoding);
            }
        }

        virtual public string SerializeToString()
        {
            Type type = m_dataType.GetType();
            ISerializable serializer = null;

            Type serializerType = Type.GetType(GetType().Namespace + "." + type.Name + "Serializer", false, true);
            if (serializerType != null)
                serializer = (ISerializable)Activator.CreateInstance(serializerType, new object[] { m_dataType });

            if (serializer == null)
                serializer = new FieldSerializer(m_dataType);

            if (serializer != null)
                return serializer.SerializeToString();
            else return string.Empty;
        }

        #endregion

        #region IParameterSerializable Members

        virtual public List<string> Parameters
        {
            get
            {
                List<string> Parameters = new List<string>();
                foreach (DictionaryEntry de in m_dataType.Parameters)
                {
                    if (!this.DisallowedParameters.Contains(de.Key.ToString()))
                    {
                        Parameters.Add(de.Key + "=" + string.Join(",", ((Parameter)de.Value).Values.ToArray()));
                    }
                }
                return new List<string>();
            }
        }

        virtual public List<string> DisallowedParameters
        {
            get
            {
                return new List<string>();
            }
        }

        public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion        
    }
}
