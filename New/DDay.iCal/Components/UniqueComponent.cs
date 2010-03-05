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

        #region IUniqueComponent Members
        
        [field: NonSerialized]
        public event UIDChangedEventHandler UIDChanged;

        virtual public string UID
        {
            get
            {
                return Properties.Get<string>("UID");
            }
            set
            {
                if (!object.Equals(UID, value))
                {
                    string oldUID = UID;

                    Properties.Set("UID", value);
                    OnUIDChanged(oldUID, value);
                }
            }
        }

        #endregion

        #region Public Properties

        virtual public IList<IBinary> Attachments
        {
            get { return Properties.Get<IList<IBinary>>("ATTACH"); }
            set { Properties.Set("ATTACH", value); }
        }

        virtual public IList<IAttendee> Attendees
        {
            get { return Properties.Get<IList<IAttendee>>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

        virtual public IList<string> Categories
        {
            get { return Properties.Get<IList<string>>("CATEGORIES"); }
            set { Properties.Set("CATEGORIES", value); }
        }

        virtual public string Class
        {
            get { return Properties.Get<string>("CLASS"); }
            set { Properties.Set("CLASS", value); }
        }

        virtual public IList<string> Comments
        {
            get { return Properties.Get<IList<string>>("COMMENT"); }
            set { Properties.Set("COMMENT", value); }
        }

        virtual public IList<string> Contacts
        {
            get { return Properties.Get<IList<string>>("CONTACT"); }
            set { Properties.Set("CONTACT", value); }
        }

        virtual public iCalDateTime Created
        {
            get { return Properties.Get<iCalDateTime>("CREATED"); }
            set { Properties.Set("CREATED", value); }
        }

        virtual public string Description
        {
            get { return Properties.Get<string>("DESCRIPTION"); }
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
            get { return Properties.Get<IList<string>>("RELATED-TO"); }
            set { Properties.Set("RELATED-TO", value); }
        }

        virtual public IList<IRequestStatus> RequestStatuses
        {
            get { return Properties.Get<IList<IRequestStatus>>("REQUEST-STATUS"); }
            set { Properties.Set("REQUEST-STATUS", value); }
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
            UID = new UIDFactory().New();

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
