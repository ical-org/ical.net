using System;
using System.Collections.Generic;

namespace DDay.iCal
{
    public class Attendee :
        EncodableDataType,
        IAttendee
	{
        #region IAttendee Members
        
        virtual public Uri SentBy
        {
            get { return new Uri(Parameters.Get("SENT-BY")); }
            set
            {
                if (value != null)
                    Parameters.Set("SENT-BY", value.OriginalString);
                else
                    Parameters.Set("SENT-BY", (string)null);
            }
        }

        virtual public string CommonName
        {
            get { return Parameters.Get("CN"); }
            set { Parameters.Set("CN", value); }
        }

        virtual public Uri DirectoryEntry
        {
            get { return new Uri(Parameters.Get("DIR")); }
            set
            {
                if (value != null)
                    Parameters.Set("DIR", value.OriginalString);
                else
                    Parameters.Set("DIR", (string)null);
            }
        }
        
        virtual public string Type
        {
            get { return Parameters.Get("CUTYPE"); }
            set { Parameters.Set("CUTYPE", value); }
        }
        
        virtual public IList<string> Members
        {
            get { return Parameters.GetList("MEMBER"); }
            set { Parameters.Set("MEMBER", value); }
        }
        
        virtual public string Role
        {
            get { return Parameters.Get("ROLE"); }
            set { Parameters.Set("ROLE", value); }
        }
        
        virtual public string ParticipationStatus
        {
            get { return Parameters.Get("PARTSTAT"); }
            set { Parameters.Set("PARTSTAT", value); }
        }
        
        virtual public bool RSVP
        {
            get
            {
                bool val;
                string rsvp = Parameters.Get("RSVP");
                if (rsvp != null && bool.TryParse(rsvp, out val))
                    return val;
                return false;
            }
            set
            {
                string val = value.ToString();
                if (val != null)
                    val = val.ToUpper();
                Parameters.Set("RSVP", val);
            }
        }
        
        virtual public IList<string> DelegatedTo
        {
            get { return Parameters.GetList("DELEGATED-TO"); }
            set { Parameters.Set("DELEGATED-TO", value); }
        }
         
        virtual public IList<string> DelegatedFrom
        {
            get { return Parameters.GetList("DELEGATED-FROM"); }
            set { Parameters.Set("DELEGATED-FROM", value); }
        }
        
        virtual public Uri Value { get; set; }
        
        #endregion        
            
        #region Constructors
		
        public Attendee()
        {
        }

        public Attendee(Uri attendee)
        {
            Value = attendee;
        }

        public Attendee(string attendeeUri)
        {
            if (!Uri.IsWellFormedUriString(attendeeUri, UriKind.Absolute))
                throw new ArgumentException("attendeeUri");
            Value = new Uri(attendeeUri);
        }
		
        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            IAttendee a = obj as IAttendee;
            if (a != null)
                return object.Equals(Value, a.Value);
            return base.Equals(obj);
        }

        #endregion
    }
}
