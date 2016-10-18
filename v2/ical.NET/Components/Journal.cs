using System.Runtime.Serialization;
using Ical.Net.Interfaces.Components;

namespace Ical.Net
{
    /// <summary>
    /// A class that represents an RFC 5545 VJOURNAL component.
    /// </summary>
    public class Journal : RecurringComponent, IJournal
    {
        public JournalStatus Status
        {
            get { return Properties.Get<JournalStatus>("STATUS"); }
            set { Properties.Set("STATUS", value); }
        }

        private void Initialize()
        {
            Name = Components.Journal;
        }

        protected override bool EvaluationIncludesReferenceDate => true;

        protected override void OnDeserializing(StreamingContext context)
        {
            base.OnDeserializing(context);

            Initialize();
        }
    }
}