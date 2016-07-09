using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using ical.NET.Collections;
using Ical.Net.DataTypes;
using Ical.Net.Factory;
using Ical.Net.Interfaces.Components;
using Ical.Net.Interfaces.DataTypes;
using Ical.Net.Interfaces.General;

namespace Ical.Net
{
    /// <summary>
    /// Represents a unique component, a component with a unique UID,
    /// which can be used to uniquely identify the component.    
    /// </summary>
    public class UniqueComponent : CalendarComponent, IUniqueComponent
    {
        // TODO: Add AddRelationship() public method.
        // This method will add the UID of a related component
        // to the Related_To property, along with any "RELTYPE"
        // parameter ("PARENT", "CHILD", "SIBLING", or other)
        // TODO: Add RemoveRelationship() public method.        

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
            if (string.IsNullOrEmpty(Uid))
            {
                // Create a new UID for the component
                Uid = new UidFactory().Build();
            }

            // NOTE: removed setting the 'CREATED' property here since it breaks serialization.
            // See https://sourceforge.net/projects/dday-ical/forums/forum/656447/topic/3754354
            if (DtStamp == null)
            {
                // Here, we don't simply set to DateTime.Now because DateTime.Now contains milliseconds, and
                // the iCalendar standard doesn't care at all about milliseconds.  Therefore, when comparing
                // two calendars, one generated, and one loaded from file, they may be functionally identical,
                // but be determined to be different due to millisecond differences.
                var now = DateTime.Now;
                DtStamp = new CalDateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            }
        }

        private void Initialize()
        {
            Properties.ItemAdded += Properties_ItemAdded;
            Properties.ItemRemoved += Properties_ItemRemoved;
        }

        public virtual IList<IAttendee> Attendees
        {
            get { return Properties.GetMany<IAttendee>("ATTENDEE"); }
            set { Properties.Set("ATTENDEE", value); }
        }

        public virtual IList<string> Comments
        {
            get { return Properties.GetMany<string>("COMMENT"); }
            set { Properties.Set("COMMENT", value); }
        }

        public virtual IDateTime DtStamp
        {
            get { return Properties.Get<IDateTime>("DTSTAMP"); }
            set { Properties.Set("DTSTAMP", value); }
        }

        public virtual IOrganizer Organizer
        {
            get { return Properties.Get<IOrganizer>("ORGANIZER"); }
            set { Properties.Set("ORGANIZER", value); }
        }

        public virtual IList<IRequestStatus> RequestStatuses
        {
            get { return Properties.GetMany<IRequestStatus>("REQUEST-STATUS"); }
            set { Properties.Set("REQUEST-STATUS", value); }
        }

        public virtual Uri Url
        {
            get { return Properties.Get<Uri>("URL"); }
            set { Properties.Set("URL", value); }
        }

        private void Properties_ItemRemoved(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            if (string.Equals(e.First.Name, "UID", StringComparison.OrdinalIgnoreCase))
            {
                OnUidChanged(e.First.Values.Cast<string>().FirstOrDefault(), null);
                e.First.ValueChanged -= Object_ValueChanged;
            }
        }

        private void Properties_ItemAdded(object sender, ObjectEventArgs<ICalendarProperty, int> e)
        {
            if (string.Equals(e.First.Name, "UID", StringComparison.OrdinalIgnoreCase))
            {
                OnUidChanged(null, e.First.Values.Cast<string>().FirstOrDefault());
                e.First.ValueChanged += Object_ValueChanged;
            }
        }

        private void Object_ValueChanged(object sender, ValueChangedEventArgs<object> e)
        {
            var oldValue = e.RemovedValues.OfType<string>().FirstOrDefault();
            var newValue = e.AddedValues.OfType<string>().FirstOrDefault();
            OnUidChanged(oldValue, newValue);
        }

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
            if (obj is RecurringComponent && obj != this)
            {
                var r = (RecurringComponent) obj;
                if (Uid != null)
                {
                    return Uid.Equals(r.Uid);
                }
                return Uid == r.Uid;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            if (Uid != null)
            {
                return Uid.GetHashCode();
            }
            return base.GetHashCode();
        }

        public virtual event EventHandler<ObjectEventArgs<string, string>> UidChanged;

        protected virtual void OnUidChanged(string oldUid, string newUid)
        {
            UidChanged?.Invoke(this, new ObjectEventArgs<string, string>(oldUid, newUid));
        }

        public virtual string Uid
        {
            get { return Properties.Get<string>("UID"); }
            set { Properties.Set("UID", value); }
        }
    }
}