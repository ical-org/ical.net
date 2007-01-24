using System;
using System.Collections;
using System.Text;
using DDay.iCal.Components;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace DDay.iCal.Objects
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
        
        #endregion

        #region Public Events

        public delegate void UIDChangedEventHandler(object sender, Text OldUID, Text NewUID);
        public event UIDChangedEventHandler UIDChanged;

        #endregion

        #region Public Fields

        [Serialized]
        public Binary[] Attach;
        [Serialized]
        public Cal_Address[] Attendee;
        [Serialized]
        public TextCollection[] Categories;
        [Serialized]
        public Text Class;
        [Serialized]
        public Text[] Comment;
        [Serialized]
        public Text[] Contact;
        [Serialized, DefaultValueType("DATE-TIME")]
        public Date_Time Created; 
        [Serialized]
        public Text Description;
        [Serialized, DefaultValueType("DATE-TIME")]
        public Date_Time DTStamp;
        [Serialized, DefaultValueType("DATE-TIME")]
        public Date_Time LastModified;
        [Serialized]
        public Cal_Address Organizer;
        [Serialized]
        public Integer Priority;
        [Serialized]
        public Text[] Related_To;
        [Serialized]
        public RequestStatus[] RequestStatus;
        [Serialized]
        public Integer Sequence;
        [Serialized]
        public Text Summary;
        [Serialized]
        public URI Url;

        #endregion        

        #region Public Properties

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

        public override void SetContentLineValue(ContentLine cl)
        {
            base.SetContentLineValue(cl);

            if (cl.Name == "UID")
            {
                Text text = new Text();
                text.ContentLine = cl;
                UID = text.Value;
            }
        }

        #endregion
    }
}
