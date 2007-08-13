using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using DDay.iCal.Components;

namespace DDay.iCal.Serialization.iCalendar.Components
{
    public class iCalObjectSerializer : ISerializable
    {
        #region Static Public Methods

        static public TextReader NormalizeLineEndings(string s)
        {
            // Replace \r and \n with \r\n.
            return new StringReader(Regex.Replace(s, @"((\r(?=[^\n]))|((?<=[^\r])\n))", "\r\n"));            
        }

        static public TextReader NormalizeLineEndings(TextReader tr)
        {
            string s = tr.ReadToEnd();
            TextReader reader = NormalizeLineEndings(s);
            tr.Close();

            return reader;
        }

        #endregion

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
        public iCalObjectSerializer(DDay.iCal.Components.iCalObject iCalObject)
        {
            Object = iCalObject;
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
            if (Object.Properties.ContainsKey("VERSION"))
            {
                Property p = (Property)Object.Properties["VERSION"];
                ISerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DictionaryEntry de in Object.Properties)
            {
                // Don't serialize "VERSION" again, we've already done it above.
                if (de.Key.Equals("VERSION"))
                    continue; 

                Property p = (Property)de.Value;
                ISerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DictionaryEntry de in Object.Parameters)
            {
                Parameter p = (Parameter)de.Value;
                ISerializable serializer = SerializerFactory.Create(p);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DDay.iCal.Components.iCalObject obj in Object.Children)
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
