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
    public class EventSerializer : RecurringComponentSerializer
    {
        #region Protected Properties

        protected Event Event
        {
            get { return Component as Event; }
        }

        #endregion

        #region Constructors

        public EventSerializer(Event evt) : base(evt) { }
        
        #endregion

        #region Overrides

        public override void Serialize(Stream stream, Encoding encoding)
        {
            if (Event != null)
            {
                if (Event.Resources != null)
                {
                    foreach (TextCollection tc in Event.Resources)
                        tc.Name = "RESOURCES";
                }                
            }

            base.Serialize(stream, encoding);
        }

        #endregion
    }
}
