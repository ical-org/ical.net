using System;
using System.Collections;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

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

        private Text m_UID;
        private Binary[] m_Attach;
        private Cal_Address[] m_Attendee;        
        private TextCollection[] m_Categories;
        private Text m_Class;        
        private Text[] m_Comment;        
        private Text[] m_Contact;
        private Date_Time m_Created;        
        private Text m_Description;        
        private Date_Time m_DTStamp;        
        private Date_Time m_Last_Modified;        
        private Cal_Address m_Organizer;        
        private Integer m_Priority;        
        private Text[] m_Related_To;        
        private RequestStatus[] m_RequestStatus;        
        private Integer m_Sequence;
        private Text m_Summary;
        private URI m_Url;        
        
        #endregion

        #region Public Events

        public delegate void UIDChangedEventHandler(object sender, Text OldUID, Text NewUID);
        public event UIDChangedEventHandler UIDChanged;

        #endregion

        #region Public Properties

        [Serialized]
        public Binary[] Attach
        {
            get { return m_Attach; }
            set { m_Attach = value; }
        }

        [Serialized]
        public Cal_Address[] Attendee
        {
            get { return m_Attendee; }
            set { m_Attendee = value; }
        }

        [Serialized]
        public TextCollection[] Categories
        {
            get { return m_Categories; }
            set { m_Categories = value; }
        }

        [Serialized]
        public Text Class
        {
            get { return m_Class; }
            set { m_Class = value; }
        }

        [Serialized]
        public Text[] Comment
        {
            get { return m_Comment; }
            set { m_Comment = value; }
        }

        [Serialized]
        public Text[] Contact
        {
            get { return m_Contact; }
            set { m_Contact = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time Created
        {
            get { return m_Created; }
            set { m_Created = value; }
        }

        [Serialized]
        public Text Description
        {
            get { return m_Description; }
            set { m_Description = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time DTStamp
        {
            get { return m_DTStamp; }
            set { m_DTStamp = value; }
        }

        [Serialized, DefaultValueType("DATE-TIME"), ForceUTC]
        public Date_Time Last_Modified
        {
            get { return m_Last_Modified; }
            set { m_Last_Modified = value; }
        }

        [Serialized]
        public Cal_Address Organizer
        {
            get { return m_Organizer; }
            set { m_Organizer = value; }
        }

        [Serialized]
        public Integer Priority
        {
            get { return m_Priority; }
            set { m_Priority = value; }
        }

        [Serialized]
        public Text[] Related_To
        {
            get { return m_Related_To; }
            set { m_Related_To = value; }
        }

        [Serialized]
        public RequestStatus[] RequestStatus
        {
            get { return m_RequestStatus; }
            set { m_RequestStatus = value; }
        }

        [Serialized]
        public Integer Sequence
        {
            get { return m_Sequence; }
            set { m_Sequence = value; }
        }

        [Serialized]
        public Text Summary
        {
            get { return m_Summary; }
            set { m_Summary = value; }
        }

        [Serialized]
        public Text UID
        {
            get
            {
                return m_UID;
            }
            set
            {
                if ((UID == null && value != null) ||
                    (UID != null && !UID.Equals(value)))
                {
                    Text oldUID = m_UID;

                    // If the value of UID is somehow null, then set our value to null
                    if (value == null || value.Value == null)
                        m_UID = null;
                    else m_UID = new Text(value.Value);

                    if (UIDChanged != null)
                        UIDChanged(this, oldUID, UID);
                }
            }
        }

        [Serialized]
        public URI Url
        {
            get { return m_Url; }
            set { m_Url = value; }
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
                if (Categories == null || Categories.Length == 0)
                    Categories = new TextCollection[1];
                Categories[0] = new TextCollection(value);
                Categories[0].Name = "CATEGORY";
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
            Created = new Date_Time(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            DTStamp = Created.Copy();
        }

        #endregion
    }
}
