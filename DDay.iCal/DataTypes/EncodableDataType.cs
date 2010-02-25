using System;
using System.Collections;
using System.Text;
using System.Reflection;
using DDay.iCal;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DDay.iCal.Serialization;

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
    public abstract class EncodableDataType : iCalDataType
    {
        #region Private Fields

        private string m_Value;
        private byte[] m_Data;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public string Encoding
        {
            get
            {
                if (Parameters.ContainsKey("ENCODING"))
                {
                    CalendarParameter p = (CalendarParameter)Parameters["ENCODING"];
                    if (p.Values.Count > 0)                        
                        return p.Values[0].ToString();
                }
                return null;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    // Ensure the data type can be encoded in this manner
                    bool encodable = false;
                    foreach (EncodableAttribute ea in GetType().GetCustomAttributes(typeof(EncodableAttribute), true))
                    {
                        foreach (string Name in ea.Values)
                            if (Name.ToUpper().Equals(value.ToUpper()))
                                encodable = true;
                    }

                    if (!encodable)
                        throw new NotSupportedException("The " + GetType().Name + " data type does not support " + value.ToUpper() + " encoding.");

                    // Set the encoding via the "ENCODING" parameter
                    CalendarParameter p = null;
                    if (Parameters.ContainsKey("ENCODING"))
                    {
                        p = (CalendarParameter)Parameters["ENCODING"];
                        p.Values = new List<string>();
                        p.Values.Add(value.ToUpper());
                    }
                    else
                    {
                        p = new CalendarParameter("ENCODING", value.ToUpper());
                        AddParameter(p);
                    }
                }
                else
                {
                    // Remove the "ENCODING" parameter
                    if (Parameters.ContainsKey("ENCODING"))
                        Parameters.Remove("ENCODING");
                }
            }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        public byte[] Data
        {
            get { return m_Data; }
            set { m_Data = value; }
        }

        #endregion

        #region Overrides

        public override bool TryParse(string value, ref ICalendarObject obj)
        {
            if (Encoding != null)
            {
                switch (Encoding)
                {
                    case "7BIT": return TryParse7BIT(value, ref obj);
                    case "8BIT": return TryParse8BIT(value, ref obj);
                    case "BASE64": return TryParseBASE64(value, ref obj);
                    default: break;
                }
            }
            return true;
        }
        
        #endregion

        #region Private Methods

        private bool TryParse7BIT(string value, ref ICalendarObject obj)
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

        private bool TryParse8BIT(string value, ref ICalendarObject obj)
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

        private bool TryParseBASE64(string value, ref ICalendarObject obj)
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

        #endregion

        #region Private Methods

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
