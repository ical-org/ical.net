using System;
using System.Diagnostics;
using System.Data;
using System.Configuration;
using DDay.iCal;
using DDay.iCal.Serialization;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace DDay.iCal
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

        public Journal()
        {            
        }

        void Initialize()
        {
            this.Name = Components.JOURNAL;
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
