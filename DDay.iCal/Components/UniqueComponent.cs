using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DDay.Collections;

namespace DDay.iCal
{
    /// <summary>
    /// Represents a unique component, a component with a unique UID,
    /// which can be used to uniquely identify the component.    
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
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
            EnsureProperties();
        }
        public UniqueComponent(string name) : base(name)
        {
            Initialize();
            EnsureProperties();            
        }

        private void EnsureProperties()
        {
            if (string.IsNullOrEmpty(UID))
            {
                // Create a new UID for the component
                UID = new UIDFactory().Build();
            }

            // NOTE: removed setting the 'CREATED' property here since it breaks serialization.
            // See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3754354
            if (DTStamp == null)
            {
                // Here, we don't simply set to DateTime.Now because DateTime.Now contains milliseconds, and
                // the iCalendar standard doesn't care at all about milliseconds.  Therefore, when comparing
                // two calendars, one generated, and one loaded from file, they may be functionally identical,
                // but be determined to be different due to millisecond differences.
                DateTime now = DateTime.Now;
                DTStamp = new iCalDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);                
            }            
        }

        private void Initialize()
        {
            Properties.ItemAdded += new EventHandler<ObjectEventArgs<ICalendarProperty, int>>(Properties_ItemAdded);
            Properties.ItemRemoved += new EventHandler<ObjectEventArgs<ICalendarProperty, int>>(Properties_ItemRemoved);
        }

        #endregion

        #region Public Properties

        virtual public IList<IAttendee> Attendees
        {
            get { return Properties.GetMany<IAttendee>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

        virtual public IList<string> Comments
        {
            get { return Properties.GetMany<string>("COMMENT"); }
            set { Properties.Set("COMMENT", value); }
        }

        virtual public IDateTime DTStamp
        {
            get { return Properties.Get<IDateTime>("DTSTAMP"); }
            set { Properties.Set("DTSTAMP", value); }
        }

        virtual public IOrganizer Organizer
        {
            get { return Properties.Get<IOrganizer>("ORGANIZER"); }
            set { Properties.Set("ORGANIZER", value); }
        }

        virtual public IList<IRequestStatus> RequestStatuses
        {
            get { return Properties.GetMany<IRequestStatus>("REQUEST-STATUS"); }
            set { Properties.Set("REQUEST-STATUS", value); }
        }

        virtual public Uri Url
        {
            get { return Properties.Get<Uri>("URL"); }
            set { Properties.Set("URL", value); }
        }

        #endregion

        #region Event Handlers

        void Properties_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            if (e.First != null &&
                e.First.Name != null &&
                string.Equals(e.First.Name.ToUpper(), "UID"))
            {
                OnUIDChanged(e.First.Values.Cast<string>().FirstOrDefault(), null);
                e.First.ValueChanged -= Object_ValueChanged;
            }
        }

        void Properties_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            if (e.First != null &&
                e.First.Name != null &&
                string.Equals(e.First.Name.ToUpper(), "UID"))
            {
                OnUIDChanged(null, e.First.Values.Cast<string>().FirstOrDefault());
                e.First.ValueChanged += Object_ValueChanged;
            }
        }

        void Object_ValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            string oldValue = e.RemovedValues.OfType<string>().FirstOrDefault();
            string newValue = e.AddedValues.OfType<string>().FirstOrDefault();
            OnUIDChanged(oldValue, newValue);
        }

        #endregion

        #region Overrides
        
        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        protected override void OnDeserialized(StreamingContext context)
        {
            base.OnDeserialized(context);

            EnsureProperties();
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

        virtual public event EventHandler<ObjectEventArgs<string, string>> UIDChanged;

        virtual protected void OnUIDChanged(string oldUID, string newUID)
        {
            if (UIDChanged != null)
                UIDChanged(this, new ObjectEventArgs<string, string>(oldUID, newUID));
        }

        virtual public string UID
        {
            get { return Properties.Get<string>("UID"); }
            set { Properties.Set("UID", value); }
        }

        #endregion
    }
}
