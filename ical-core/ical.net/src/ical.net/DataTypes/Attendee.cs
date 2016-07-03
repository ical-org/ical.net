using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net.DataTypes
{
    public class Attendee : EncodableDataType, IAttendee
    {
        public virtual Uri SentBy
        {
            get { return new Uri(Parameters.Get("SENT-BY")); }
            set
            {
                if (value != null)
                {
                    Parameters.Set("SENT-BY", value.OriginalString);
                }
                else
                {
                    Parameters.Set("SENT-BY", (string) null);
                }
            }
        }

        public virtual string CommonName
        {
            get { return Parameters.Get("CN"); }
            set { Parameters.Set("CN", value); }
        }

        public virtual Uri DirectoryEntry
        {
            get { return new Uri(Parameters.Get("DIR")); }
            set
            {
                if (value != null)
                {
                    Parameters.Set("DIR", value.OriginalString);
                }
                else
                {
                    Parameters.Set("DIR", (string) null);
                }
            }
        }

        public virtual string Type
        {
            get { return Parameters.Get("CUTYPE"); }
            set { Parameters.Set("CUTYPE", value); }
        }

        public virtual IList<string> Members
        {
            get { return Parameters.GetMany("MEMBER"); }
            set { Parameters.Set("MEMBER", value); }
        }

        public virtual string Role
        {
            get { return Parameters.Get("ROLE"); }
            set { Parameters.Set("ROLE", value); }
        }

        public virtual string ParticipationStatus
        {
            get { return Parameters.Get("PARTSTAT"); }
            set { Parameters.Set("PARTSTAT", value); }
        }

        public virtual bool Rsvp
        {
            get
            {
                bool val;
                var rsvp = Parameters.Get("RSVP");
                if (rsvp != null && bool.TryParse(rsvp, out val))
                {
                    return val;
                }
                return false;
            }
            set
            {
                var val = value.ToString();
                val = val.ToUpper();
                Parameters.Set("RSVP", val);
            }
        }

        public virtual IList<string> DelegatedTo
        {
            get { return Parameters.GetMany("DELEGATED-TO"); }
            set { Parameters.Set("DELEGATED-TO", value); }
        }

        public virtual IList<string> DelegatedFrom
        {
            get { return Parameters.GetMany("DELEGATED-FROM"); }
            set { Parameters.Set("DELEGATED-FROM", value); }
        }

        [DataMember(Order = 1)]
        public virtual Uri Value { get; set; }

        public Attendee() {}

        public Attendee(Uri attendee)
        {
            Value = attendee;
        }

        public Attendee(string attendeeUri)
        {
            if (!Uri.IsWellFormedUriString(attendeeUri, UriKind.Absolute))
            {
                throw new ArgumentException("attendeeUri");
            }
            Value = new Uri(attendeeUri);
        }

        protected bool Equals(Attendee other)
        {
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Attendee) obj);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }

        public override void CopyFrom(ICopyable obj)
        {
            base.CopyFrom(obj);

            var a = obj as IAttendee;
            if (a != null)
            {
                Value = a.Value;
            }
        }
    }
}