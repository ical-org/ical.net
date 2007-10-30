using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// A class that represents the address of an iCalendar user.
    /// In iCalendar terms, this is usually represented by an
    /// e-mail address, in the following form:
    /// <c>MAILTO:email.address@host.com</c>
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class Cal_Address : URI
    {
        #region Private Fields

        private Cal_Address m_SentBy;
        private Text m_CN;
        private URI m_DirectoryEntry;

        #endregion

        #region Public Properties

        public Cal_Address SentBy
        {
            get
            {
                if (m_SentBy == null && Parameters.ContainsKey("SENT-BY"))
                {
                    Parameter p = (Parameter)Parameters["SENT-BY"];
                    m_SentBy = new Cal_Address(p.Values[0].ToString());                    
                }
                return m_SentBy;
            }
        }

        public Text CommonName
        {
            get
            {
                if (m_CN == null && Parameters.ContainsKey("CN"))
                {
                    Parameter p = (Parameter)Parameters["CN"];
                    m_CN = new Text(p.Values[0].ToString());                    
                }
                return m_CN;
            }
        }

        public URI DirectoryEntry
        {
            get
            {
                if (m_DirectoryEntry == null && Parameters.ContainsKey("DIR"))
                {
                    Parameter p = (Parameter)Parameters["DIR"];
                    m_DirectoryEntry = new URI(p.Values[0].ToString());                    
                }
                return m_DirectoryEntry;
            }
        }

        public string EmailAddress
        {
            get
            {
                if (Value != null &&
                    Scheme == Uri.UriSchemeMailto)
                {
                    return Authority;
                }
                return null;
            }
        }

        #endregion

        #region Constructors

        public Cal_Address() : base() { }
        public Cal_Address(string value) : this("ATTENDEE", value) { }
        public Cal_Address(string name, string value) : this()
        {
            this.Name = name;
            object obj = this;
            if (!base.TryParse(value, ref obj))
                CopyFrom(Parse("MAILTO:" + value));
        }

        #endregion

        #region Operators

        static public implicit operator string(Cal_Address addr)
        {
            return addr.Value;
        }

        static public implicit operator Cal_Address(string s)
        {
            return new Cal_Address(s);
        }

        #endregion
    }
}
