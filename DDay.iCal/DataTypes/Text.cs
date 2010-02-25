using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// Represents an RFC 2445 "text" value.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    [Encodable("BASE64,8BIT,7BIT")]
#if DATACONTRACT
    [DataContract(Name = "Text", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Text : 
        EncodableDataType,
        IEscapable
    {
        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public string Language
        {
            get { return Parameters.Get<string>("LANGUAGE"); }
            set { Parameters.Set("LANGUAGE", value); }
        }

        #endregion

        #region Constructors

        public Text() { }
        public Text(string value, bool unescape) : this()
        {
            if (value != null)
            {
                CopyFrom(Parse(value));
                if (unescape)
                    Unescape();                    
            }
        }

        #endregion

        #region Overrides

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);
            if (obj is Text)
            {
                Text t = (Text)obj;
                this.Value = t.Value;          
            }
            base.CopyFrom(obj);
        }

        public override bool TryParse(string value, ref ICalendarObject obj)
        {
            if (base.TryParse(value, ref obj))
            {
                Text t = (Text)obj;
                if (t.Value == null)
                    t.Value = value;
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Text)
            {
                Text t = (Text)obj;
                return Value.Equals(t.Value);
            }
            else if (obj is string)            
                return Value.Equals(obj);            
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value;
        }        

        #endregion

        #region Operators

        public static implicit operator string(Text t)
        {            
            return t != null ? t.Value : null;
        }

        public static implicit operator Text(string s)
        {
            return new Text(s, false);
        }

        #endregion

        #region IEscapable Methods

        public void Unescape()
        {
            string value = Value;

            // added null check - you can't call .Replace on a null
            // string, but you can just return null as a string
            if (value != null)
            {
                value = value.Replace(@"\n", "\n");
                value = value.Replace(@"\N", "\n");
                value = value.Replace(@"\;", ";");
                value = value.Replace(@"\,", ",");
                // NOTE: double quotes aren't escaped in RFC2445, but are in Mozilla Sunbird (0.5-)
                value = value.Replace("\\\"", "\"");

                // Replace all single-backslashes with double-backslashes.
                value = Regex.Replace(value, @"(?<!\\)\\(?!\\)", "\\\\");

                // Unescape double backslashes
                value = value.Replace(@"\\", @"\");
                Value = value;
            }
        }

        public void Escape()
        {
            string value = Value;

            // added null check - you can't call .Replace on a null
            // string, but you can just return null as a string
            if (value != null)
            {
                // NOTE: fixed a bug that caused text parsing to fail on
                // programmatically entered strings.
                // SEE unit test SERIALIZE25().
                value = value.Replace("\r\n", @"\n");
                value = value.Replace("\r", @"\n");
                value = value.Replace("\n", @"\n");
                value = value.Replace(";", @"\;");
                value = value.Replace(",", @"\,");
                Value = value;
            }
        }

        #endregion
    }
}
