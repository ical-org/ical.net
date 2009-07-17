using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// An iCalendar URI (Universal Resource Identifier) value.
    /// </summary>
    [DebuggerDisplay("{Value}")]
#if SILVERLIGHT
    [DataContract(Name = "URI", Namespace="http://www.ddaysoftware.com/dday.ical/datatypes/2009/07/")]
#else
    [Serializable]
#endif
    public class URI : iCalDataType
    {
        #region Private Fields

        private string m_Value;
        private string m_Scheme;
        private string m_Authority;
        private string m_Path;
        private string m_Query;
        private string m_Fragment;

        #endregion

        #region Public Properties

        public string Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public string Scheme
        {
            get { return m_Scheme; }
            set { m_Scheme = value; }
        }

        public string Authority
        {
            get { return m_Authority; }
            set { m_Authority = value; }
        }

        public string Path
        {
            get { return m_Path; }
            set { m_Path = value; }
        }

        public string Query
        {
            get { return m_Query; }
            set { m_Query = value; }
        }

        public string Fragment
        {
            get { return m_Fragment; }
            set { m_Fragment = value; }
        }

        #endregion

        #region Constructors

        public URI() { }
        public URI(string value)
            : this()
        {
            CopyFrom(Parse(value));
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is URI)
                return Value.Equals(((URI)obj).Value);
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override bool TryParse(string value, ref object obj)
        {
            URI uri = (URI)obj;            

            try
            {
                string regexPattern = @"^((?<scheme>[^:/\?#]+):)?" +
                    @"(//(?<authority>[^/\?#]*))?" +
                    @"(?<path>[^\?#]*)" +
                    @"(\?(?<query>[^#]*))?" +
                    @"(#(?<fragment>.*))?";

                Regex re = new Regex(regexPattern, RegexOptions.ExplicitCapture);
                Match m = re.Match(value);

                uri.Value = m.Value;
                uri.Scheme = m.Groups["scheme"].Value.ToLower();
                uri.Authority = m.Groups["authority"].Value;
                uri.Path = m.Groups["path"].Value;
                uri.Query = m.Groups["query"].Value;
                uri.Fragment = m.Groups["fragment"].Value;

                return true;
            }
            catch
            {
                return false;
            }            
        }

        public override void CopyFrom(object obj)
        {
            base.CopyFrom(obj);
            if (obj is URI)
            {
                URI uri = (URI)obj;
                Value = uri.Value;
                Scheme = uri.Scheme;
                Authority = uri.Authority;
                Path = uri.Path;
                Query = uri.Query;
                Fragment = uri.Fragment;
            }
            base.CopyFrom(obj);
        }

        public override string ToString()
        {
            return Value;
        }

        #endregion

        #region Operators

        /// <summary>
        /// FIXME: create a TypeConverter from string to URI so strings will automatically
        /// be converted to URI objects when using late-binding means of setting the value.
        /// i.e. reflection - PropertyInfo.SetValue(...).
        /// </summary>
        static public implicit operator URI(string txt)
        {
            return new URI(txt);
        }

        #endregion
    }
}
