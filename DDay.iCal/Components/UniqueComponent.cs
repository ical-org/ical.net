using System;
using System.Collections;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;
using System.Collections.Generic;

namespace DDay.iCal.Components
{
    /// <summary>
    /// Represents a unique component, a component with a unique UID,
    /// which can be used to uniquely identify the component.    
    /// </summary>
    public class UniqueComponent : ComponentBase
    {
        // FIXME: Add AddRelationship() public method.
        // This method will add the UID of a related component
        // to the Related_To property, along with any "RELTYPE"
        // parameter ("PARENT", "CHILD", "SIBLING", or other)
        // FIXME: Add RemoveRelationship() public method.        

        #region Constructors

        public UniqueComponent() : base() { }
        public UniqueComponent(iCalObject parent) : base(parent) { }
        public UniqueComponent(iCalObject parent, string name) : base(parent, name) { }        

        #endregion

        #region Private Fields

        private Text _UID;
        private Binary[] _Attach;
        private Cal_Address[] _Attendee;
        private TextCollection[] _Categories;
        private Text _Class;
        private Text[] _Comment;
        private Text[] _Contact;
        private iCalDateTime _Created;
        private Text _Description;
        private iCalDateTime _DTStamp;
        private iCalDateTime _Last_Modified;
        private Cal_Address _Organizer;
        private Integer _Priority;
        private Text[] _Related_To;
        private RequestStatus[] _Request_Status;
        private Integer _Sequence = 0;
        private Text _Summary;
        private URI _Url;
        
        #endregion

        #region Public Events

        public delegate void UIDChangedEventHandler(object sender, Text OldUID, Text NewUID);
        public event UIDChangedEventHandler UIDChanged;

        #endregion

        #region Public Properties

        [Serialized]
        virtual public Binary[] Attach
        {
            get { return _Attach; }
            set { _Attach = value; }
        }

        [Serialized]
        virtual public Cal_Address[] Attendee
        {
            get { return _Attendee; }
            set
            {
                if (!object.Equals(_Attendee, value))
                {
                    _Attendee = value;

                    // NOTE: Fixes bug #1835469 - Organizer property not serializing correctly
                    if (_Attendee != null)
                    {
                        foreach (Cal_Address addr in _Attendee)
                            addr.Name = Cal_Address.ATTENDEE;
                    }
                }
            }
        }

        [Serialized]
        virtual public TextCollection[] Categories
        {
            get { return _Categories; }                    
            set { _Categories = value; }            
        }

        [Serialized]
        virtual public Text Class
        {
            get { return _Class; }
            set
            {
                _Class = value;
                if (_Class != null)
                    _Class.Name = "CLASS";
            }
        }

        [Serialized]
        virtual public Text[] Comment
        {
            get { return _Comment; }
            set { _Comment = value; }
        }

        [Serialized]
        virtual public Text[] Contact
        {
            get { return _Contact; }
            set { _Contact = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        virtual public iCalDateTime Created
        {
            get { return _Created; }
            set
            {
                _Created = value;
                if (_Created != null)
                    _Created.Name = "CREATED";
            }
        }

        [Serialized]
        virtual public Text Description
        {
            get { return _Description; }
            set
            {
                _Description = value;
                if (_Description != null)
                    _Description.Name = "DESCRIPTION";
            }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        virtual public iCalDateTime DTStamp
        {
            get { return _DTStamp; }
            set
            {
                _DTStamp = value;
                if (_DTStamp != null)
                    _DTStamp.Name = "DTSTAMP";
            }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        virtual public iCalDateTime Last_Modified
        {
            get { return _Last_Modified; }
            set
            {
                _Last_Modified = value;
                if (_Last_Modified != null)
                    _Last_Modified.Name = "LAST-MODIFIED";
            }
        }

        [Serialized]
        virtual public Cal_Address Organizer
        {
            get { return _Organizer; }
            set
            {
                if (!object.Equals(_Organizer, value))
                {
                    _Organizer = value;

                    // NOTE: Fixes bug #1835469 - Organizer property not serializing correctly
                    if (_Organizer != null)
                        _Organizer.Name = Cal_Address.ORGANIZER;
                }
            }
        }

        [Serialized]
        virtual public Integer Priority
        {
            get { return _Priority; }
            set
            {
                _Priority = value;
                if (_Priority != null)
                    _Priority.Name = "PRIORITY";
            }
        }

        [Serialized]
        virtual public Text[] Related_To
        {
            get { return _Related_To; }
            set { _Related_To = value; }
        }

        [Serialized]
        virtual public RequestStatus[] Request_Status
        {
            get { return _Request_Status; }
            set { _Request_Status = value; }
        }

        [Serialized]
        virtual public Integer Sequence
        {
            get { return _Sequence; }
            set
            {
                _Sequence = value;
                if (_Sequence != null)
                    _Sequence.Name = "SEQUENCE";
            }
        }

        [Serialized]
        virtual public Text Summary
        {
            get { return _Summary; }
            set
            {
                _Summary = value;
                if (_Summary != null)
                    _Summary.Name = "SUMMARY";
            }
        }

        [Serialized]
        virtual public Text UID
        {
            get
            {
                return _UID;
            }
            set
            {
                if (!object.Equals(_UID, value))
                {
                    Text oldUID = _UID;

                    _UID = value;
                    if (_UID != null)
                    {
                        if (_UID.Value == null)
                            _UID = null;
                        else _UID.Name = "UID";
                    }

                    OnUIDChanged(oldUID, _UID);
                }
            }
        }

        [Serialized]
        virtual public URI Url
        {
            get { return _Url; }
            set
            {
                _Url = value;
                if (_Url != null)
                    _Url.Name = "URL";
            }
        }

        virtual public string Category
        {
            get
            {
                if (Categories != null && Categories.Length > 0 && Categories[0].Values.Count > 0)
                    return Categories[0].Values[0];
                return null;
            }
            set
            {
                if (!object.Equals(Category, value))
                {
                    if (Categories == null || Categories.Length == 0)
                        Categories = new TextCollection[1];
                    Categories[0] = new TextCollection(value);
                    Categories[0].Name = "CATEGORIES";
                }                
            }
        }

        #endregion

        #region Protected Methods

        protected void OnUIDChanged(Text oldUID, Text newUID)
        {
            if (UIDChanged != null)
                UIDChanged(this, oldUID, newUID);
        }

        #endregion

        #region Public Methods

        virtual public void AddAttendee(Cal_Address attendee)
        {
            List<Cal_Address> attendees = new List<Cal_Address>();
            if (Attendee != null)
                attendees.AddRange(Attendee);

            // NOTE: Fixes bug #1835469 - Organizer property not serializing correctly
            attendee.Name = Cal_Address.ATTENDEE;
            attendees.Add(attendee);
            Attendee = attendees.ToArray();
        }

        virtual public void RemoveAttendee(Cal_Address attendee)
        {
            List<Cal_Address> attendees = new List<Cal_Address>();
            if (Attendee != null)
                attendees.AddRange(Attendee);

            attendees.Remove(attendee);
            if (attendees.Count > 0)
                Attendee = attendees.ToArray();
            else Attendee = null;
        }

        virtual public void AddCategory(string categoryName)
        {
            Text cn = new Text(categoryName);
            if (Categories != null)
            {
                foreach (TextCollection tc in Categories)
                {
                    if (tc.Values.Contains(cn))
                    {
                        return;
                    }
                }
            }

            if (Categories == null ||
                Categories.Length == 0)
            {
                Categories = new TextCollection[1] { new TextCollection(categoryName) };
                Categories[0].Name = "CATEGORIES";
            }
            else
            {
                Categories[0].Values.Add(cn);
            }
        }

        virtual public void RemoveCategory(string categoryName)
        {
            if (Categories != null)
            {
                Text cn = new Text(categoryName);
                foreach (TextCollection tc in Categories)
                {
                    if (tc.Values.Contains(cn))
                    {
                        tc.Values.Remove(cn);
                        return;
                    }
                }
            }
        }

        virtual public void AddComment(string comment)
        {
            if (Comment == null)
                Comment = new Text[] { comment };
            else
            {
                Text[] comments = Comment;
                Comment = new Text[Comment.Length + 1];
                comments.CopyTo(Comment, 0);
                Comment[Comment.Length - 1] = comment;                
            }
        }

        virtual public void RemoveComment(string comment)
        {
            if (Comment == null)
                return;
            else
            {
                int index = Array.IndexOf<Text>(Comment, comment);
                if (index >= 0)
                {
                    Text[] comments = new Text[Comment.Length - 1];
                    Array.Copy(Comment, 0, comments, 0, index);
                    Array.Copy(Comment, index + 1, comments, index, comments.Length - index);
                    Comment = comments;
                }
            }
        }

        virtual public void AddContact(string contact)
        {
            AddContact(contact, null);            
        }

        virtual public void AddContact(string contact, Uri alternateTextRepresentation)
        {
            if (Contact == null)
            {
                Contact = new Text[] { contact };
                if (alternateTextRepresentation != null)
                    Contact[0].AddParameter("ALTREP", alternateTextRepresentation.OriginalString);
            }
            else
            {
                Text[] contacts = Contact;
                Contact = new Text[Contact.Length + 1];
                contacts.CopyTo(Contact, 0);
                Contact[Contact.Length - 1] = contact;
                if (alternateTextRepresentation != null)
                    Contact[Contact.Length - 1].AddParameter("ALTREP", alternateTextRepresentation.OriginalString);
            }
        }

        virtual public void RemoveContact(string contact)
        {
            if (Comment == null)
                return;
            else
            {
                int index = Array.IndexOf<Text>(Contact, contact);
                if (index >= 0)
                {
                    Text[] contacts = new Text[Contact.Length - 1];
                    Array.Copy(Contact, 0, contacts, 0, index);
                    Array.Copy(Contact, index + 1, contacts, index, contacts.Length - index);
                    Contact = contacts;
                }
            }
        }

        #endregion

        #region Static Public Methods

        static public Text NewUID()
        {
            return new Text(Guid.NewGuid().ToString());
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (obj is RecurringComponent && 
                obj != this)
            {
                RecurringComponent r = (RecurringComponent)obj;                
                if (UID != null)
                    return UID.Equals(r.UID);
                else return UID == r.UID;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (UID != null)
                return UID.GetHashCode();
            return base.GetHashCode();
        }

        public override void CreateInitialize()
        {
            base.CreateInitialize();

            // Create a new UID for the component
            UID = UniqueComponent.NewUID();

            // Here, we don't simply set to DateTime.Now because DateTime.Now contains milliseconds, and
            // the iCalendar standard doesn't care at all about milliseconds.  Therefore, when comparing
            // two calendars, one generated, and one loaded from file, they may be functionally identical,
            // but be determined to be different due to millisecond differences.
            DateTime now = DateTime.Now;
            Created = new iCalDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            DTStamp = Created.Copy();
        }

        #endregion
    }
}
