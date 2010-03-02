using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DDay.iCal
{    
    /// <summary>
    /// An abstract class that represents an iCalendar data type which
    /// accepts the ENCODING parameter.
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "EncodableDataType", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public abstract class EncodableDataType : 
        CalendarDataType
    {
        #region Private Fields

        private string m_Value;
        private byte[] m_Data;

        #endregion

        #region Public Properties

        virtual public string Encoding
        {
            get { return Parameters.Get("ENCODING"); }
            set { Parameters.Set("ENCODING", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public byte[] Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        #endregion        

        #region Private Methods

        private bool TryParse7BIT(string value, ref ICalendarDataType obj)
        {
            EncodableDataType edt = (EncodableDataType)obj;

            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] data = utf8.GetBytes(value);
                for (int i = 0; i < data.Length; i++)
                {
                    byte b = data[i];
                    if (b == 0 || b > 127)
                        return false;
                    // Ensure CR is always part of CRLF
                    else if (b == 13 &&
                        i < data.Length - 1 &&
                        data[i + 1] != 10)
                        return false;
                    // Ensure LF is always part of CRLF
                    else if (b == 10 &&
                        i > 0 &&
                        data[i - 1] != 13)
                        return false;
                }

                edt.Data = data;
                edt.Value = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParse8BIT(string value, ref ICalendarDataType obj)
        {
            EncodableDataType edt = (EncodableDataType)obj;

            try
            {
                UTF8Encoding utf8 = new UTF8Encoding();
                byte[] data = utf8.GetBytes(value);
                for (int i = 0; i < data.Length; i++)
                {
                    byte b = data[i];
                    if (b == 0)
                        return false;
                    // Ensure CR is always part of CRLF
                    else if (b == 13 &&
                        i < data.Length - 1 &&
                        data[i + 1] != 10)
                        return false;
                    // Ensure LF is always part of CRLF
                    else if (b == 10 &&
                        i > 0 &&
                        data[i - 1] != 13)
                        return false;
                }

                edt.Data = data;
                edt.Value = value;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TryParseBASE64(string value, ref ICalendarDataType obj)
        {
            EncodableDataType edt = (EncodableDataType)obj;
            try
            {
                UTF8Encoding encoder = new UTF8Encoding();
                Decoder utf8Decode = encoder.GetDecoder();

                edt.Data = Convert.FromBase64String(value);
                int charCount = utf8Decode.GetCharCount(edt.Data, 0, edt.Data.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(edt.Data, 0, edt.Data.Length, decoded_char, 0);
                edt.Value = new String(decoded_char);
                return true;
            }
            catch
            {
                return false;
            }            
        }

        #endregion

        #region Public Methods

        public void SetEncoding(EncodingType encoding)
        {
            Encoding = GetEncodingName(encoding);   
        }

        public string GetEncodingName(EncodingType encoding)
        {
            switch (encoding)
            {                
                case EncodingType.Base64: return "BASE64";
                case EncodingType.Bit7: return "7BIT";
                case EncodingType.Bit8: return "8BIT";
                case EncodingType.None:
                default:
                    return null;
            }
        }

        #endregion
    }

    public enum EncodingType
    {
        None,
        Base64,
        Bit7,
        Bit8
    }
}
