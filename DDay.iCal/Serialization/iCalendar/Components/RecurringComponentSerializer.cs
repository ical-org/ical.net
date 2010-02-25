using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class RecurringComponentSerializer : UniqueComponentSerializer
    {
        #region Protected Properties

        protected RecurringComponent RecurringComponent
        {
            get { return Component as RecurringComponent; }
        }

        #endregion

        #region Constructors

        public RecurringComponentSerializer(RecurringComponent rc) : base(rc) { }
        
        #endregion

        #region Overrides

        public override void Serialize(Stream stream, Encoding encoding)
        {
            if (RecurringComponent != null)
            {
                if (RecurringComponent.ExDate != null)
                {
                    foreach (RecurrenceDates rd in RecurringComponent.ExDate)
                        rd.Name = "EXDATE";
                }
                if (RecurringComponent.ExRule != null)
                {
                    foreach (RecurrencePattern rp in RecurringComponent.ExRule)
                        rp.Name = "EXRULE";
                }
                if (RecurringComponent.RDate != null)
                {
                    foreach (RecurrenceDates rd in RecurringComponent.RDate)
                        rd.Name = "RDATE";
                }
                if (RecurringComponent.RRule != null)
                {
                    foreach (RecurrencePattern rp in RecurringComponent.RRule)
                        rp.Name = "RRULE";
                }  
            }

            base.Serialize(stream, encoding);
        }

        #endregion
    }
}
