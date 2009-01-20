using System;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using System.IO;
using System.Xml;
using System.Collections;

namespace DDay.iCal.Serialization.xCal.Components
{
    public class iCalObjectSerializer : IXCalSerializable
    {
        #region Private Fields

        private DDay.iCal.Components.iCalObject m_Object;

        #endregion

        #region Public Properties

        public DDay.iCal.Components.iCalObject Object
        {
            get { return m_Object; }
            set { m_Object = value; }
        }

        #endregion

        #region Constructors

        public iCalObjectSerializer() { }
        public iCalObjectSerializer(iCalObject iCalObject)
        {
            Object = iCalObject;
        }

        #endregion

        #region ISerializable Members

        virtual public string SerializeToString()
        {
            return string.Empty;
        }

        virtual public void Serialize(Stream stream, Encoding encoding)
        {
            XmlTextWriter xtw = new XmlTextWriter(stream, encoding);
            xtw.Formatting = Formatting.Indented;
            xtw.IndentChar = ' ';            
            Serialize(xtw);
            xtw.Close();
        }

        virtual public iCalObject Deserialize(Stream stream, Encoding encoding, Type iCalendarType)
        {
            return null;
        }

        #endregion

        #region IXCalSerializable Members

        virtual public void Serialize(XmlTextWriter xtw)
        {
            if (Object.Name != null)
            {
                // Serialize "VERSION" before any other properties
                if (Object.Properties.ContainsKey("VERSION"))
                {
                    Property p = (Property)Object.Properties["VERSION"];
                    IXCalSerializable serializer = SerializerFactory.Create(p);
                    if (serializer != null)
                        serializer.Serialize(xtw);
                }

                foreach (Property p in Object.Properties)
                {
                    // Don't serialize "VERSION" again, we've already done it above.
                    if (p.Key.Equals("VERSION"))
                        continue;
                                        
                    IXCalSerializable serializer = SerializerFactory.Create(p);
                    if (serializer != null)
                        serializer.Serialize(xtw);
                }

                foreach (Parameter p in Object.Parameters)
                {                    
                    IXCalSerializable serializer = SerializerFactory.Create(p);
                    if (serializer != null)
                        serializer.Serialize(xtw);
                }
            }
            
            foreach (DDay.iCal.Components.iCalObject obj in Object.Children)
            {
                IXCalSerializable serializer = SerializerFactory.Create(obj);
                if (serializer != null)
                    serializer.Serialize(xtw);
            }            
        }

        virtual public iCalObject Deserialize(XmlTextReader xtr, Type iCalendarType)
        {            
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
