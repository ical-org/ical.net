using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DDay.iCal.DataTypes;
using DDay.iCal.Objects;

namespace DDay.iCal.Serialization.iCalendar.DataTypes
{
    public class ContentLineSerializer : ISerializable
    {
        #region Private Fields

        private string m_text;

        #endregion

        #region Constructors

        public ContentLineSerializer(string s)            
        {
            this.m_text = s;
        }

        #endregion

        #region ISerializable Members

        public string SerializeToString()
        {
            List<string> values = new List<string>();
            string value = m_text;
            // Wrap lines at 75 characters, per RFC 2445 "folding" technique
            while (value.Length > 75 &&
                value[74] != '\r' &&
                value[74] != '\n')
            {
                values.Add(value.Substring(0, 74));
                value = "\r\n " + value.Substring(74);
            }
            values.Add(value);

            return string.Join(string.Empty, values.ToArray());
        }

        public void Serialize(Stream stream, Encoding encoding)
        {
            byte[] data = encoding.GetBytes(SerializeToString());
            if (data.Length > 0)
                stream.Write(data, 0, data.Length);
        }

        #endregion
    }
}
