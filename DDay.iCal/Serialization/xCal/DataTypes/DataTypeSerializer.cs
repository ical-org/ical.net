using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization.xCal.Components;
using System.Xml;

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
                    List<string> Parameters = paramSerializer.Parameters;
                    foreach (string param in paramSerializer.Parameters)
                    {
                        Parameter p = m_dataType.Parameters[param] as Parameter;
                        if (p != null)
                            xtw.WriteAttributeString(p.Name, string.Join(",", p.Values.ToArray()));
                    }
                }
                                
                xtw.WriteString(serializer.SerializeToString());
                xtw.WriteEndElement();                
            }
        }

        virtual public iCalObject Deserialize(System.Xml.XmlTextReader xtr, Type iCalendarType)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
