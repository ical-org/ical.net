using System;
using System.Collections;
using System.Text;
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

        public UniqueComponent()
        {
            Initialize();
            CreateInitialize();
        }
        public UniqueComponent(string name) : base(name)
        {
            Initialize();
            CreateInitialize();            
        }

        private void CreateInitialize()
        {
            // Create a new UID for the component
            UID = new UIDFactory().Build();

            // Here, we don't simply set to DateTime.Now because DateTime.Now contains milliseconds, and
            // the iCalendar standard doesn't care at all about milliseconds.  Therefore, when comparing
            // two calendars, one generated, and one loaded from file, they may be functionally identical,
            // but be determined to be different due to millisecond differences.
            DateTime now = DateTime.Now;
            Created = new iCalDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            DTStamp = Created.Copy<IDateTime>();
        }

        private void Initialize()
        {
            Properties.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarProperty>>(Properties_ItemAdded);
            Properties.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarProperty>>(Properties_ItemRemoved);

            Attachments = new List<IAttachment>();
            Attendees = new List<IAttendee>();
            Categories = new List<string>();
            Comments = new List<string>();
            Contacts = new List<string>();
            RelatedComponents = new List<string>();
            RequestStatuses = new List<IRequestStatus>();
        }

        #endregion

        #region Public Properties

        virtual public IList<IAttachment> Attachments
        {
            get { return Properties.GetList<IAttachment>("ATTACH"); }
            set { Properties.SetList("ATTACH", value); }
        }

        virtual public IList<IAttendee> Attendees
        {
            get { return Properties.GetList<IAttendee>("ATTENDEE"); }
            set { Properties.SetList("ATTENDEE", value); }
        }

        virtual public IList<string> Categories
        {
            get { return Properties.GetList<string>("CATEGORIES"); }
            set { Properties.SetList("CATEGORIES", value); }            
        }

        virtual public string Class
        {
            get { return Properties.Get<string>("CLASS"); }
            set { Properties.Set("CLASS", value); }
        }

        virtual public IList<string> Comments
        {
            get { return Properties.GetList<string>("COMMENT"); }
            set { Properties.SetList("COMMENT", value); }
        }

        virtual public IList<string> Contacts
        {
            get { return Properties.GetList<string>("CONTACT"); }
            set { Properties.SetList("CONTACT", value); }
        }

        virtual public IDateTime Created
        {
            get { return Properties.Get<IDateTime>("CREATED"); }
            set { Properties.Set("CREATED", value); }
        }

        virtual public string Description
        {
            get { return Properties.Get<string>("DESCRIPTION"); }
            set { Properties.Set("DESCRIPTION", value); }
        }

        virtual public IDateTime DTStamp
        {
            get { return Properties.Get<IDateTime>("DTSTAMP"); }
            set { Properties.Set("DTSTAMP", value); }
        }

        virtual public IDateTime LastModified
        {
            get { return Properties.Get<IDateTime>("LAST-MODIFIED"); }
            set { Properties.Set("LAST-MODIFIED", value); }
        }

        virtual public IOrganizer Organizer
        {
            get { return Properties.Get<IOrganizer>("ORGANIZER"); }
            set { Properties.Set("ORGANIZER", value); }
        }

        virtual public int Priority
        {
            get { return Properties.Get<int>("PRIORITY"); }
            set { Properties.Set("PRIORITY", value); }
        }

        virtual public IList<string> RelatedComponents
        {
            get { return Properties.GetList<string>("RELATED-TO"); }
            set { Properties.SetList("RELATED-TO", value); }
        }

        virtual public IList<IRequestStatus> RequestStatuses
        {
            get { return Properties.GetList<IRequestStatus>("REQUEST-STATUS"); }
            set { Properties.SetList("REQUEST-STATUS", value); }
        }

        virtual public int Sequence
        {
            get { return Properties.Get<int>("SEQUENCE"); }
            set { Properties.Set("SEQUENCE", value); }
        }

        virtual public string Summary
        {
            get { return Properties.Get<string>("SUMMARY"); }
            set { Properties.Set("SUMMARY", value); }
        }

        virtual public Uri Url
        {
            get { return Properties.Get<Uri>("URL"); }
            set { Properties.Set("URL", value); }
        }

        #endregion

        #region Protected Methods

        protected void OnUIDChanged(string oldUID, string newUID)
        {
            if (UIDChanged != null)
                UIDChanged(this, oldUID, newUID);
        }

        #endregion

        #region Event Handlers

        void Properties_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty> e)
        {
            if (e.Object != null &&
                e.Object.Name != null &&
                string.Equals(e.Object.Name.ToUpper(), "UID"))
            {
                OnUIDChanged(e.Object.Value != null ? e.Object.Value.ToString() : null, null);
                e.Object.ValueChanged -= new EventHandler<ValueChangedEventArgs>(UID_ValueChanged);
            }
        }

        void Properties_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty> e)
        {
            if (e.Object != null &&
                e.Object.Name != null &&
                string.Equals(e.Object.Name.ToUpper(), "UID"))
            {
                OnUIDChanged(null, e.Object.Value != null ? e.Object.Value.ToString() : null);
                e.Object.ValueChanged += new EventHandler<ValueChangedEventArgs>(UID_ValueChanged);
            }
        }

        void UID_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            OnUIDChanged(
                e.OldValue != null ? e.OldValue.ToString() : null,
                e.NewValue != null ? e.NewValue.ToString() : null
            );
        }

        #endregion

        #region Overrides

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

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

        #endregion

        #region IUniqueComponent Members

        [field: NonSerialized]
        public event UIDChangedEventHandler UIDChanged;

        virtual public string UID
        {
            get { return Properties.Get<string>("UID"); }
            set { Properties.Set("UID", value); }
        }

        #endregion
    }
}
