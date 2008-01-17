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
        private Date_Time _Created;
        private Text _Description;
        private Date_Time _DTStamp;
        private Date_Time _Last_Modified;
        private Cal_Address _Organizer;
        private Integer _Priority;
        private Text[] _Related_To;
        private RequestStatus[] _RequestStatus;
        private Integer _Sequence;
        private Text _Summary;
        private URI _Url;
        
        #endregion

        #region Public Events

        public delegate void UIDChangedEventHandler(object sender, Text OldUID, Text NewUID);
        public event UIDChangedEventHandler UIDChanged;

        #endregion

        #region Public Properties

        [Serialized]
        public Binary[] Attach
        {
            get { return _Attach; }
            set { _Attach = value; }
        }

        [Serialized]
        public Cal_Address[] Attendee
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
        public TextCollection[] Categories
        {
            get { return _Categories; }                    
            set { _Categories = value; }            
        }

        [Serialized]
        public Text Class
        {
            get { return _Class; }
            set { _Class = value; }
        }

        [Serialized]
        public Text[] Comment
        {
            get { return _Comment; }
            set { _Comment = value; }
        }

        [Serialized]
        public Text[] Contact
        {
            get { return _Contact; }
            set { _Contact = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time Created
        {
            get { return _Created; }
            set { _Created = value; }
        }

        [Serialized]
        public Text Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time DTStamp
        {
            get { return _DTStamp; }
            set { _DTStamp = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time Last_Modified
        {
            get { return _Last_Modified; }
            set { _Last_Modified = value; }
        }

        [Serialized]
        public Cal_Address Organizer
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
        public Integer Priority
        {
            get { return _Priority; }
            set { _Priority = value; }
        }

        [Serialized]
        public Text[] Related_To
        {
            get { return _Related_To; }
            set { _Related_To = value; }
        }

        [Serialized]
        public RequestStatus[] RequestStatus
        {
            get { return _RequestStatus; }
            set { _RequestStatus = value; }
        }

        [Serialized]
        public Integer Sequence
        {
            get { return _Sequence; }
            set { _Sequence = value; }
        }

        [Serialized]
        public Text Summary
        {
            get { return _Summary; }
            set { _Summary = value; }
        }

        [Serialized]
        public Text UID
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
                    if (_UID.Value == null)
                        _UID = null;

                    OnUIDChanged(oldUID, _UID);
                }
            }
        }

        [Serialized]
        public URI Url
        {
            get { return _Url; }
            set { _Url = value; }
        }

        public string Category
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
                    Categories[0].Name = "CATEGORY";
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

        public void AddAttendee(Cal_Address attendee)
        {
            List<Cal_Address> attendees = new List<Cal_Address>();
            if (Attendee != null)
                attendees.AddRange(Attendee);

            // NOTE: Fixes bug #1835469 - Organizer property not serializing correctly
            attendee.Name = Cal_Address.ATTENDEE;
            attendees.Add(attendee);
            Attendee = attendees.ToArray();
        }

        public void RemoveAttendee(Cal_Address attendee)
        {
            List<Cal_Address> attendees = new List<Cal_Address>();
            if (Attendee != null)
                attendees.AddRange(Attendee);

            attendees.Remove(attendee);
            if (attendees.Count > 0)
                Attendee = attendees.ToArray();
            else Attendee = null;
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
            Created = new Date_Time(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
            DTStamp = Created.Copy();
        }

        #endregion
    }
}
