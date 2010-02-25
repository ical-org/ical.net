using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using DDay.iCal;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// A class that represents a potential attendee 
    /// of an event/todo/journal.
    /// </summary>
    [DebuggerDisplay("{Value}")]
#if DATACONTRACT
    [DataContract(Name = "Attendee", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class Attendee : CalendarAddress
    {
        #region Public Classes

        public class UserTypes
        {
            /// <summary>
            /// An individual.
            /// </summary>
            public const string Individual = "INDIVIDUAL";

            /// <summary>
            /// A group of individuals.
            /// </summary>
            public const string Group = "GROUP";

            /// <summary>
            /// A physical resource.
            /// </summary>
            public const string Resource = "RESOURCE";

            /// <summary>
            /// A room resource.
            /// </summary>
            public const string Room = "ROOM";

            /// <summary>
            /// Otherwise not known.
            /// </summary>
            public const string Unknown = "UNKNOWN";
        }

        public class Roles
        {
            /// <summary>
            /// Indicates chair of the calendar entity.
            /// </summary>
            public const string Chair = "CHAIR";

            /// <summary>
            /// Indicates a participant whose
            /// participation is required.
            /// </summary>
            public const string RequiredParticipant = "REQ-PARTICIPANT";

            /// <summary>
            /// Indicates a participant whose 
            /// participation is optional.
            /// </summary>
            public const string OptionalParticipant = "OPT-PARTICIPANT";

            /// <summary>
            /// Indicates a participant who
            /// is copied for information.
            /// </summary>
            public const string NonParticipant = "NON-PARTICIPANT";
        }

        public class ParticipationStatuses
        {
            /// <summary>
            /// Event/Todo/Journal needs action.
            /// </summary>
            public const string NeedsAction = "NEEDS-ACTION";

            /// <summary>
            /// Event/Todo/Journal accepted.
            /// </summary>
            public const string Accepted = "ACCEPTED";

            /// <summary>
            /// Event/Todo/Journal declined.
            /// </summary>
            public const string Declined = "DECLINED";

            /// <summary>
            /// Event/Todo tentatively accepted.
            /// </summary>
            public const string Tentative = "TENTATIVE";

            /// <summary>
            /// Event/Todo delegated.
            /// </summary>
            public const string Delegated = "DELEGATED";

            /// <summary>
            /// Todo completed and  COMPLETED property has
            /// DATE-TIME completed.
            /// </summary>
            public const string Completed = "COMPLETED";

            /// <summary>
            /// Todo is in the process of being completed.
            /// </summary>
            public const string InProcess = "IN-PROCESS";
        }

        #endregion

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
        virtual public string Type
        {
            get { return Parameters.Get<string>("CUTYPE"); }
            set { Parameters.Set("CUTYPE", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 5)]
#endif
        virtual public CalendarAddressCollection Member
        {
            get { return Parameters.Get<CalendarAddressCollection>("MEMBER"); }
            set { Parameters.Set("MEMBER", value != null ? value.ToString() : null); }
        }

#if DATACONTRACT
        [DataMember(Order = 6)]
#endif
        virtual public string Role
        {
            get { return Parameters.Get<string>("ROLE"); }
            set { Parameters.Set("ROLE", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 7)]
#endif
        virtual public string ParticipationStatus
        {
            get { return Parameters.Get<string>("PARTSTAT"); }
            set { Parameters.Set("PARTSTAT", value); }
        }

#if DATACONTRACT
        [DataMember(Order = 8)]
#endif
        virtual public bool RSVP
        {
            get
            {
                string value = Parameters.Get<string>("RSVP");
                if (value != null &&
                    string.Compare(value, "TRUE", StringComparison.InvariantCultureIgnoreCase) == 0)
                    return true;
                return false;
            }
            set { Parameters.Set("RSVP", value != null ? value.ToString().ToUpper() : null); }
        }

#if DATACONTRACT
        [DataMember(Order = 9)]
#endif
        virtual public CalendarAddressCollection DelegatedTo
        {
            get { return Parameters.Get<CalendarAddressCollection>("DELEGATED-TO"); }
            set { Parameters.Set("DELEGATED-TO", value != null ? value.ToString() : null); }
        }

#if DATACONTRACT
        [DataMember(Order = 10)]
#endif
        virtual public CalendarAddressCollection DelegatedFrom
        {
            get { return Parameters.Get<CalendarAddressCollection>("DELEGATED-FROM"); }
            set { Parameters.Set("DELEGATED-FROM", value != null ? value.ToString() : null); }
        }

#if DATACONTRACT
        [DataMember(Order = 11)]
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

        public Attendee() : base() { }
        public Attendee(string value) : this()
        {
            this.Name = "ATTENDEE";
        }

        #endregion

        #region Operators

        static public implicit operator string(Attendee a)
        {
            return a != null ? a.Value : null;            
        }

        static public implicit operator Attendee(string a)
        {
            return new Attendee(a);
        }

        #endregion
    }
}
