using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization.xCal.Components;

namespace DDay.iCal.Serialization.xCal.DataTypes
{
    public class DataTypeSerializer : IXCalSerializable, IParameterSerializable
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

        public void Serialize(Stream stream, Encoding encoding)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
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
    
        #region IXCalSerializable Members

        virtual public void Serialize(XmlTextWriter xtw)
        {
            Type type = m_dataType.GetType();
            IXCalSerializable serializer = null;

            Type serializerType = Type.GetType(GetType().Namespace + "." + type.Name + "Serializer", false, true);
            if (serializerType != null)
                serializer = (IXCalSerializable)Activator.CreateInstance(serializerType, new object[] { m_dataType });

            if (serializer == null)
                serializer = new FieldSerializer(m_dataType);

            if (serializer != null)
            {
                xtw.WriteStartElement(m_dataType.Name.ToLower());                

                if (serializer is IParameterSerializable)
                {
                    IParameterSerializable paramSerializer = (IParameterSerializable)serializer;
                    List<Parameter> Parameters = paramSerializer.Parameters;
                    foreach (Parameter param in paramSerializer.Parameters)
                    {                        
                        xtw.WriteAttributeString(param.Name.ToLower(), string.Join(",", param.Values.ToArray()));
                    }
                }

                // Determine if we should serialize the data of this serializer
                // as CDATA instead of a standard string.
                if (serializer.GetType().GetCustomAttributes(typeof(CDataAttribute), true).Length > 0)
                    xtw.WriteCData(serializer.SerializeToString());
                else xtw.WriteString(serializer.SerializeToString());
                
                xtw.WriteEndElement();                
            }
        }

        virtual public iCalObject Deserialize(System.Xml.XmlTextReader xtr, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
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

        #endregion        
    }
}
