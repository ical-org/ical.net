using System;
using System.Runtime.Serialization;
using Ical.Net.Interfaces.Components;

namespace Ical.Net
{
    /// <summary>
    /// A class that represents an RFC 5545 VJOURNAL component.
    /// </summary>
#if !SILVERLIGHT
    [Serializable]
#endif
    public class Journal : 
        RecurringComponent,
        IJournal
    {
        #region IJournal Members
        
        public JournalStatus Status
        {
            get { return Properties.Get<JournalStatus>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        } 

        #endregion

        #region Constructors

        void Initialize()
        {
            Name = Components.Journal;
        }

        #endregion

        #region Overrides

        protected override bool EvaluationIncludesReferenceDate
        {
            get
            {
                return true;
            }
        }

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }

        #endregion
    }
}
