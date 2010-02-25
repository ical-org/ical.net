using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents the organizer of an event/todo/journal.
    /// </summary>
    [DebuggerDisplay("{Value}")]
#if DATACONTRACT
    [DataContract(Name = "Organizer", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Organizer : CalendarAddress
    {
        #region Public Properties

#if DATACONTRACT
        [DataMember(Order = 1)]
#endif
        virtual public CalendarAddress SentBy
        {
            get { return Parameters.Get<CalendarAddress>("SENT-BY"); }
            set { Parameters.Set("SENT-BY", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 2)]
#endif
        virtual public Text CommonName
        {
            get { return Parameters.Get<Text>("CN"); }
            set { Parameters.Set("CN", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 3)]
#endif
        virtual public URI DirectoryEntry
        {
            get { return Parameters.Get<URI>("DIR"); }
            set { Parameters.Set("DIR", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 4)]
#endif
        virtual public string Language
        {
            get { return Parameters.Get<string>("LANGUAGE"); }
            set { Parameters.Set("LANGUAGE", value); }
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

        public Organizer() : base() { }
        public Organizer(string value)
            : this()
        {
            this.Name = "ORGANIZER";
        }

        #endregion

        #region Operators

        static public implicit operator string(Organizer o)
        {
            return o != null ? o.Value : null;            
        }

        static public implicit operator Organizer(string o)
        {
            return new Organizer(o);
        }

        #endregion
    }
}
