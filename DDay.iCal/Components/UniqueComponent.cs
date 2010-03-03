using System;
using System.Collections;
using System.Text;
using DDay.iCal;
using DDay.iCal;
using DDay.iCal.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DDay.iCal
{
    /// <summary>
    /// Represents a unique component, a component with a unique UID,
    /// which can be used to uniquely identify the component.    
    /// </summary>
#if DATACONTRACT
    [DataContract(Name = "UniqueComponent", Namespace = "http://www.ddaysoftware.com/dday.ical/2009/07/")]
#endif
    [Serializable]
    public class UniqueComponent : 
        CalendarComponent,
        IUniqueComponent
    {
        // TODO: Add AddRelationship() public method.
        // This method will add the UID of a related component
        // to the Related_To property, along with any "RELTYPE"
        // parameter ("PARENT", "CHILD", "SIBLING", or other)
        // TODO: Add RemoveRelationship() public method.        

        #region Constructors

        public UniqueComponent() : base() { }
        public UniqueComponent(string name) : base(name) { }        

        #endregion

        #region Private Fields

        private IBinary[] _Attach;
        private IAttendee[] _Attendee;
        private ITextCollection[] _Categories;
        private Text _Class;
        private Text[] _Comment;
        private Text[] _Contact;
        private iCalDateTime _Created;
        private Text _Description;
        private iCalDateTime _DTStamp;
        private iCalDateTime _Last_Modified;
        private IOrganizer _Organizer;
        private IInteger _Priority;
        private Text[] _Related_To;
        private IRequestStatus[] _Request_Status;
        private IInteger _Sequence = 0;
        private Text _Summary;
        private IURI _Url;
        
        #endregion

        #region IUniqueComponent Members
        
        [field: NonSerialized]
        public event UIDChangedEventHandler UIDChanged;

        virtual public Text UID
        {
            get
            {
                return Properties.Get<Text>("UID");
            }
            set
            {
                if (!object.Equals(UID, value))
                {
                    Text oldUID = UID;

                    Properties.Set("UID", value);
                    OnUIDChanged(oldUID, value);
                }
            }
        }

        #endregion

        #region Public Properties

        virtual public IBinary[] Attach
        {
            get { return Properties.Get<Binary[]>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

        virtual public IAttendee[] Attendee
        {
            get { return Properties.Get<Attendee[]>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

        virtual public ITextCollection[] Categories
        {
            get { return Properties.Get<TextCollection[]>("CATEGORIES"); }
            set { Properties.Set("CATEGORIES", value); }
        }

        virtual public IText Class
        {
            get { return Properties.Get<Text>("CLASS"); }
            set { Properties.Set("CLASS", value); }
        }

        virtual public IText[] Comment
        {
            get { return Properties.Get<Text[]>("COMMENT"); }
            set { Properties.Set("COMMENT", value); }
        }

        virtual public IText[] Contact
        {
            get { return Properties.Get<Text[]>("CONTACT"); }
            set { Properties.Set("CONTACT", value); }
        }

        virtual public iCalDateTime Created
        {
            get { return Properties.Get<iCalDateTime>("CREATED"); }
            set { Properties.Set("CREATED", value); }
        }

        virtual public IText Description
        {
            get { return Properties.Get<Text>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

        virtual public iCalDateTime DTStamp
        {
            get { return Properties.Get<iCalDateTime>("DTSTAMP"); }
            set { Properties.Set("DTSTAMP", value); }
        }

        virtual public iCalDateTime LastModified
        {
            get { return Properties.Get<iCalDateTime>("LAST-MODIFIED"); }
            set { Properties.Set("LAST-MODIFIED", value); }
        }

        virtual public IOrganizer Organizer
        {
            get { return Properties.Get<Organizer>("ORGANIZER"); }
            set { Properties.Set("ORGANIZER", value); }
        }

        virtual public Integer Priority
        {
            get { return Properties.Get<Integer>("PRIORITY"); }
            set { Properties.Set("PRIORITY", value); }
        }

        virtual public Text[] RelatedTo
        {
            get { return Properties.Get<Text[]>("RELATED-TO"); }
            set { Properties.Set("RELATED-TO", value); }
        }

        virtual public RequestStatus[] RequestStatus
        {
            get { return Properties.Get<RequestStatus[]>("REQUEST-STATUS"); }
            set { Properties.Set("REQUEST-STATUS", value); }
        }

        virtual public Integer Sequence
        {
            get { return Properties.Get<Integer>("SEQUENCE"); }
            set { Properties.Set("SEQUENCE", value); }
        }

        virtual public Text Summary
        {
            get { return Properties.Get<Text>("SUMMARY"); }
            set { Properties.Set("SUMMARY", value); }
        }

        virtual public URI Url
        {
            get { return Properties.Get<URI>("URL"); }
            set { Properties.Set("URL", value); }
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

        virtual public void AddAttachment(byte[] data)
        {
            Binary binary = new Binary();
            binary.Data = data;

            AddAttachment(binary);
        }

        virtual public void AddAttachment(Binary binary)
        {
            if (Attach == null)
            {
                Attach = new Binary[] { binary };
            }
            else
            {
                Binary[] attachments = Attach;
                Attach = new Binary[Attach.Length + 1];
                attachments.CopyTo(Attach, 0);
                Attach[Attach.Length - 1] = binary;
            }
        }

        virtual public void RemoveAttachment(Binary binary)
        {
            if (Attach == null)
                return;
            else
            {
                int index = Array.IndexOf<Binary>(Attach, binary);
                if (index >= 0)
                {
                    Binary[] attachments = new Binary[Attach.Length - 1];
                    Array.Copy(Attach, 0, attachments, 0, index);
                    Array.Copy(Attach, index + 1, attachments, index, attachments.Length - index);
                    Attach = attachments;
                }
            }
        }


        virtual public void AddAttendee(Attendee attendee)
        {
            List<Attendee> attendees = new List<Attendee>();
            if (Attendee != null)
                attendees.AddRange(Attendee);

            attendees.Add(attendee);
            Attendee = attendees.ToArray();
        }

        virtual public void RemoveAttendee(Attendee attendee)
        {
            List<Attendee> attendees = new List<Attendee>();
            if (Attendee != null)
                attendees.AddRange(Attendee);

            attendees.Remove(attendee);
            if (attendees.Count > 0)
                Attendee = attendees.ToArray();
            else Attendee = null;
        }

        virtual public void AddCategory(string categoryName)
        {
            Text cn = categoryName;
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
                Text cn = categoryName;
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

        virtual public void AddRelatedTo(string uid)
        {
            AddRelatedTo(uid, null);
        }

        virtual public void AddRelatedTo(string uid, string relationshipType)
        {
            Text text = uid;
            if (relationshipType != null)
                text.AddParameter(new CalendarParameter("RELTYPE", relationshipType));

            if (RelatedTo == null)
            {
                RelatedTo = new Text[] { text };
            }
            else
            {
                Text[] related_to = RelatedTo;
                RelatedTo = new Text[RelatedTo.Length + 1];
                related_to.CopyTo(RelatedTo, 0);
                RelatedTo[RelatedTo.Length - 1] = text;                
            }
        }

        virtual public void RemoveRelatedTo(string uid)
        {
            if (RelatedTo == null)
                return;
            else
            {
                int index = -1;
                for (int i = 0; i < RelatedTo.Length; i++)
                {
                    if (RelatedTo[i].Value.Equals(uid) ||
                        RelatedTo[i].Value.Equals("<" + uid + ">"))
                    {
                        index = i;
                        break;
                    }
                }
                
                if (index >= 0)
                {
                    Text[] related_to = new Text[RelatedTo.Length - 1];
                    Array.Copy(RelatedTo, 0, related_to, 0, index);
                    Array.Copy(RelatedTo, index + 1, related_to, index, related_to.Length - index);
                    RelatedTo = related_to;
                }
            }
        }

        #endregion

        #region Static Public Methods

        static public Text NewUID()
        {
            return Guid.NewGuid().ToString();
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
            DTStamp = Created;
        }

        #endregion
    }
}
