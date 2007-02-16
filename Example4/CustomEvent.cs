using System;
using DDay.iCal.DataTypes;
using DDay.iCal.Serialization;

namespace Example4
{
    /// <summary>
    /// A custom event that contains additional information about our event.
    /// </summary>
    class CustomEvent : DDay.iCal.Components.Event
    {
        private string m_AdditionalInformation;

        /// <summary>
        /// Additional information about our event.
        /// We use the SerializedAttribute and NonstandardAttribute
        /// attributes to tell the serialization engine that we
        /// want to serialize this property, and that this property
        /// is not an RFC2445-standard property, respectively.
        /// <remarks>
        /// Note that we must use an iCalObject-based data type
        /// for this property so it is serialized correctly.
        /// </remarks>
        /// </summary>
        [Serialized, Nonstandard]
        public Text AdditionalInformation
        {
            get { return m_AdditionalInformation; }
            set { m_AdditionalInformation = value; }
        }

        /// <summary>
        /// A default constructor for iCalendar objects
        /// </summary>
        /// <param name="parent">The parent object that contains this one, or NULL.</param>
        public CustomEvent(DDay.iCal.Objects.iCalObject parent) : base(parent) { }
    }
}
