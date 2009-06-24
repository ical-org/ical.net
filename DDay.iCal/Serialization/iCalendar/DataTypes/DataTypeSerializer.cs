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
            {
                if (m_dataType is EncodableDataType)
                    serializer = new EncodableDataTypeSerializer(m_dataType as EncodableDataType);
                else serializer = new FieldSerializer(m_dataType);
            }

            if (serializer != null)
            {
                string value = m_dataType.Name;
                                
                if (serializer is IParameterSerializable)
                {
                    IParameterSerializable paramSerializer = (IParameterSerializable)serializer;
                    List<Parameter> Parameters = paramSerializer.Parameters;
                    if (Parameters.Count > 0)
                    {                        
                        List<string> values = new List<string>();
                        foreach (Parameter p in Parameters)
                        {
                            ParameterSerializer pSerializer = new ParameterSerializer(p);
                            values.Add(pSerializer.SerializeToString());
                        }

                        value += ";" + string.Join(";", values.ToArray());
                    }
                }

                value += ":";
                value += serializer.SerializeToString();                

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

        virtual public List<Parameter> Parameters
        {
            get
            {
                List<Parameter> Parameters = new List<Parameter>();
                foreach (Parameter p in m_dataType.Parameters)
                {
                    if (!this.DisallowedParameters.Contains(p))
                        Parameters.Add(p);
                }
                return Parameters;
            }
        }

        virtual public List<Parameter> DisallowedParameters
        {
            get
            {
                return new List<Parameter>();
            }
        }

        public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion        
    }
}
