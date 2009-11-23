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

        /// <summary>
        /// Unwraps lines from the RFC 2445 "line folding" technique.
        /// NOTE: this method makes the line/col numbers output from
        /// antlr incorrect.
        /// </summary>
        static public string UnwrapLines(string s)
        {
            return Regex.Replace(s, @"(\r\n[ \t])", string.Empty);
        }

        /// <summary>
        /// Removes blank lines from a string with normalized (\r\n)
        /// line endings.
        /// NOTE: this method makes the line/col numbers output from
        /// antlr incorrect.
        /// </summary>
        static public string RemoveEmptyLines(string s)
        {
            int len = -1;
            while (len != s.Length)
            {
                s = s.Replace("\r\n\r\n", "\r\n");
                len = s.Length;
            }
            return s;
        }

        /// <summary>
        /// Normalizes line endings, converting "\r" into "\r\n" and "\n" into "\r\n".        
        /// </summary>
        static public TextReader NormalizeLineEndings(string s, bool maintainLineAccuracy)
        {
            // Replace \r and \n with \r\n.
            s = Regex.Replace(s, @"((\r(?=[^\n]))|((?<=[^\r])\n))", "\r\n");
            if (!maintainLineAccuracy)
                s = RemoveEmptyLines(UnwrapLines(s));
            return new StringReader(s);
        }

        static public TextReader NormalizeLineEndings(TextReader tr, bool maintainLineAccuracy)
        {
            string s = tr.ReadToEnd();
            TextReader reader = NormalizeLineEndings(s, maintainLineAccuracy);
            tr.Close();

            return reader;
        }

        #endregion

        #region Private Fields

        private DDay.iCal.Components.iCalObject m_Object;
        private ISerializationContext m_SerializationContext;        

        #endregion

        #region Public Properties

        public DDay.iCal.Components.iCalObject Object
        {
            get { return m_Object; }
            set { m_Object = value; }
        }

        #endregion

        #region Constructors

        public iCalObjectSerializer() 
        {
            m_SerializationContext = DDay.iCal.Serialization.SerializationContext.Default;
        }
        public iCalObjectSerializer(DDay.iCal.Components.iCalObject iCalObject) : this()
        {
            Object = iCalObject;
        }

        #endregion

        #region ISerializable Members

        virtual public ISerializationContext SerializationContext
        {
            get { return m_SerializationContext; }
            set { m_SerializationContext = value; }
        }

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
                ISerializable serializer = SerializerFactory.Create(p, SerializationContext);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (Property p in Object.Properties)
            {
                // Don't serialize "VERSION" again, we've already done it above.
                if (p.Key.Equals("VERSION"))
                    continue;

                ISerializable serializer = SerializerFactory.Create(p, SerializationContext);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (Parameter p in Object.Parameters)
            {
                ISerializable serializer = SerializerFactory.Create(p, SerializationContext);
                if (serializer != null)
                    serializer.Serialize(stream, encoding);
            }

            foreach (DDay.iCal.Components.iCalObject obj in Object.Children)
            {
                ISerializable serializer = SerializerFactory.Create(obj, SerializationContext);
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
