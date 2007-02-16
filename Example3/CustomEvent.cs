using System;

namespace Example3
{
    /// <summary>
    /// A custom event that contains additional information about our event.
    /// </summary>
    class CustomEvent : DDay.iCal.Components.Event
    {
        private string m_AdditionalInformation;

        public string AdditionalInformation
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
