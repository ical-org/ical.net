using System;
using System.Diagnostics;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// Represents an RFC 2445 "text" value.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    [Encodable("BASE64,8BIT,7BIT")]
    public class Text : EncodableDataType
    {
        #region Constructors

        public Text() { }
        public Text(string value) : this()
        {
            if (value != null)
                CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override void CopyFrom(object obj)
        {
            if (obj is Text)
            {
                Text t = (Text)obj;
                this.Value = t.Value;          
            }
            base.CopyFrom(obj);
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

        public override bool TryParse(string value, ref object obj)
        {
            if (!base.TryParse(value, ref obj))
                return false;
            Text t = (Text)obj;
            if (t.Value != null)
                value = t.Value;

            // NOTE: fixed a bug that caused text parsing to fail on
            // programmatically entered strings.
            // SEE unit test SERIALIZE25().
            value = value.Replace("\r\n", @"\n");
            value = value.Replace("\r", @"\n");
            value = value.Replace("\n", @"\n");
            value = value.Replace(@"\n", "\n");
            value = value.Replace(@"\N", "\n");            
            value = value.Replace(@"\;", ";");
            value = value.Replace(@"\,", ",");
            // FIXME: double quotes aren't escaped in RFC2445, but are in Mozilla
            value = value.Replace("\\\"", "\"");

            // Replace all single-backslashes with double-backslashes.
            value = Regex.Replace(value, @"[^\\]\\[^\\]", @"\\");
                        
            // Everything but backslashes has been unescaped. Validate this...
            if (Regex.IsMatch(value, @"[^\\]\\[^\\]"))
                return false;

            // Unescape double backslashes
            value = value.Replace(@"\\", @"\");
            t.Value = value;

            return true;
        }

        public override string ToString()
        {
            return Value;
        }

        #endregion

        #region Operators

        public static implicit operator string(Text t)
        {
            return t.Value;
        }

        public static implicit operator Text(string s)
        {
            return new Text(s);
        }

        #endregion
    }
}
