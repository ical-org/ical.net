using System.Runtime.Serialization;

namespace ical.net
{
    /// <summary> A class that represents an RFC 5545 VJOURNAL component. </summary>
    public class Journal : RecurringComponent
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