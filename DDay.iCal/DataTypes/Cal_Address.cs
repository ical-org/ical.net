using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DDay.iCal.Components;
using System.Runtime.Serialization;

namespace DDay.iCal.DataTypes
{
    /// <summary>
    /// A class that represents the address of an iCalendar user.
    /// In iCalendar terms, this is usually represented by an
    /// e-mail address, in the following form:
    /// <c>MAILTO:email.address@host.com</c>
    /// </summary>
    [DebuggerDisplay("{Value}")]
#if DATACONTRACT
    [DataContract(Name = "Cal_Address", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
    [KnownType(typeof(Cal_Address))]
    [KnownType(typeof(Text))]
    [KnownType(typeof(URI))]
#else
    [Serializable]
#endif
    public class Cal_Address : URI
    {
        public const string ORGANIZER = "ORGANIZER";
        public const string ATTENDEE = "ATTENDEE";

        #region Private Fields

        private Cal_Address m_SentBy;
        private Text m_CN;
        private URI m_DirectoryEntry;

        #endregion

        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public Cal_Address SentBy
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
            protected set
            {
                if (value != null)
                    Parameters["SENT-BY"] = new Parameter("SENT-BY", value);
                else
                    Parameters.Remove("SENT-BY");
            }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public Text CommonName
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
            protected set
            {
                if (value != null)
                    Parameters["CN"] = new Parameter("CN", value);
                else
                    Parameters.Remove("CN");
            }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public URI DirectoryEntry
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
            protected set
            {
                if (value != null)
                    Parameters["DIR"] = new Parameter("DIR", value.ToString());
                else
                    Parameters.Remove("DIR");
            }
        }

        virtual public string EmailAddress
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
        public Cal_Address(string value) : this(string.Empty, value) { }
        protected Cal_Address(string name, string value) : this()
        {
            this.Name = name;
            object obj = this;
            if (!base.TryParse(value, ref obj) ||
                string.IsNullOrEmpty(this.Scheme))
                CopyFrom(Parse("MAILTO:" + value));
        }

        #endregion

        #region Operators

        static public implicit operator string(Cal_Address addr)
        {
            return addr != null ? addr.Value : null;            
        }

        static public implicit operator Cal_Address(string s)
        {
            return new Cal_Address(s);
        }

        #endregion
    }
}
